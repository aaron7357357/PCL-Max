using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PCL.Core.App;
using PCL.Core.App.Localization;

namespace PCL.Pages.PageSetup;

/// <summary>
///     更新说明窗口：版本变化后展示本次改动。
///     <para>
///         内容写在这里而不是从网络拉：更新说明要和这个构建<b>一一对应</b>，
///         从网络拉的话，老版本会被推上将来版本的说明，看起来像是自己已经变了但功能没有。
///         发新版时改这一段即可。
///     </para>
/// </summary>
public partial class WindowUpdateNotes
{
    /// <summary>
    ///     当前构建的说明文案。发版时改这里。
    /// </summary>
    private const string Subtitle = "联机页新增网络状态检测，开房前就能看到自己的 IPv6 情况。";

    /// <summary>
    ///     每条的标题与说明。
    /// </summary>
    private static readonly (string Title, string[] Items)[] Sections =
    [
        ("联机", new[]
        {
            "左栏新增「当前状态」，直接显示本机 IPv6 是否可用。",
            "有公网 IPv6：陶瓦可以直连打洞，延迟低、不限速。",
            "没有公网 IPv6：自动走中继，能用但速度取决于中继节点。",
            "「重新检测网络」可以手动刷新，插拔网线或换网络后点一下即可。"
        }),
        ("顺带修掉的", new[]
        {
            "IPv6 检测改为遍历全部网卡，不再因为第一张网卡没有 IPv6 就误判成不可用。",
            "排除链路本地地址（fe80::）与虚拟网卡，避免把内网地址当成公网可用。"
        })
    ];

    private static readonly Brush HeadingBrush = _Frozen(0xED, 0xF2, 0xF8);
    private static readonly Brush AccentBrush = _Frozen(0x00, 0xB7, 0xFF);
    private static readonly Brush BodyBrush = _Frozen(0xC9, 0xD3, 0xDF);

    private static Brush _Frozen(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }

    public WindowUpdateNotes()
    {
        InitializeComponent();
        LabVersion.Text = ModBase.versionBaseName;
        LabSubtitle.Text = Subtitle;
        BuildNotes();
    }

    private void BuildNotes()
    {
        foreach (var (title, items) in Sections)
        {
            PanNotes.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 17,
                FontWeight = FontWeights.SemiBold,
                Foreground = HeadingBrush,
                Margin = new Thickness(0, 14, 0, 8)
            });

            foreach (var item in items)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 7) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                var dot = new TextBlock
                {
                    Text = "•",
                    FontSize = 14,
                    Foreground = AccentBrush,
                    VerticalAlignment = VerticalAlignment.Top
                };
                var text = new TextBlock
                {
                    Text = item,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 21,
                    Foreground = BodyBrush
                };
                Grid.SetColumn(text, 1);
                row.Children.Add(dot);
                row.Children.Add(text);
                PanNotes.Children.Add(row);
            }
        }
    }

    /// <summary>
    ///     有未读更新说明时弹出。调用方需在 UI 线程。
    /// </summary>
    public static void ShowIfNeeded(Window? owner = null)
    {
        try
        {
            var current = ModBase.versionBaseName;
            if (string.IsNullOrWhiteSpace(current)) return;
            if (Config.Update.LastNotesVersion == current) return;
            if (Config.Update.DismissedNotesVersion == current) return;

            var window = new WindowUpdateNotes { Owner = owner };
            window.ShowDialog();
            Config.Update.LastNotesVersion = current;
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "显示更新说明失败", ModBase.LogLevel.Debug);
        }
    }

    private void BtnOk_Click(object sender, MouseButtonEventArgs e)
    {
        if (CheckDismiss.Checked == true) Config.Update.DismissedNotesVersion = ModBase.versionBaseName;
        Close();
    }

    private void BtnRelease_Click(object sender, MouseButtonEventArgs e)
    {
        try
        {
            ModBase.OpenWebsite(ProjectLinks.Releases);
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "打开发布页失败", ModBase.LogLevel.Debug);
        }
    }
}
