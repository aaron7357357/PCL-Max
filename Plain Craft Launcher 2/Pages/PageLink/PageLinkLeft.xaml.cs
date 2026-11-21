using System;
using System.Windows;
using System.Windows.Media;
using PCL.Controls;
using PCL.Core.App.Localization;
using PCL.Core.IO.Net;

namespace PCL.Pages.PageLink;

public partial class PageLinkLeft
{
    public PageLinkLeft()
    {
        InitializeComponent();
        Loaded += (_, _) => RefreshNetworkStatus();
    }

    public int pageID
    {
        get => 0;
        set { }
    }

    public FrameworkElement PageGet(FormMain.PageSubType subType)
    {
        ModMain.frmLinkRight ??= new PageLinkMain();
        return ModMain.frmLinkRight;
    }

    /// <summary>左栏单项被勾选：联机页只有一项，直接确保右侧内容就位。</summary>
    private void PageCheck(object senderRaw, ModBase.RouteEventArgs e)
    {
        ModMain.frmLinkRight ??= new PageLinkMain();
    }

    private void BtnNetRefresh_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        RefreshNetworkStatus();
    }

    /// <summary>
    ///     检测并展示本机 IPv6 可用性。
    ///     <para>
    ///         为什么关心这个：陶瓦（Terracotta）靠 EasyTier 建 P2P 连接，有公网 IPv6 时能直连打洞，
    ///         没有就只能走中继，延迟和带宽都差一截。所以开房前先告诉用户当前处在哪种情况。
    ///     </para>
    ///     <para>
    ///         检测本身复用 <see cref="NetworkInterfaceUtils.GetIPv6Status" />，但这里<b>不用它的返回值</b>：
    ///         那个实现在「第一个没有 IPv6 地址的网卡」上就直接 return Unavailable，
    ///         一台同时插着网线和 WiFi、或装了虚拟网卡的机器会被误判成「没有 IPv6」。
    ///         这里自己遍历全部接口，任意一张有公网地址就算可用。
    ///     </para>
    /// </summary>
    private void RefreshNetworkStatus()
    {
        try
        {
            var status = DetectIPv6();
            switch (status)
            {
                case NetworkInterfaceUtils.IPv6Status.Public:
                    SetStatus("公网 IPv6 可用", "网络环境正常，可以直接开房或加房", true);
                    break;
                case NetworkInterfaceUtils.IPv6Status.RFC4193:
                    SetStatus("仅内网 IPv6", "检测到局域网内 IPv6，公网直连可能失败，会退回中继", false);
                    break;
                case NetworkInterfaceUtils.IPv6Status.Unavailable:
                    SetStatus("没有 IPv6", "将走陶瓦中继，速度取决于中继节点", false);
                    break;
                default:
                    SetStatus("网络状态未知", "无法识别网络环境，仍可尝试开房", false);
                    break;
            }
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "[Link] 检测 IPv6 失败", ModBase.LogLevel.Debug);
            SetStatus("检测失败", "读取网络接口时出错，不影响联机", false);
        }
    }

    /// <summary>
    ///     遍历所有物理网卡判断 IPv6 情况。
    ///     <para>
    ///     忽略回环、隧道、VPN、虚拟机网卡——它们身上也会挂 IPv6 地址（比如 <c>::1</c> 或
    ///     Hyper-V 的虚拟网段），算进来会把「其实没有公网 IPv6」误报成有。
    ///     </para>
    /// </summary>
    private static NetworkInterfaceUtils.IPv6Status DetectIPv6()
    {
        var sawAnyV6 = false;

        foreach (var iface in NetworkInterfaceUtils.GetAvailableInterface())
        {
            System.Net.NetworkInformation.UnicastIPAddressInformationCollection addresses;
            try
            {
                addresses = iface.GetIPProperties().UnicastAddresses;
            }
            catch
            {
                // 网卡在枚举过程中被拔掉 / 驱动异常时会抛，跳过这张继续看下一张
                continue;
            }

            foreach (var info in addresses)
            {
                var ip = info.Address;
                if (ip.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6) continue;

                // 链路本地（fe80::/10）只在本网段有效，不能算「能用」
                if (ip.IsIPv6LinkLocal) continue;

                sawAnyV6 = true;

                var bytes = ip.GetAddressBytes();
                // 公网范围 2000::/3，即首字节 0x20–0x3F
                if (bytes[0] >= 0x20 && bytes[0] <= 0x3F)
                    return NetworkInterfaceUtils.IPv6Status.Public;
            }
        }

        return sawAnyV6 ? NetworkInterfaceUtils.IPv6Status.RFC4193 : NetworkInterfaceUtils.IPv6Status.Unavailable;
    }

    private void SetStatus(string title, string description, bool good)
    {
        LabNetTitle.Text = title;
        LabNetDesc.Text = description;

        // 可用时用主题强调色，不可用时用灰色——不引入新的红色，
        // 「没有 IPv6」只是速度慢一点，不是错误状态
        ShapeNetDot.Fill = good
            ? (Brush)FindResource("ColorBrush3")
            : (Brush)FindResource("ColorBrushGray2");
    }
}
