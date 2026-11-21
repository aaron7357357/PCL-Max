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
    private const string Subtitle = "新增官网与日志分析工具，软件更新页可以直接打开官网。";

    /// <summary>
    ///     每条的标题与说明。
    /// </summary>
    private static readonly (string Title, string[] Items)[] Sections =
    [
        ("官网", new[]
        {
            "官网已上线：aaron7357357.github.io/PCL-Max",
            "软件更新页新增「官网」按钮，点一下就能打开。",
            "站内有下载列表、服务器状态，以及一个日志分析工具。"
        }),
        ("日志分析", new[]
        {
            "启动出问题时，把日志拖进官网的日志分析页，会直接告诉你哪里出了问题。",
            "全部在浏览器本地分析，日志不会上传到任何服务器。",
            "能识别崩溃、缺 .NET 运行时、单例锁冲突、界面加载失败、更新失败、网络错误、Java 问题、内存与磁盘不足等常见情况。"
        }),
        ("顺带修的", new[]
        {
            "启动卡顿约 1 秒：硬件信息查询改为后台执行并只取需要的字段。",
            "GPU 使用率之前不显示：性能计数器需要跨刷新复用实例才能取到值。"
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
