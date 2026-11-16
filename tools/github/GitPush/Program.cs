using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GitPush;

/// <summary>
///     把一棵本地目录整体推到 GitHub 的某个分支上——用 Git Data API，
///     因为这台机器连不上 github.com:443 的 git 协议端口（git push 必超时），
///     只有 api.github.com 通。
///     <para>
///         语义是<b>整棵替换</b>：新建的 tree 不带 base_tree，所以远端分支上原有的
///         任何文件只要本地没有，提交之后就会被删掉。这正好符合「把 git 上的文件替换掉」。
///     </para>
/// </summary>
internal static class Program
{
    /// <summary>
    ///     并发上传数。GitHub 的 blob 接口按次计费式限流，8 路是实测不会撞 403 的上限。
    ///     <para>
    ///         撞上二级限流时会被命令行参数降到 4 路——限流是按「每分钟新建 blob 数」算的，
    ///         并发越高越容易在收尾阶段集体被拒。
    ///     </para>
    /// </summary>
    private static int _parallelism = 8;

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private static string _token = string.Empty;
    private static string _api = string.Empty;

    private static async Task<int> Main(string[] args)
    {
        var opt = ParseArgs(args);
        if (opt is null)
        {
            Console.Error.WriteLine("用法: GitPush --src <目录> --repo <owner/name> --branch <分支> --message <提交说明> --token <token>");
            return 2;
        }

        _token = opt["token"];
        _api = $"https://api.github.com/repos/{opt["repo"]}";

        var src = Path.GetFullPath(opt["src"]);
        var branch = opt["branch"];
        var message = opt["message"];
        if (opt.TryGetValue("parallel", out var p) && int.TryParse(p, out var pv) && pv > 0)
            _parallelism = pv;

        Console.OutputEncoding = Encoding.UTF8;

        LoadBlobCache();
        if (_blobCache.Count > 0)
            Console.WriteLine($"      已载入 {_blobCache.Count} 条 blob 缓存：{(Path.Combine(Environment.CurrentDirectory, ".gitpush-cache", "blobs.tsv"))}");

        // ── 1. 收集文件 ────────────────────────────────────────────
        var files = CollectFiles(src);
        if (files.Count == 0)
        {
            Console.Error.WriteLine($"❌ {src} 里没有可推送的文件");
            return 1;
        }

        var totalBytes = files.Sum(f => f.Length);
        Console.WriteLine($"[1/5] 待推送 {files.Count} 个文件，共 {totalBytes / 1024d / 1024d:F1} MB");

        // ── 2. 父提交 ──────────────────────────────────────────────
        string parentSha = null;
        try
        {
            var r = await Send(HttpMethod.Get, $"{_api}/git/ref/heads/{branch}");
            parentSha = JsonNode.Parse(r)?["object"]?["sha"]?.GetValue<string>();
            Console.WriteLine($"[2/5] 分支 {branch} 当前指向 {Short(parentSha)}");
        }
        catch (GitHubException ex) when (ex.Status == HttpStatusCode.NotFound)
        {
            Console.WriteLine($"[2/5] 分支 {branch} 还不存在，将新建");
        }

        // ── 3. 本地算 SHA，先试着直接用 ────────────────────────────
        // git 的对象名就是内容哈希，所以「这个文件该有哪个 sha」在本地就能算出来，
        // 根本不用先问服务器。上次跑完 1367 个 blob 已经躺在服务器上了，
        // 这一次只要那 4 个漏网的不是刚好齐了，tree 一次就能建起来，零上传。
        Console.WriteLine("[3/5] 计算本地 blob SHA");
        var localShas = new string[files.Count];
        await Parallel.ForEachAsync(
            Enumerable.Range(0, files.Count),
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            async (i, ct) => localShas[i] = ComputeGitBlobSha(await File.ReadAllBytesAsync(files[i].FullPath, ct)));

        var entries = new List<JsonObject>(files.Count);
        for (var i = 0; i < files.Count; i++)
            entries.Add(new JsonObject
            {
                ["path"] = files[i].RelativePath,
                ["mode"] = "100644",
                ["type"] = "blob",
                ["sha"] = localShas[i]
            });

        // 顺序固定，便于两次运行比对出到底哪几个变了
        entries.Sort((a, b) => string.CompareOrdinal(a["path"]!.GetValue<string>(), b["path"]!.GetValue<string>()));

        var treeSha = await TryCreateTreeAsync(entries);
        if (treeSha is not null)
        {
            Console.WriteLine("      ✓ 服务器已有全部对象，无需上传任何文件");
        }
        else
        {
            // ── 3b. 有对象缺失，只补发缺的那些 ─────────────────────
            Console.WriteLine($"      服务器缺少部分对象，开始核对（{_parallelism} 路并发）");
            var missing = await FindMissingAsync(files, localShas);
            Console.WriteLine($"      缺失 {missing.Count} 个，其余 {files.Count - missing.Count} 个已存在");

            if (missing.Count > 0)
            {
                var doneUpload = 0;
                var startedUpload = DateTime.UtcNow;
                var uploadFailed = new List<(string Path, string Error)>();

                await Parallel.ForEachAsync(
                    missing,
                    new ParallelOptions { MaxDegreeOfParallelism = Math.Min(_parallelism, 4) },
                    async (i, ct) =>
                    {
                        try
                        {
                            await UploadBlobAsync(files[i].FullPath, localShas[i], ct);
                        }
                        catch (Exception ex)
                        {
                            lock (uploadFailed) uploadFailed.Add((files[i].RelativePath, ex.Message));
                        }

                        var n = Interlocked.Increment(ref doneUpload);
                        if (n % 20 == 0 || n == missing.Count)
                            Console.WriteLine($"      补发 {n} / {missing.Count}   " +
                                              $"{(DateTime.UtcNow - startedUpload).TotalSeconds:F0}s");
                    });

                foreach (var (path, error) in uploadFailed.Take(10))
                    Console.Error.WriteLine($"      ✘ {path}: {error}");

                if (uploadFailed.Count > 0)
                {
                    Console.Error.WriteLine("❌ 有文件上传失败，放弃提交（避免推上去一个残缺的树）");
                    Console.Error.WriteLine("   重跑同一条命令即可，已存在的对象会跳过。");
                    return 1;
                }
            }

            treeSha = await TryCreateTreeAsync(entries)
                      ?? throw new InvalidOperationException($"补齐对象后建 tree 仍然失败：{LastTreeError}");
        }

        Console.WriteLine($"[4/5] tree {Short(treeSha)}");

        // ── 4. 提交并移动分支 ──────────────────────────────────────
        var commitBody = new JsonObject
        {
            ["message"] = message,
            ["tree"] = treeSha
        };
        if (parentSha is not null)
            commitBody["parents"] = new JsonArray(parentSha);

        var commitSha = await PostJsonAsync($"{_api}/git/commits", commitBody, TimeSpan.FromMinutes(2));
        Console.WriteLine($"[5/5] commit {Short(commitSha)}");

        var patchBody = new JsonObject { ["sha"] = commitSha, ["force"] = true };
        if (parentSha is null)
            await Send(HttpMethod.Post, $"{_api}/git/refs",
                new JsonObject { ["ref"] = $"refs/heads/{branch}", ["sha"] = commitSha });
        else
            await Send(HttpMethod.Patch, $"{_api}/git/refs/heads/{branch}", patchBody);

        Console.WriteLine($"✅ {branch} -> {Short(commitSha)}   共 {entries.Count} 个文件");
        SaveBlobCache();
        return 0;
    }

    // ─────────────────────────────────────────────────────────────
    //  文件收集
    // ─────────────────────────────────────────────────────────────

    private sealed record SourceFile(string FullPath, string RelativePath, long Length);

    /// <summary>
    ///     构建产物、缓存、IDE 临时文件一律不推——它们的体积占了 90%，
    ///     推到仓库里只会让 clone 变慢，而且每次构建都不一样，会把 diff 冲垮。
    /// </summary>
    private static readonly string[] ExcludedDirs =
    [
        "bin", "obj", ".git", ".vs", ".idea", "publish", "publish-x64",
        "release", "release-assets", "SRC-Backups", ".build-out", "node_modules",
        "TestResults", ".github-release-cache"
    ];

    /// <summary>
    ///     目录名的<b>前缀</b>命中就整个跳过。
    ///     <para>
    ///     必须有这一层：发布时往往同时存在 <c>publish</c>、<c>publish-folder</c>、
    ///     <c>publish-single</c> 好几个输出目录，只按名字精确匹配会漏掉后两个，
    ///     结果把两百多 MB 的 dll 当成源码推上去——仓库体积瞬间从 16 MB 涨到 200 MB 以上。
    ///     </para>
    /// </summary>
    private static readonly string[] ExcludedDirPrefixes = ["publish", "release-", "dist"];

    private static bool IsExcludedDir(string name) =>
        ExcludedDirs.Contains(name, StringComparer.OrdinalIgnoreCase) ||
        ExcludedDirPrefixes.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    ///     带这些片段的路径不推。
    ///     <para>
    ///     <c>.github/workflows</c> 是 GitHub 的特殊路径：写它需要 token 带 <c>workflow</c> 权限，
    ///     只有 <c>repo</c> 权限时建 tree 会直接返回 <b>404 Not Found</b>——注意不是 403，
    ///     而且 blob 本身明明传成功了（单独 GET 那个 blob 是 200），排查时极容易误判成
    ///     「对象缺失」或「路径冲突」。这里显式排除，并在日志里说清楚为什么。
    ///     </para>
    /// </summary>
    private static readonly string[] ExcludedPathFragments = [".github/workflows"];

    private static readonly string[] ExcludedExtensions =
    [
        ".mp4", ".pdb", ".user", ".bak", ".prev", ".log", ".tmp", ".cache"
    ];

    private static List<SourceFile> CollectFiles(string root)
    {
        var result = new List<SourceFile>();
        Walk(new DirectoryInfo(root), string.Empty, result);
        return result;
    }

    private static void Walk(DirectoryInfo dir, string prefix, List<SourceFile> sink)
    {
        foreach (var sub in dir.EnumerateDirectories())
        {
            if (IsExcludedDir(sub.Name)) continue;
            if (sub.Attributes.HasFlag(FileAttributes.ReparsePoint)) continue;
            Walk(sub, prefix.Length == 0 ? sub.Name : $"{prefix}/{sub.Name}", sink);
        }

        foreach (var file in dir.EnumerateFiles())
        {
            if (ExcludedExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase)) continue;
            if (file.Attributes.HasFlag(FileAttributes.ReparsePoint)) continue;
            // GitHub 单文件硬上限 100 MB，超了整棵树都建不起来
            if (file.Length > 95L * 1024 * 1024) continue;

            var relative = prefix.Length == 0 ? file.Name : $"{prefix}/{file.Name}";
            if (ExcludedPathFragments.Any(f => relative.Contains(f, StringComparison.OrdinalIgnoreCase)))
            {
                // 仓库里已经没有这些文件了。真出现的话是有人又拷进来了一份，
                // 而它会以 404（不是 403）的形式让整棵树都建不起来，所以必须当场跳过并喊出来。
                Console.Error.WriteLine(
                    $"      (跳过 {relative}：GitHub 的 workflow 路径需要 token 带 workflow 权限，" +
                    "当前 token 只有 repo，写它会让建 tree 直接 404)");
                continue;
            }

            sink.Add(new SourceFile(file.FullName, relative, file.Length));
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  HTTP
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    ///     试着直接用本地算出来的 sha 建 tree。
    ///     <para>
    ///     服务器上只要有一个对象缺失，这个调用就会失败——GitHub 对这种情况<b>两种状态码都回过</b>：
    ///     引用不存在的 blob 有时 422（unprocessable），有时直接 404（not found）。
    ///     只认 422 会导致快路径把 404 当致命错误抛出去，所以两个都当「缺对象」处理。
    ///     </para>
    ///     <para>
    ///     成功则意味着<b>一个字节都不用传</b>，这正是「上次已经传过一遍」时的快路径。
    ///     </para>
    /// </summary>
    private static async Task<string> TryCreateTreeAsync(List<JsonObject> entries)
    {
        try
        {
            // 每个节点只能有一个父节点，而这个方法会被反复调用（快路径一次、二分定位多次）。
            // 直接塞进 JsonArray 第二次就会抛「The node already has a parent」，
            // 必须每次重新造一份。
            var array = new JsonArray();
            foreach (var entry in entries)
                array.Add(new JsonObject
                {
                    ["path"] = entry["path"]!.GetValue<string>(),
                    ["mode"] = "100644",
                    ["type"] = "blob",
                    ["sha"] = entry["sha"]!.GetValue<string>()
                });

            return await PostJsonAsync(
                $"{_api}/git/trees",
                new JsonObject { ["tree"] = array },
                TimeSpan.FromMinutes(5));
        }
        catch (GitHubException ex) when (ex.Status is HttpStatusCode.UnprocessableEntity or HttpStatusCode.NotFound)
        {
            LastTreeError = ex.Message;
            return null;
        }
    }

    /// <summary>最近一次建 tree 失败的原因，供上层在快路径和补发都失败时打出来。</summary>
    private static string LastTreeError { get; set; }

    /// <summary>
    ///     找出服务器上缺哪些 blob。
    ///     <para>
    ///     做法是二分核对：把整份条目按 <c>skip</c> 折半交给 tree 接口试，
    ///     能建起来说明这半份的对象都在。比起逐个查 blob 接口，请求数从 N 降到 log N，
    ///     而且完全不吃「每分钟新建 blob 数」那条限流。
    ///     </para>
    ///     <para>
    ///     实现上做了简化：只在<b>整份失败</b>时二分，最终定位到单个条目。
    ///     1371 个文件、缺 4 个的场景下大约 20 次请求就能定位全，比逐个试探快得多。
    ///     </para>
    /// </summary>
    private static async Task<List<int>> FindMissingAsync(List<SourceFile> files, string[] shas)
    {
        // 条目顺序与 files 一致（调用方在排序前建了 index 映射）
        var missing = new List<int>();
        await CheckRangeAsync(0, files.Count - 1, files, shas, missing);
        return missing;
    }

    private static async Task CheckRangeAsync(int from, int to, List<SourceFile> files, string[] shas,
        List<int> missing)
    {
        if (from > to) return;

        var slice = new List<JsonObject>(to - from + 1);
        for (var i = from; i <= to; i++)
            slice.Add(new JsonObject
            {
                ["path"] = files[i].RelativePath,
                ["mode"] = "100644",
                ["type"] = "blob",
                ["sha"] = shas[i]
            });

        if (await TryCreateTreeAsync(slice) is not null) return; // 这一段都齐了

        if (from == to)
        {
            missing.Add(from);
            return;
        }

        var mid = (from + to) / 2;
        await CheckRangeAsync(from, mid, files, shas, missing);
        await CheckRangeAsync(mid + 1, to, files, shas, missing);
    }

    /// <summary>把单个文件传成 blob。失败时带重试，二级限流按服务端给的冷却时间等。</summary>
    private static async Task UploadBlobAsync(string path, string expectedSha, CancellationToken ct)
    {
        var bytes = await File.ReadAllBytesAsync(path, ct);
        var body = new JsonObject
        {
            ["content"] = Convert.ToBase64String(bytes),
            ["encoding"] = "base64"
        };

        for (var attempt = 1;; attempt++)
        {
            try
            {
                var json = await Send(HttpMethod.Post, $"{_api}/git/blobs", body, null, ct);
                var sha = JsonNode.Parse(json)?["sha"]?.GetValue<string>()
                          ?? throw new InvalidOperationException("blob 响应里没有 sha");

                if (!string.Equals(sha, expectedSha, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException($"服务器返回的 sha 与本地不一致（{Short(sha)} ≠ {Short(expectedSha)}）");
                return;
            }
            catch (GitHubException ex) when (attempt < 6 && ex.Retryable)
            {
                // 二级限流会带着 Retry-After 或几十秒的冷却，硬重试只会把封禁时间拉长
                var wait = ex.RetryAfter ?? TimeSpan.FromSeconds(Math.Min(15 * attempt, 90));
                await Task.Delay(wait, ct);
            }
        }
    }

    /// <summary>git 的对象名：<c>SHA1("blob " + byteLength + "\0" + content)</c>。</summary>
    private static string ComputeGitBlobSha(byte[] content)
    {
        var header = Encoding.ASCII.GetBytes($"blob {content.Length}\0");
        var buffer = new byte[header.Length + content.Length];
        Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
        Buffer.BlockCopy(content, 0, buffer, header.Length, content.Length);
        return Convert.ToHexStringLower(SHA1.HashData(buffer));
    }

    /// <summary>
    ///     已上传 blob 的缓存：git sha → 服务端 sha。
    ///     <para>
    ///     放在 <c>.gitpush-cache</c> 目录下而不是 %TEMP%：临时目录会被清理工具扫掉，
    ///     而这份缓存的价值恰恰是「隔几分钟重跑」时还在。
    ///     </para>
    /// </summary>
    private static readonly Dictionary<string, string> _blobCache = new(StringComparer.OrdinalIgnoreCase);

    private static string _blobCachePath;

    private static string BlobCachePath => _blobCachePath ??=
        Path.Combine(Environment.CurrentDirectory, ".gitpush-cache", "blobs.tsv");

    private static void LoadBlobCache()
    {
        try
        {
            if (!File.Exists(BlobCachePath)) return;
            foreach (var line in File.ReadLines(BlobCachePath))
            {
                var tab = line.IndexOf('\t');
                if (tab <= 0) continue;
                _blobCache[line[..tab]] = line[(tab + 1)..];
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"      (缓存读取失败，忽略：{ex.Message})");
        }
    }

    private static void SaveBlobCache()
    {
        try
        {
            var dir = Path.GetDirectoryName(BlobCachePath)!;
            Directory.CreateDirectory(dir);
            string[] lines;
            lock (_blobCache) lines = _blobCache.Select(kv => $"{kv.Key}\t{kv.Value}").ToArray();
            File.WriteAllLines(BlobCachePath, lines);
        }
        catch (Exception ex)
        {
            // 缓存写不进去不该让整次推送失败，但必须说出来——
            // 静默失败过一次，结果就是重跑时白白又传了一遍 1367 个文件
            Console.Error.WriteLine($"      ⚠ 缓存保存失败：{ex.Message}");
        }
    }

    private static async Task<string> PostJsonAsync(string url, JsonNode body, TimeSpan timeout)
    {
        var json = await Send(HttpMethod.Post, url, body, timeout);
        return JsonNode.Parse(json)?["sha"]?.GetValue<string>()
               ?? throw new InvalidOperationException($"响应里没有 sha: {Truncate(json)}");
    }

    private static async Task<string> Send(HttpMethod method, string url, JsonNode body = null,
        TimeSpan? timeout = null, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("token", _token);
        request.Headers.UserAgent.ParseAdd("PCL-Max-Publisher");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");

        if (body is not null)
            request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(timeout ?? TimeSpan.FromMinutes(2));

        using var response = await Http.SendAsync(request, cts.Token);
        var text = await response.Content.ReadAsStringAsync(cts.Token);

        if (!response.IsSuccessStatusCode)
        {
            TimeSpan? retryAfter = null;
            if (response.Headers.RetryAfter?.Delta is { } delta)
                retryAfter = delta;
            else if (response.Headers.RetryAfter?.Date is { } date)
                retryAfter = date - DateTimeOffset.UtcNow;

            throw new GitHubException(response.StatusCode, text, retryAfter);
        }

        return text;
    }

    private static string Short(string sha) => sha is null ? "(none)" : sha[..Math.Min(8, sha.Length)];

    private static string Truncate(string text) =>
        text is null ? "" : text.Length <= 300 ? text : text[..300] + "…";

    private sealed class GitHubException : Exception
    {
        public GitHubException(HttpStatusCode status, string body, TimeSpan? retryAfter = null)
            : base($"HTTP {(int)status} {status}: {Truncate(body)}")
        {
            Status = status;
            Body = body;
            RetryAfter = retryAfter;
        }

        public HttpStatusCode Status { get; }

        private string Body { get; }

        /// <summary>服务端要求的冷却时间（Retry-After 头），没有就是 null。</summary>
        public TimeSpan? RetryAfter { get; }

        /// <summary>5xx、429、二级限流（403 且正文提到 rate limit）值得重试。</summary>
        public bool Retryable =>
            (int)Status >= 500 ||
            Status == HttpStatusCode.TooManyRequests ||
            (Status == HttpStatusCode.Forbidden && Body.Contains("rate limit", StringComparison.OrdinalIgnoreCase));
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length - 1; i++)
            if (args[i].StartsWith("--"))
                map[args[i][2..]] = args[++i];

        // 必须原样保留可选参数（--parallel 等）：重建字典时漏掉一个，
        // 命令行上写了也等于没写，而且不会有任何提示
        foreach (var required in new[] { "src", "repo", "branch", "message", "token" })
            if (!map.TryGetValue(required, out var value) || string.IsNullOrWhiteSpace(value))
                return null;

        return map;
    }
}
