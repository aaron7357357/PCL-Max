using System;
using System.Collections.Generic;

namespace PCL.Core.App;

/// <summary>
/// 本项目对外的链接集中地。
/// <para>
/// 从上游 fork 之后，代码里散落着大量指向 PCL-CE 的地址，改起来容易漏。
/// 需要改仓库时只动这里的 <see cref="GitHubOwner"/> 和 <see cref="GitHubRepo"/> 两个常量即可，
/// 其余地址都由它们拼出来。
/// </para>
/// </summary>
public static class ProjectLinks
{
    // ─────────────────────────────────────────────────────────────
    //  改成你自己的仓库，这两行是唯一需要动的地方
    // ─────────────────────────────────────────────────────────────

    /// <summary>GitHub 用户名或组织名。</summary>
    public const string GitHubOwner = "aaron7357357";

    /// <summary>源码仓库名（代码、Issue、PR、README 都在这儿）。</summary>
    public const string GitHubRepo = "PCL-Max";

    // ─────────────────────────────────────────────────────────────
    //  OTA 锚点（主服务器 / 备用服务器）
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// OTA 主服务器的组织名。
    /// <para>
    /// 与 <see cref="GitHubOwner"/> 分开写，是为了让「主服务器换成别的账号」这件事
    /// 只需要改这一个常量，不用连带改源码仓的链接。
    /// </para>
    /// <para>
    /// 2026-10 从 <c>Pcl-Lite</c> 组织迁到这里：那个组织被 GitHub 标记隐藏，
    /// 组织下所有仓库对匿名访问全部 404，清单和 Release 一起失联。
    /// 个人账号下的仓库不会因为「组织被标记」而整片消失。
    /// </para>
    /// </summary>
    public const string ServiceOwner = "aaron7357357";

    /// <summary>
    /// OTA 主服务器仓库：更新清单与发布包的主来源。
    /// <para>
    /// 现在就是源码仓本身（<c>PCL-Max</c>）——它是「主服务器」，清单放在它根目录下，
    /// 发布包放在它的 Releases 里。客户端默认只问它。
    /// </para>
    /// </summary>
    public const string ServiceRepo = "PCL-Max";

    // ─────────────────────────────────────────────────────────────
    //  以下由上面两个常量推导，一般不用改
    // ─────────────────────────────────────────────────────────────

    /// <summary>仓库是否已经配置好（占位符没被替换掉就视为未配置）。</summary>
    public static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(GitHubOwner) &&
        !GitHubOwner.Equals("YOUR_GITHUB_NAME", StringComparison.OrdinalIgnoreCase);

    public static string Repository => $"https://github.com/{GitHubOwner}/{GitHubRepo}";

    /// <summary>仓库所有者的主页。</summary>
    public static string OwnerHome => $"https://github.com/{GitHubOwner}";

    /// <summary>发布包下载页。主服务器与备用服务器都放了 Release，这里给主服务器的。</summary>
    public static string Releases => $"{ServiceRepository}/releases";

    public static string LatestRelease => $"{Releases}/latest";

    public static string Issues => $"{Repository}/issues";

    public static string PullRequests => $"{Repository}/pulls";

    public static string NewIssue => $"{Issues}/new/choose";

    public static string Contributors => $"{Repository}/graphs/contributors";

    /// <summary>仓库的简体中文 README（项目说明、支持平台与许可条款都在这里）。</summary>
    public static string Readme => $"{Repository}/blob/main/README.md";

    /// <summary>
    /// Lunar Client 支持的完整说明（物料来源、账号与续期、IPC 支持矩阵、好友与私聊通道、已知限制）。
    /// <para>
    /// Lunar 那一块逻辑变化很快，页面上写死的说明必然过时——所以页面上只留一句概述，
    /// 细节一律指向这份文档，改文档不用发版。
    /// </para>
    /// </summary>
    public static string LunarDocs => $"{Repository}/blob/main/docs/LUNAR.md";

    public static string Wiki => $"{Repository}/wiki";

    /// <summary>
    ///     官网地址。站点源码在仓库的 <c>site/</c> 目录，通过 GitHub Pages 发布。
    /// </summary>
    public static string Website => $"https://{GitHubOwner.ToLowerInvariant()}.github.io/{GitHubRepo}/";

    /// <summary>GitHub API：仓库的 release 列表。</summary>
    public static string ReleasesApi => $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases";

    /// <summary>GitHub API：仓库的 issue 列表。</summary>
    public static string IssuesApi => $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/issues";

    /// <summary>GitHub API：贡献者列表。</summary>
    public static string ContributorsApi => $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/contributors";

    // ─────────────────────────────────────────────────────────────
    //  更新分支
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    ///     清单读哪条分支。
    ///     <para>
    ///         本项目只有一条发布线：清单和发布包全部放在 <c>main</c>。正式版与测试版靠
    ///         Release 上的 <c>prerelease</c> 标记加更新通道（见 <see cref="UpdateChannelLevels" />）区分。
    ///     </para>
    ///     <para>
    ///         以前这里会看「版本后缀是不是 beta」翻到 <c>beta</c> 分支，也支持「测试 OTA」开关强制翻。
    ///         两条路在生产环境都是纯粹的地雷：仓库里只要没有那条分支，检查更新的结果就是
    ///         六条通道全 404，而界面只会显示一句「获取版本信息失败」——真实原因
    ///         （分支不存在）在用户眼里完全不可见。单仓单分支之后没这个问题了，直接恒等 main。
    ///     </para>
    /// </summary>
    public static string ManifestBranch => "main";

    // ─────────────────────────────────────────────────────────────
    //  更新清单：唯一入口，发布仓库换了也只改这里指向的 json
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 更新清单的原始地址（raw.githubusercontent，走 CDN，不吃 API 限流）。
    /// <para>
    /// 老客户端把这个地址硬编码在心里，所以它<b>永远不能变</b>。
    /// 以后要换发布仓库、换组织、换下载通道，都只改这份 json 里的内容，
    /// 老版本依旧能顺着它找到新版本，不会因为仓库搬家而失联。
    /// </para>
    /// </summary>
    public static string UpdateManifest =>
        $"https://raw.githubusercontent.com/{ServiceOwner}/{ServiceRepo}/{ManifestBranch}/update.json";

    /// <summary>更新清单的镜像地址（jsDelivr）。raw.githubusercontent 在国内经常连不上。</summary>
    public static string UpdateManifestMirror =>
        $"https://cdn.jsdelivr.net/gh/{ServiceOwner}/{ServiceRepo}@{ManifestBranch}/update.json";

    /// <summary>
    /// 更新清单的第三条通道：GitHub Contents API。
    /// <para>
    /// raw 与 jsDelivr 都挂的时候靠它——api.github.com 的可达性通常比 raw 好得多，
    /// 代价是吃每小时 60 次的匿名限流额度。只读一个小文件，够用。
    /// </para>
    /// </summary>
    public static string UpdateManifestApi =>
        $"https://api.github.com/repos/{ServiceOwner}/{ServiceRepo}/contents/update.json?ref={ManifestBranch}";

    /// <summary>更新清单仓库的网页地址，用于报错时告诉用户去哪儿看。</summary>
    public static string ServiceRepository => $"https://github.com/{ServiceOwner}/{ServiceRepo}";

    // ─────────────────────────────────────────────────────────────
    //  多源清单 updatev2.json
    // ─────────────────────────────────────────────────────────────

    /// <summary>v1 清单文件名：单指针，老客户端只认这个，<b>永远不能删</b>。</summary>
    public const string ManifestV1FileName = "update.json";

    /// <summary>
    /// v2 清单文件名：多源 + 权重。
    /// <para>
    /// 用<b>另一个文件</b>而不是往 v1 里加字段，是因为老客户端的解析器对形状很严
    /// （顶层对象里出现多个字符串值就判失败）。同一个文件既当单指针又当多源列表，
    /// 必然有一边要妥协。分文件之后两边各读各的，互不影响。
    /// </para>
    /// <para>
    /// 新客户端先试这个文件，404 / 写坏 / 超时都退回 v1。所以发布顺序上，
    /// <b>v2 文件没推上去也不影响任何人</b>——只是退化成单源而已。
    /// </para>
    /// </summary>
    public const string ManifestV2FileName = "updatev2.json";

    // ─────────────────────────────────────────────────────────────
    //  备用锚点：让「锚点本身没了」也有救
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 额外的清单锚点（备用服务器），每项写 <c>owner/repo</c>，该仓库根目录下也要有一份内容相同的
    /// <c>update.json</c>。
    /// <para>
    /// 语义是「主服务器不可用时的切换目标」，不是并列候选：客户端先把主锚点的所有通道试完，
    /// 全部失败之后才按列表顺序问这里。主服务器正常时一分钱额外请求都不发。
    /// </para>
    /// <para>
    /// 为什么需要这个：锚点解决了「发布仓库搬家」，但解决不了「托管锚点的仓库整体出事」。
    /// 2026-10 就发生过一次——组织被 GitHub 标记隐藏，组织下所有仓库对匿名访问全部 404，
    /// 清单的六条通道和 Releases 兜底源一起断，客户端没有任何退路。
    /// </para>
    /// <para>
    /// <b>注意</b>：备用锚点只对「包含这段代码的版本」有效，
    /// 已经装在别人电脑上的旧版本只认主锚点——救不了它们，只能靠把主锚点恢复。
    /// </para>
    /// </summary>
    public static readonly string[] ExtraServiceAnchors = [];

    /// <summary>某个锚点的清单原始地址（分支跟随 <see cref="ManifestBranch"/>）。</summary>
    public static string ManifestRaw(string owner, string repo, string fileName = ManifestV1FileName) =>
        $"https://raw.githubusercontent.com/{owner}/{repo}/{ManifestBranch}/{fileName}";

    /// <summary>某个锚点的 jsDelivr 镜像地址（分支跟随 <see cref="ManifestBranch"/>）。</summary>
    public static string ManifestMirror(string owner, string repo, string fileName = ManifestV1FileName) =>
        $"https://cdn.jsdelivr.net/gh/{owner}/{repo}@{ManifestBranch}/{fileName}";

    /// <summary>某个锚点的 Contents API 地址（用 <c>ref</c> 指定分支，默认分支不一定是我们要的那条）。</summary>
    public static string ManifestApi(string owner, string repo, string fileName = ManifestV1FileName) =>
        $"https://api.github.com/repos/{owner}/{repo}/contents/{fileName}?ref={ManifestBranch}";

    /// <summary>
    /// 所有锚点（主锚点在前，随后是 <see cref="ExtraServiceAnchors"/>）。
    /// 解析不了的行直接跳过，不让一处手误拖垮整个更新。
    /// </summary>
    public static IReadOnlyList<(string Owner, string Repo)> ServiceAnchors()
    {
        var list = new List<(string Owner, string Repo)> { (ServiceOwner, ServiceRepo) };

        foreach (var entry in ExtraServiceAnchors)
        {
            var parts = (entry ?? string.Empty).Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2) continue;
            if (parts[0].Contains(':') || parts[1].Contains(':')) continue;
            if (list.Contains((parts[0], parts[1]))) continue;
            list.Add((parts[0], parts[1]));
        }

        return list;
    }
}
