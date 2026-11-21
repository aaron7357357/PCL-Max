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
    private const string Subtitle = "修掉任务管理页打不开的问题，更新页也不再显示占位文字。";

    /// <summary>
    ///     每条的标题与说明。
    /// </summary>
    private static readonly (string Title, string[] Items)[] Sections =
    [
        ("修掉的问题", new[]
        {
            "任务管理页打不开：点进去后卡片构造失败，整页空白。原因是动态拼 XAML 时把控件命名空间写成了 PCL，而 MyCard 实际在 PCL.Controls。",
            "更新页显示 by bsk10ww：这是从上游抄来的设计器占位文字，从来没有被真实数据覆盖过。",
            "更新页提示「开发者似乎忘记提供更新摘要」：Release 说明没写 <summary> 标签就取不到摘要，现在改用正文第一段。"
        }),
        ("联机", new[]
        {
            "左栏显示本机 IPv6 状态，开房前就知道能不能直连。",
            "有公网 IPv6 走直连打洞，没有则自动退回中继。"
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
