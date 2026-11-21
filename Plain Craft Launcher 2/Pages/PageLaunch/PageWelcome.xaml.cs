using System;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using PCL.Core.App;
using PCL.Core.App.Localization;
using PCL.Core.IO.Net.Consent;

namespace PCL.Pages.PageLaunch;

/// <summary>
/// PCL_Max 欢迎页。
/// <para>
/// 静态文案按语言分分支存放，加载时由 <see cref="HomepageLanguageApi"/> 按当前 UI 语言过滤——
/// 与自定义主页（Custom.xaml）走的是同一套语言协议，所以欢迎页本身就是该协议的第一个内置示例。
/// 卡片标题、按钮与状态行等需要随语言即时刷新的部分沿用 DynamicResource / <see cref="Lang.Text"/>。
/// </para>
/// <para>
/// 只使用内置的 <see cref="MyCard"/> / <see cref="MyButton"/> 等控件，因此视觉与其余页面一致。
/// </para>
/// </summary>
public partial class PageWelcome
{
    private TextBlock? _networkStatus;
    private TextBlock? _privacyStatus;

    public PageWelcome()
    {
        InitializeComponent();
        Loaded += (_, _) => Build();
        // 切换语言后整体重建，让 lang: 分支重新按新语言过滤。
        // 注意：必须用静态方法组而不是 static lambda，否则 WeakLanguageChanged 会因
        // 委托带有 Target 而抛 ArgumentException（详见 WeakLanguageChanged.Add 的注释）。
        WeakLanguageChanged.Add(this, OnLanguageChanged);
    }

    private static void OnLanguageChanged(PageWelcome page) => ModBase.RunInUi(page.Build);

    /// <summary>
    ///     构建（或重建）欢迎页内容并应用当前语言的分支过滤。
    /// </summary>
    /// <summary>
    ///     是否在主页显示彩蛋卡片。
    ///     <para>
    ///         2.0 不显示，2.1 起显示；只改这一个常量即可，卡片本身与构建顺序都保持不变。
    ///     </para>
    /// </summary>
    private const bool EnableEasterEgg = false;

    private void Build()
    {
        PanRoot.Children.Clear();
        // PCL_Max：主页只保留「上次游玩」和「性能状态」。
        // 「启动时检查模组依赖」开关移到「设置 → 启动」里；社区卡片从主页移除。
        // 彩蛋卡片放在性能卡片之上：与性能同款 MyCard，只是没有按钮
        if (EnableEasterEgg) PanRoot.Children.Add(_BuildEasterEggCard());
        PanRoot.Children.Add(_BuildHero());
        // 主页语言 API：同一父容器下只保留命中当前语言的分支
        HomepageLanguageApi.ApplyToTree(this, LocalizationService.CurrentLanguage.Code);
    }

    /// <summary>
    ///     启动选项卡片：与启动日志卡同样式的轻量开关区。
    /// </summary>
    private MyCard _BuildLaunchOptionsCard()
    {
        var card = new MyCard { Margin = new Thickness(0, 0, 0, 15), CanSwap = false };

        var panel = new StackPanel { Margin = new Thickness(20, 14, 20, 14) };

        var check = new MyCheckBox
        {
            Text = Lang.Text("Welcome.Options.CheckDependency"),
            Margin = new Thickness(0, 0, 0, 0)
        };
        check.SetChecked(Config.Launch.ModDependencyCheck, false);
        check.Change += (_, value) => Config.Launch.ModDependencyCheck = value;
        panel.Children.Add(check);

        card.Children.Add(panel);
        return card;
    }

    #region 卡片

    #region 上次游玩
    /// <summary>
    ///     主页最上方的「上次游玩」卡片：显示当前选中的实例，并提供一键继续。
    ///     <para>
    ///         没有选中实例时给出空态提示，而不是把整张卡片藏起来，避免主页顶部突然塌陷。
    ///     </para>
    /// </summary>
    private MyCard _BuildRecentCard()
    {
        var card = new MyCard
        {
            Margin = new Thickness(0, 15, 0, 0),
            Title = Lang.Text("Welcome.Recent.Title"),
            CanSwap = false
        };

        var panel = new StackPanel { Margin = new Thickness(25, 38, 25, 18) };

        var instance = ModInstanceList.mcInstanceList?
            .SelectMany(pair => pair.Value)
            .FirstOrDefault(item => string.Equals(item.PathInstance, States.Game.SelectedInstance,
                StringComparison.OrdinalIgnoreCase));

        if (instance is null)
        {
            var empty = new TextBlock
            {
                Text = Lang.Text("Welcome.Recent.Empty"),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.75
            };
            panel.Children.Add(empty);
        }
        else
        {
            panel.Children.Add(new TextBlock
            {
                Text = instance.Name,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold
            });
            panel.Children.Add(new TextBlock
            {
                Text = instance.Info?.VanillaName ?? "",
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 0),
                Opacity = 0.7
            });

            var launch = new MyButton
            {
                Text = Lang.Text("Welcome.Recent.Launch"),
                ColorType = MyButton.ColorState.Highlight,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 16, 0, 0)
            };
            launch.Click += (_, _) => ModLaunch.McLaunchStart();
            panel.Children.Add(launch);
        }

        card.Children.Add(panel);
        return card;
    }

    #endregion

    private MyCard _BuildHero()
    {
        var card = new MyCard { Margin = new Thickness(0, 15, 0, 0), CanSwap = false };

        var panel = new StackPanel { Margin = new Thickness(25, 30, 25, 22) };

        var titleLine = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        var titleMain = _Text("性能状态", 28, bold: true);
        titleMain.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrush1");
        titleLine.Children.Add(titleMain);
        panel.Children.Add(titleLine);

        var subtitle = _Text("本机实时 · 仅本地计算", 12, false);
        subtitle.Margin = new Thickness(0, 6, 0, 0);
        subtitle.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrushGray4");
        panel.Children.Add(subtitle);

        var cpuRow = _BuildMetricRow("CPU 使用率", out var cpuValue, out var cpuFill, out var cpuTrack);
        cpuRow.Margin = new Thickness(0, 22, 0, 0);
        panel.Children.Add(cpuRow);

        var ramRow = _BuildMetricRow("内存占用", out var ramValue, out var ramFill, out var ramTrack);
        ramRow.Margin = new Thickness(0, 14, 0, 0);
        panel.Children.Add(ramRow);

        // GPU 使用率：靠性能计数器，某些驱动不提供这组计数器，取不到时把整行隐藏，
        // 而不是显示一个永远 0% 的假数据
        var gpuRow = _BuildMetricRow("GPU 使用率", out var gpuValue, out var gpuFill, out var gpuTrack);
        gpuRow.Margin = new Thickness(0, 14, 0, 0);
        gpuRow.Visibility = Visibility.Collapsed;
        panel.Children.Add(gpuRow);

        var detail = _Text("", 11, false);
        detail.Margin = new Thickness(0, 14, 0, 0);
        detail.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrushGray4");
        panel.Children.Add(detail);

        card.Children.Add(panel);

        ulong lastIdle = 0, lastKernel = 0, lastUser = 0;
        var seeded = false;
        // 记录每根进度条「当前显示到哪」，作为下一次动画的起点。
        // 不能读 fill.Width：属性返回的是基值，动画跑起来之后它永远还是那个初始值。
        var cpuShown = 0d;
        var ramShown = 0d;
        var gpuShown = 0d;
        var gpuProbed = false;
        var gpuSupported = false;

        void Update()
        {
            if (!GetSystemTimes(out var idle, out var kernel, out var user)) return;

            var idleNow = ToUInt64(idle);
            var kernelNow = ToUInt64(kernel);
            var userNow = ToUInt64(user);

            if (seeded)
            {
                var idleDelta = idleNow - lastIdle;
                var totalDelta = kernelNow - lastKernel + (userNow - lastUser);
                var cpu = totalDelta > 0 ? (1d - idleDelta / (double)totalDelta) * 100d : 0d;
                cpu = Math.Clamp(cpu, 0d, 100d);
                cpuValue.Text = $"{cpu:0}%";
                AnimateBarWidth(cpuFill, cpuTrack, cpu, ref cpuShown);
            }

            lastIdle = idleNow;
            lastKernel = kernelNow;
            lastUser = userNow;
            seeded = true;

            var mem = new MemoryStatusEx { dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>() };
            if (GlobalMemoryStatusEx(ref mem))
            {
                var ram = mem.dwMemoryLoad;
                ramValue.Text = $"{ram}%";
                AnimateBarWidth(ramFill, ramTrack, ram, ref ramShown);
                detail.Text = $"已用 {FormatGb(mem.ullTotalPhys - mem.ullAvailPhys)} / " +
                              $"{FormatGb(mem.ullTotalPhys)}  ·  可用 {FormatGb(mem.ullAvailPhys)}";
            }

            // GPU 只在第一次成功取到值后才把行露出来；之后取不到就保持上一次的数字，
            // 避免因为某个瞬间计数器为空导致行忽隐忽现
            if (!gpuProbed || gpuSupported)
            {
                var gpu = _ReadGpuUsage();
                gpuProbed = true;
                if (gpu >= 0d)
                {
                    gpuSupported = true;
                    gpuRow.Visibility = Visibility.Visible;
                    gpuValue.Text = $"{gpu:0}%";
                    AnimateBarWidth(gpuFill, gpuTrack, gpu, ref gpuShown);
                }
            }
        }

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
        timer.Tick += (_, _) => Update();
        card.Loaded += (_, _) =>
        {
            Update();
            timer.Start();
        };
        card.Unloaded += (_, _) => timer.Stop();

        return card;
    }

    /// <summary>
    ///     把进度条从「当前显示值」缓动到百分位对应的宽度。
    ///     <para>
    ///     宽度按 <paramref name="track" /> 的<b>实际像素宽</b>算，不能写死成常量：
    ///     底槽是 Stretch 撑满卡片的，窗口一宽就七百里，写死 300 的话 100% 也才走到
    ///     三分之一处，看起来像「永远不走满」。
    ///     </para>
    ///     <para>
    ///     <paramref name="shown" /> 保存的是逻辑进度（0–100），不是像素宽度——
    ///     窗口缩放后同一个百分比对应不同像素，用它换算才能在改尺寸后依然准确。
    ///     </para>
    /// </summary>
    private static void AnimateBarWidth(System.Windows.Shapes.Rectangle fill, System.Windows.FrameworkElement track,
        double percent, ref double shown, bool animate = true)
    {
        percent = Math.Clamp(percent, 0d, 100d);

        // ActualWidth 在首帧还没测量出来（是 0），此时没法换算，直接跳过等下一次 Tick。
        var trackWidth = track.ActualWidth;
        if (trackWidth <= 0d) return;

        var target = trackWidth * percent / 100d;

        if (!animate)
        {
            fill.BeginAnimation(FrameworkElement.WidthProperty, null);
            fill.Width = target;
            shown = percent;
            return;
        }

        var from = trackWidth * shown / 100d;
        shown = percent;

        if (Math.Abs(from - target) < 0.4)   // 变化太小不动画
        {
            fill.BeginAnimation(FrameworkElement.WidthProperty, null);
            fill.Width = target;
            return;
        }

        try
        {
            var anim = new DoubleAnimation(from, target, TimeSpan.FromMilliseconds(720))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
            };
            fill.BeginAnimation(FrameworkElement.WidthProperty, anim);
        }
        catch
        {
            fill.BeginAnimation(FrameworkElement.WidthProperty, null);
            fill.Width = target;
        }
    }

    private static Grid _BuildMetricRow(string label, out TextBlock value,
        out System.Windows.Shapes.Rectangle fill, out Grid track)
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        row.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var lab = _Text(label, 12, false);
        lab.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrushGray3");
        row.Children.Add(lab);

        value = _Text("--", 12, bold: true);
        value.HorizontalAlignment = HorizontalAlignment.Right;
        value.SetResourceReference(TextBlock.ForegroundProperty, "ColorBrush1");
        Grid.SetColumn(value, 1);
        row.Children.Add(value);

        track = new Grid
        {
            Height = 7,
            Margin = new Thickness(0, 7, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        var background = new Border { CornerRadius = new CornerRadius(3.5) };
        background.SetResourceReference(Border.BackgroundProperty, "ColorBrush7");
        track.Children.Add(background);

        fill = new System.Windows.Shapes.Rectangle
        {
            Width = 0,
            Height = 7,
            RadiusX = 3.5,
            RadiusY = 3.5,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        fill.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, "ColorBrush3");
        track.Children.Add(fill);

        Grid.SetRow(track, 1);
        Grid.SetColumnSpan(track, 2);
        row.Children.Add(track);
        return row;
    }

    private static string FormatGb(ulong bytes) => $"{bytes / 1024d / 1024d / 1024d:0.0} GB";

    /// <summary>
    ///     读取整机 GPU 使用率（0–100）。取不到返回 -1。
    ///     <para>
    ///     数据源是性能计数器 <c>GPU Engine</c> 的 <c>Utilization Percentage</c>。这组计数器
    ///     按「引擎」拆分实例，一台机器上常有几十上百个（3D、Copy、VideoDecode…，
    ///     每个进程每个引擎一条），要按 <c>engtype_3D</c> 过滤后再把同一时刻的百分比相加。
    ///     相加而不是取最大，因为多显卡或多个 3D 引擎真的会同时在跑。
    ///     </para>
    ///     <para>
    ///     几个必须处理的坑：
    ///     <list type="bullet">
    ///         <item>首次 <c>NextValue()</c> 永远返回 0，必须间隔采样两次才有数。</item>
    ///         <item>旧实例会残留（进程退出后计数器还在），读它抛异常，逐个 try 掉。</item>
    ///         <item>老显卡/老驱动根本没有这组计数器，<c>PerformanceCounterCategory</c> 直接抛，
    ///               这时返回 -1 让调用方把整行隐藏。</item>
    ///     </list>
    ///     </para>
    /// </summary>
    private static double _ReadGpuUsage()
    {
        try
        {
            if (!PerformanceCounterCategory.Exists("GPU Engine")) return -1d;

            var category = new PerformanceCounterCategory("GPU Engine");
            var instances = category.GetInstanceNames();
            if (instances.Length == 0) return -1d;

            var total = 0d;
            var seen = 0;

            foreach (var instance in instances)
            {
                // 只看 3D 引擎：Copy / VideoDecode 那些和「显卡忙不忙」不是一回事
                if (!instance.Contains("engtype_3D", StringComparison.OrdinalIgnoreCase)) continue;

                PerformanceCounter counter = null;
                try
                {
                    counter = new PerformanceCounter("GPU Engine", "Utilization Percentage", instance, true);
                    counter.NextValue();          // 第一次丢弃
                    var value = counter.NextValue();
                    if (value <= 0d) continue;
                    total += value;
                    seen++;
                }
                catch
                {
                    // 计数器实例刚被回收 / 权限不足，跳过这条
                }
                finally
                {
                    counter?.Dispose();
                }
            }

            if (seen == 0) return -1d;
            return Math.Clamp(total, 0d, 100d);
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "[Welcome] 读取 GPU 使用率失败", ModBase.LogLevel.Debug);
            return -1d;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileTime
    {
        public uint Low;
        public uint High;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    private static ulong ToUInt64(FileTime time) => ((ulong)time.High << 32) | time.Low;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);

    private MyCard _BuildNetworkCard()
    {
        var card = new MyCard
        {
            Margin = new Thickness(0, 15, 0, 0),
            Title = Lang.Text("Welcome.Network.Title"),
            CanSwap = false
        };

        var panel = new StackPanel { Margin = new Thickness(25, 38, 25, 18) };

        var description = new StackPanel();
        _AddLocalizedText(description, 13, false,
            ("zh-CN", "PCL_Max 不会在你不知情时联网。每一次出站连接都会先归入某个「用途」，" +
                      "由你决定是否放行；未登记的目标会被直接拦截，请求不会离开进程。"),
            ("zh-TW", "PCL_Max 不會在你不知情時連網。每一次連出連線都會先歸入某個「用途」，" +
                      "由你決定是否放行；未登記的目標會被直接攔截，請求不會離開行程。"),
            ("en", "PCL_Max never goes online behind your back. Every outbound connection is first " +
                   "classified into a purpose that you choose to allow or block; unregistered targets " +
                   "are dropped before the request ever leaves the process."));
        panel.Children.Add(description);

        _networkStatus = _Text("", 13, bold: true);
        _networkStatus.Margin = new Thickness(0, 12, 0, 0);
        panel.Children.Add(_networkStatus);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 14, 0, 0)
        };
        buttons.Children.Add(_Button(Lang.Text("Welcome.Network.Manage"),
            () => _Navigate(FormMain.PageType.Setup, FormMain.PageSubType.SetupLauncherMisc)));
        panel.Children.Add(buttons);

        card.Children.Add(panel);
        card.Loaded += (_, _) => RefreshStatus();
        return card;
    }

    private MyCard _BuildPrivacyCard()
    {
        var card = new MyCard
        {
            Margin = new Thickness(0, 15, 0, 0),
            Title = Lang.Text("Welcome.Privacy.Title"),
            CanSwap = false
        };

        var panel = new StackPanel { Margin = new Thickness(25, 38, 25, 18) };

        var description = new StackPanel();
        _AddLocalizedText(description, 13, false,
            ("zh-CN", "没有任何遥测与崩溃上报，不收集任何使用数据；崩溃分析全程在本地完成。" +
                      "已安装的版本在断网状态下依然可以启动。"),
            ("zh-TW", "沒有任何遙測與崩潰回報，不收集任何使用資料；崩潰分析全程在本機完成。" +
                      "已安裝的版本在斷網狀態下依然可以啟動。"),
            ("en", "No telemetry, no crash reporting, no usage data collection. Crash analysis " +
                   "happens entirely on your own machine, and installed versions keep launching " +
                   "while you are offline."));
        panel.Children.Add(description);

        _privacyStatus = _Text("", 13, bold: true);
        _privacyStatus.Margin = new Thickness(0, 12, 0, 0);
        panel.Children.Add(_privacyStatus);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 14, 0, 0)
        };
        buttons.Children.Add(_Button(Lang.Text("Welcome.Privacy.OpenDownload"),
            () => _Navigate(FormMain.PageType.Download)));
        panel.Children.Add(buttons);

        card.Children.Add(panel);
        card.Loaded += (_, _) => RefreshStatus();
        return card;
    }

    /// <summary>
    ///     彩蛋卡片：一句与功能无关的玩笑话。
    ///     <para>
    ///         文案本身是恶搞，名字均为虚拟，不代表任何真实人物；只在主页出现，不参与任何逻辑。
    ///     </para>
    /// </summary>
    private MyCard _BuildEasterEggCard()
    {
        var card = new MyCard
        {
            Margin = new Thickness(0, 15, 0, 0),
            Title = Lang.Text("Welcome.EasterEgg.Title"),
            CanSwap = false
        };

        var panel = new StackPanel { Margin = new Thickness(25, 38, 25, 18) };

        var description = new StackPanel();
        _AddLocalizedText(description, 13, false,
            ("zh-CN", "来自山西太原小店区的李亚华阿姨和马晨轩小朋友  你们好啊，我害死了PCL"),
            ("zh-TW", "來自山西太原小店區的李亞華阿姨和馬晨軒小朋友  你們好啊，我害死了PCL"),
            ("en", "来自山西太原小店区的李亚华阿姨和马晨轩小朋友  你们好啊，我害死了PCL"));
        panel.Children.Add(description);

        card.Children.Add(panel);
        return card;
    }

    #endregion

    #region 状态
    private void RefreshStatus()
    {
        var purposes = NetworkPurposeCatalog.All
            .Where(x => x.Purpose != NetworkPurpose.Unknown)
            .ToArray();
        var allowed = purposes.Count(x => NetworkConsentService.IsAllowed(x.Purpose));

        if (_networkStatus is not null)
        {
            _networkStatus.Text = NetworkConsentService.AllowAll
                ? Lang.Text("Welcome.Network.Status.All", purposes.Length)
                : Lang.Text("Welcome.Network.Status.Partial", allowed, purposes.Length);
        }

        if (_privacyStatus is not null)
        {
            var privacy = Config.System.PrivacyMode
                ? Lang.Text("Welcome.Privacy.Mode.On")
                : Lang.Text("Welcome.Privacy.Mode.Off");
            _privacyStatus.Text = Lang.Text("Welcome.Privacy.Status", privacy);
        }
    }

    #endregion

    #region 构造辅助

    /// <summary>
    ///     添加一组按语言分分支的文本，每个分支一个 <see cref="TextBlock"/>，
    ///     由 <see cref="HomepageLanguageApi"/> 在加载时保留命中当前语言的那个。
    /// </summary>
    private static void _AddLocalizedText(StackPanel panel, double size, bool bold,
        params (string Code, string Text)[] branches)
    {
        foreach (var (code, text) in branches)
            panel.Children.Add(_Text(text, size, bold, HomepageLanguageApi.LanguageTagPrefix + code));
    }

    private static TextBlock _Text(string text, double size, bool bold = false, string? tag = null)
    {
        var block = new TextBlock
        {
            Text = text,
            FontSize = size,
            TextWrapping = TextWrapping.Wrap,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal
        };
        if (tag is not null) block.Tag = tag;
        block.SetResourceReference(TextBlock.ForegroundProperty, bold ? "ColorBrush1" : "ColorBrushGray2");
        return block;
    }

    private static MyButton _Button(string text, Action onClick)
    {
        var button = new MyButton
        {
            Text = text,
            Height = 32,
            MinWidth = 116,
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(14, 0, 14, 0),
            ColorType = MyButton.ColorState.Highlight
        };
        button.Click += (_, _) => onClick();
        return button;
    }

    private static void _Navigate(FormMain.PageType page, FormMain.PageSubType subType = FormMain.PageSubType.Default)
    {
        if (ModMain.frmMain is null) return;
        ModMain.frmMain.PageChange(new FormMain.PageStackData { page = page }, subType);
    }

    #endregion
}
