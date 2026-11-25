using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

// 验证 mods.toml 依赖解析：对比「不过滤 mandatory」与「只收 mandatory」的差异。
// 直接读用户真实的 mods 目录，输出两种口径下的缺失统计。
internal static class Program
{
    static void Main(string[] args)
    {
        var modsDir = args.Length > 0 ? args[0] : @"D:\awa\.minecraft\versions\BattleArmory TACZ\mods";
        if (!Directory.Exists(modsDir))
        {
            Console.WriteLine("目录不存在: " + modsDir);
            return;
        }

        var oldDeps = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase); // 文件 → 依赖（旧口径：全收）
        var newDeps = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase); // 新口径：只收 mandatory
        var modIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "minecraft", "java" };

        // 第一遍：收集所有 modId
        foreach (var jar in Directory.EnumerateFiles(modsDir, "*.jar"))
        {
            var meta = ReadMeta(jar);
            if (meta is null) continue;
            if (!string.IsNullOrEmpty(meta.Value.ModId)) modIds.Add(meta.Value.ModId!);
        }

        // 第二遍：解析依赖
        foreach (var jar in Directory.EnumerateFiles(modsDir, "*.jar"))
        {
            var meta = ReadMeta(jar);
            if (meta is null) continue;
            oldDeps[Path.GetFileName(jar)] = meta.Value.AllDeps;
            newDeps[Path.GetFileName(jar)] = meta.Value.MandatoryDeps;
        }

        Console.WriteLine($"模组数: {oldDeps.Count}");
        Console.WriteLine();

        var oldMissing = CountOld(modIds, oldDeps);
        var newMissing = CountNew(modIds, newDeps);

        Console.WriteLine($"旧口径（不过滤 mandatory）缺失: {oldMissing} 条");
        Console.WriteLine($"新口径（只收 mandatory）缺失:   {newMissing} 条");
        Console.WriteLine();

        Console.WriteLine("=== 新口径下仍然缺失的（前 20 条）===");
        var shown = 0;
        foreach (var kv in newDeps)
        {
            foreach (var dep in kv.Value)
            {
                if (modIds.Contains(dep)) continue;
                if (IsPlatform(dep)) continue;
                Console.WriteLine($"  {kv.Key}  →  {dep}");
                if (++shown >= 20) break;
            }
            if (shown >= 20) break;
        }
        if (shown == 0) Console.WriteLine("  （无）");

        Console.WriteLine();
        Console.WriteLine("=== 被新口径排除掉的可选依赖（前 15 条）===");
        var skipped = 0;
        foreach (var kv in oldDeps)
        {
            var mandatory = new HashSet<string>(newDeps[kv.Key], StringComparer.OrdinalIgnoreCase);
            foreach (var dep in kv.Value)
            {
                if (mandatory.Contains(dep)) continue;
                Console.WriteLine($"  {kv.Key}  →  {dep}  (optional)");
                if (++skipped >= 15) break;
            }
            if (skipped >= 15) break;
        }
        if (skipped == 0) Console.WriteLine("  （无）");
    }

    static int CountOld(HashSet<string> ids, Dictionary<string, List<string>> deps)
    {
        var n = 0;
        foreach (var kv in deps)
            foreach (var d in kv.Value)
                if (!ids.Contains(d) && !IsPlatform(d)) n++;
        return n;
    }

    static int CountNew(HashSet<string> ids, Dictionary<string, List<string>> deps)
    {
        var n = 0;
        foreach (var kv in deps)
            foreach (var d in kv.Value)
                if (!ids.Contains(d) && !IsPlatform(d)) n++;
        return n;
    }

    static bool IsPlatform(string id) =>
        id.Equals("forge", StringComparison.OrdinalIgnoreCase) ||
        id.Equals("neoforge", StringComparison.OrdinalIgnoreCase) ||
        id.Equals("fabricloader", StringComparison.OrdinalIgnoreCase) ||
        id.Equals("quilt_loader", StringComparison.OrdinalIgnoreCase) ||
        id.Equals("javafml", StringComparison.OrdinalIgnoreCase) ||
        id.Equals("lowcodefml", StringComparison.OrdinalIgnoreCase) ||
        id.Equals("fml", StringComparison.OrdinalIgnoreCase);

    static (string? ModId, List<string> AllDeps, List<string> MandatoryDeps)? ReadMeta(string jar)
    {
        try
        {
            using var zip = ZipFile.OpenRead(jar);

            // mods.toml（Forge / NeoForge）
            var entry = zip.Entries.FirstOrDefault(e => e.FullName == "META-INF/mods.toml")
                     ?? zip.Entries.FirstOrDefault(e => e.FullName == "META-INF/neoforge.mods.toml");
            if (entry is not null)
            {
                using var sr = new StreamReader(entry.Open());
                return ParseToml(sr.ReadToEnd());
            }

            // fabric.mod.json / quilt.mod.json
            var fj = zip.Entries.FirstOrDefault(e => e.FullName == "fabric.mod.json");
            if (fj is not null)
            {
                using var sr = new StreamReader(fj.Open());
                return ParseFabricJson(sr.ReadToEnd());
            }
        }
        catch { }
        return null;
    }

    static (string? ModId, List<string> AllDeps, List<string> MandatoryDeps) ParseToml(string text)
    {
        string? modId = null;
        var all = new List<string>();
        var mandatory = new List<string>();

        var inDep = false;
        string? pendingId = null;
        var pendingMandatory = true;

        void Flush()
        {
            if (pendingId is null) return;
            all.Add(pendingId);
            if (pendingMandatory) mandatory.Add(pendingId);
            pendingId = null;
            pendingMandatory = true;
        }

        foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;

            if (line.StartsWith("[[dependencies", StringComparison.OrdinalIgnoreCase)) { Flush(); inDep = true; continue; }
            if (line.StartsWith('[') && !line.StartsWith("[[")) { Flush(); inDep = false; continue; }

            var m = Regex.Match(line, @"^([A-Za-z_][\w]*)\s*=\s*""?([^""#]*)""?");
            if (!m.Success) continue;
            var key = m.Groups[1].Value.Trim();
            var val = m.Groups[2].Value.Trim();

            if (inDep)
            {
                if (key.Equals("modId", StringComparison.OrdinalIgnoreCase))
                {
                    if (pendingId is not null) Flush();
                    pendingId = val;
                    pendingMandatory = true;
                }
                else if (key.Equals("mandatory", StringComparison.OrdinalIgnoreCase))
                {
                    pendingMandatory = !val.Equals("false", StringComparison.OrdinalIgnoreCase) && val != "0";
                }
                continue;
            }

            if (key.Equals("modId", StringComparison.OrdinalIgnoreCase)) modId ??= val;
        }
        Flush();
        return (modId, all, mandatory);
    }

    static (string? ModId, List<string> AllDeps, List<string> MandatoryDeps) ParseFabricJson(string text)
    {
        var all = new List<string>();
        var mandatory = new List<string>();
        string? id = null;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(text);
            var root = doc.RootElement;
            if (root.TryGetProperty("id", out var idEl)) id = idEl.GetString();

            if (root.TryGetProperty("depends", out var dep) && dep.ValueKind == System.Text.Json.JsonValueKind.Object)
                foreach (var p in dep.EnumerateObject())
                { all.Add(p.Name); mandatory.Add(p.Name); }

            // recommends / suggests 是可选，只进 all 不进 mandatory
            foreach (var optionalKey in new[] { "recommends", "suggests", "breaks", "conflicts" })
                if (root.TryGetProperty(optionalKey, out var opt) && opt.ValueKind == System.Text.Json.JsonValueKind.Object)
                    foreach (var p in opt.EnumerateObject())
                        all.Add(p.Name);
        }
        catch { }
        return (id, all, mandatory);
    }
}
