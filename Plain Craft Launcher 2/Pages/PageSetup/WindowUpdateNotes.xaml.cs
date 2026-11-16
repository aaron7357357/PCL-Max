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
    private const string Subtitle = "底座换成了 PCL_Plus 4.0.1 的源码，并修掉了几个一直在碍事的问题。";

    /// <summary>
    ///     每条的标题与说明。
    /// </summary>
    private static readonly (string Title, string[] Items)[] Sections =
    [
        ("底座重建", new[]
        {
            "整个项目改为在 PCL_Plus 4.0.1 源码上继续开发，不再往老 exe 里注入代码。",
            "换底座之后可以用上 .NET 10 的 WPF 特性，之前被卡住的液态玻璃等效果有了实现条件。",
            "图标换成自绘的 M 标，标题栏显示 PCL_Max 文字。"
        }),
        ("修掉的问题", new[]
        {
            "反馈、软件信息、查看日志、管理四个页面点开就报「未知的设置子页面种类」，现已全部恢复。",
            "检查更新一直失败，原因是更新源指向了一个不存在的仓库；现在指向本项目自己的发布页。",
            "主页性能条在 CPU 跑满时只走到三分之一，现在按进度槽真实宽度换算。",
            "WebP 明明系统支持却被提示缺失，改为检查系统解码器与 libwebp.dll 后再判断。"
        }),
        ("界面调整", new[]
        {
            "主页精简为一张「性能状态」卡：CPU 与内存，1 秒刷新，宽度变化带缓动。",
            "新增顶级「联机」页，把陶瓦联机面板从设置里独立出来。",
            "Lunar Client 入口移除，其余入口全部保留。"
        }),
        ("关于更新", new[]
        {
            "正式版与测试版现在走同一条更新线，靠 Release 上的预发布标记和更新通道区分。",
            "必须使用本版本及以后版本才能自动收到更新，更早的版本无法自动升级。"
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
