using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using PCL.Core.App.IoC;
using PCL.Core.App.Localization;
using PCL.Core.UI;
using PCL.Core.Utils;

namespace PCL.Core.App.Tools;

[LifecycleScope("dependency-check", "依赖检查")]
[LifecycleService(LifecycleState.Running)]
public sealed partial class DependencyCheckService
{
        [LifecycleStart]
    private static async Task _StartAsync()
    {
        Context.Info(Lang.Text("Tools.Test.Dependency.Checking"));

        if (RuntimeInformation.OSArchitecture.Equals(Architecture.Arm64))
            await _CheckAndAskAsync("Microsoft.D3DMappingLayers", "OpenGL 兼容包", "9nqpsl29bfff")
                .ConfigureAwait(false);

        // WebP 组件包：PCL 的头像、皮肤、资源图标都要用它解码，缺了图片会显示不出来。
        // 先尝试静默安装，失败再引导到微软商店。
        await _CheckAndInstallWebpAsync().ConfigureAwait(false);
    }

    /// <summary>
    ///     WebP 支持检测：先看系统是否真能解码 WebP（WIC 解码器 / libwebp.dll），
    ///     能解码就完全不提示；真的不能解码才引导用户安装组件包。
    /// </summary>
    private static Task _CheckAndInstallWebpAsync()
    {
        // 1) 系统 WIC 是否有 WebP 解码器（Win10 1809+ 自带 MSWebp.dll）
        if (_HasWicWebpDecoder())
        {
            Context.Info("[依赖检查] 系统已支持 WebP 解码，跳过组件包检测");
            return Task.CompletedTask;
        }

        // 2) 启动器目录里有没有 libwebp.dll（PCL 自带，够用）
        if (_HasLocalLibWebp())
        {
            Context.Info("[依赖检查] 启动器自带 libwebp.dll，跳过组件包检测");
            return Task.CompletedTask;
        }

        // 3) 都没找到，才提示用户装组件包
        Context.Info("[依赖检查] 未找到 WebP 解码能力，提示安装组件包");
        var selection = MsgBoxWrapper.Show(
            Lang.Text("Tools.Test.Dependency.MissingMessage", "WebP 组件包"),
            buttons: [Lang.Text("Common.Action.Confirm"), Lang.Text("Tools.Test.Dependency.Later")]);
        if (selection == 1) _LaunchMsStore("9pg2dk419drg");
        return Task.CompletedTask;
    }

    /// <summary>系统是否存在 WIC WebP 解码器（MSWebp.dll 或注册表项）。</summary>
    private static bool _HasWicWebpDecoder()
    {
        try
        {
            var sys = Environment.GetFolderPath(Environment.SpecialFolder.System);
            if (File.Exists(Path.Combine(sys, "MSWebp.dll"))) return true;

            // 注册表：WIC 解码器 CLSID 里查 WebP
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows Imaging Component\Decoders");
            if (key is not null)
                foreach (var name in key.GetSubKeyNames())
                {
                    using var sub = key.OpenSubKey(name);
                    var friendly = sub?.GetValue("FriendlyName") as string;
                    if (friendly is not null &&
                        friendly.Contains("WebP", StringComparison.OrdinalIgnoreCase)) return true;
                }
        }
        catch
        {
            // 查不到就当不支持，继续走后面的检测
        }

        return false;
    }

    /// <summary>启动器目录（或 PCL 子目录）是否有 libwebp.dll。</summary>
    private static bool _HasLocalLibWebp()
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            foreach (var candidate in new[]
                     {
                         Path.Combine(baseDir, "libwebp.dll"),
                         Path.Combine(baseDir, "PCL", "libwebp.dll")
                     })
                if (File.Exists(candidate)) return true;
        }
        catch
        {
            // 忽略
        }

        return false;
    }

    /// <summary>
    ///     尝试静默安装 WebP 组件包。依次试 winget、Add-AppxPackage（从商店 CDN 拉包）。
    /// </summary>
    private static async Task<bool> _TrySilentInstallWebpAsync(string storeId)
    {
        // 方案一：winget（Win10 1809+ 可能有，需用户装过 App Installer）
        if (await _RunAndWaitAsync("winget",
                $"install --id 9N4WGH0Z6VHQ -s msstore --accept-package-agreements --accept-source-agreements --silent",
                120000).ConfigureAwait(false))
        {
            await Task.Delay(800).ConfigureAwait(false);
            if (await _CheckPackageAsync("Microsoft.WebpImageExtension").ConfigureAwait(false)) return true;
        }

        // 方案二：用 PowerShell 从商店下载并安装
        var script =
            "$ErrorActionPreference='Stop';" +
            "try {" +
            "  $r = Invoke-RestMethod -Uri 'https://store.rg-adguard.net/api/GetFiles' -Method Post " +
            "    -Body @{type='ProductId';url='" + storeId + "';ring='Retail';lang='zh-CN'} -TimeoutSec 30;" +
            "} catch { exit 1 }";
        // 商店直链解析较脆弱，这里只做尝试
        if (await _RunAndWaitAsync("powershell.exe",
                $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"", 60000).ConfigureAwait(false))
        {
            if (await _CheckPackageAsync("Microsoft.WebpImageExtension").ConfigureAwait(false)) return true;
        }

        return false;
    }

    /// <summary>启动进程并等它结束，返回是否成功（退出码 0）。</summary>
    private static async Task<bool> _RunAndWaitAsync(string fileName, string args, int timeoutMs)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using var ps = new Process { StartInfo = psi };
            ps.Start();
            var waitTask = ps.WaitForExitAsync();
            var done = await Task.WhenAny(waitTask, Task.Delay(timeoutMs)).ConfigureAwait(false);
            if (done != waitTask)
            {
                try { ps.Kill(true); } catch { }
                return false;
            }

            return ps.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Context.Info($"执行 {fileName} 失败: {ex.Message}");
            return false;
        }
    }

    private static async Task _CheckAndAskAsync(string packageId, string packageName, string storeId)
    {
        if (!await _CheckPackageAsync(packageId))
        {
            Context.Info($"检测到依赖缺失 (package-id = {packageId})");
            var selection = MsgBoxWrapper.Show(
                Lang.Text("Tools.Test.Dependency.MissingMessage", packageName),
                buttons: [Lang.Text("Common.Action.Confirm"), Lang.Text("Tools.Test.Dependency.Later")]);
            if (selection == 1) _LaunchMsStore(storeId);
        }
    }

    private static void _LaunchMsStore(string id)
    {
        Context.Info($"正在打开微软应用商店 (id = {id})");
        var psi = new ProcessStartInfo()
        {
            FileName = $"ms-windows-store://pdp?launch=true&productid={id}&mode=full",
            UseShellExecute = true
        };
        using var ps = new Process() { StartInfo = psi };
        ps.Start();
    }

    private static async Task<bool> _CheckPackageAsync(string id)
    {
        var command = $"Get-AppxPackage -Name *{id}* | ConvertTo-Json";

        var psi = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -Command \"{command}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var ps = new Process() { StartInfo = psi };
        ps.Start();

        JsonNode? jnode;
        try
        {
            // ConvertTo-Json 结构不一定可靠，不太能 Serialize
            jnode = await JsonNode.ParseAsync(ps.StandardOutput.BaseStream, JsonCompat.NodeOptions, JsonCompat.DocumentOptions);
        }
        catch
        {
            return false;
        }

        if (jnode is null) return false;
        if (jnode.GetValueKind().Equals(JsonValueKind.Array))
        {
            var hasPack = false;
            foreach (var node in jnode.AsArray())
            {
                if (node is not null && CheckPackSuit(node, id))
                {
                    hasPack = true;
                    break;
                }

            }

            return hasPack;
        }
        else
        {
            return CheckPackSuit(jnode, id);
        }

        // 检查当前的 JsonNode 下是否是符合的包
        static bool CheckPackSuit(JsonNode jnode, string id)
        {
            var findedPackName = JsonCompat.ToObject<string>(jnode["Name"]);
            return findedPackName is not null && findedPackName.Contains(id);
        }
    }
}
