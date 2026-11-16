using System;
using System.Diagnostics;
using PCL.Core.App;
using PCL.Core.App.Localization;

namespace PCL.Modules.Event;

/// <summary>
///     PCL_Max 交流群。
///     <para>
///         群号与加群链接写在这一个地方，「设置 → 软件信息」的社区卡片只读这里，改群号只改这里。
///     </para>
///     <para>
///         用 QQ 官方后台生成的短链 <c>qm.qq.com/q/xxx</c>，浏览器打开即可拉起 QQ 加群，
///         不依赖本机注册 <c>mqqapi://</c> 协议——那个协议在没装 QQ 或系统被清理过协议关联的
///         机器上会直接抛异常，而且只弹名片、不能直接进群。
///     </para>
/// </summary>
public static class QqGroup
{
    /// <summary>群号（用于展示与复制兜底）。</summary>
    public const string Number = "1135547472";

    /// <summary>
    ///     群名，展示在群号旁边。留空则只显示群号。
    /// </summary>
    public const string Name = "PCL_Max交流群";

    /// <summary>QQ 官方加群短链；没有短链时留空，点击改为复制群号。</summary>
    public const string InviteUrl = "";

    /// <summary>
    ///     用默认浏览器打开加群链接；没有短链或浏览器起不来时退化为复制群号并提示。
    /// </summary>
    public static void Open()
    {
        if (string.IsNullOrWhiteSpace(InviteUrl))
        {
            ModBase.ClipboardSet(Number, false);
            HintService.Hint(Lang.Text("Community.Qq.Copied", Number));
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(InviteUrl) { UseShellExecute = true });
            HintService.Hint(Lang.Text("Community.Qq.Opening"));
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "打开加群链接失败，已改为复制群号", ModBase.LogLevel.Debug);
            ModBase.ClipboardSet(Number, false);
            HintService.Hint(Lang.Text("Community.Qq.Copied", Number));
        }
    }
}
