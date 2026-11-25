using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PCL.Core.App.Localization;

namespace PCL.Pages.PageLink;

public partial class PageLinkLeft
{
    public FormMain.PageSubType pageID = FormMain.PageSubType.LinkHost;
    private bool _isUpdatingSelection;
    private Brush? _brushReady;
    private Brush? _brushIdle;

    public PageLinkLeft()
    {
        InitializeComponent();
        AnimatedControl = PanItem;
        Loaded += (_, _) =>
        {
            Select(pageID, false);
            ModMain.frmLinkRight?.PushSidebarStatus();
        };
    }

    private void PageCheck(object sender, ModBase.RouteEventArgs e)
    {
        if (_isUpdatingSelection) return;
        if (sender is MyListItem { Tag: not null } item)
            PageChange((FormMain.PageSubType)ModBase.Val(item.Tag));
    }

    public object PageGet(FormMain.PageSubType id)
    {
        // 陶瓦走另一套页面，剩下两种（房主 / 房客）共用 IPv6 直连页
        if (id == FormMain.PageSubType.LinkTerracotta)
        {
            ModMain.frmLinkTerracotta ??= new PageLinkTerracotta();
            return ModMain.frmLinkTerracotta;
        }

        ModMain.frmLinkRight ??= new PageLinkRight();
        var normalized = id == FormMain.PageSubType.LinkGuest
            ? FormMain.PageSubType.LinkGuest
            : FormMain.PageSubType.LinkHost;
        ModMain.frmLinkRight.SetMode(normalized);
        return ModMain.frmLinkRight;
    }

    public void Select(FormMain.PageSubType id, bool animate)
    {
        var item = id switch
        {
            FormMain.PageSubType.LinkGuest => ItemGuest,
            FormMain.PageSubType.LinkTerracotta => ItemTerracotta,
            _ => ItemHost
        };
        if (item.Checked) return;
        _isUpdatingSelection = true;
        try
        {
            item.SetChecked(true, animate, false);
        }
        finally
        {
            _isUpdatingSelection = false;
        }
    }

    public void UpdateSidebarStatus(bool ready, string statusText, string detailText)
    {
        if (!CheckAccess())
        {
            Dispatcher.BeginInvoke(() => UpdateSidebarStatus(ready, statusText, detailText));
            return;
        }

        _brushReady ??= (Brush)FindResource("ColorBrush3");
        _brushIdle ??= (Brush)FindResource("ColorBrushGray4");
        ShapeStatusDot.Fill = ready ? _brushReady : _brushIdle;
        LabStatus.Text = statusText;
        LabDetail.Text = detailText;
    }

    private void PageChange(FormMain.PageSubType id)
    {
        if (pageID == id) return;
        Select(id, false);

        // 已经在联机页时，左侧切换只需换右半边内容，但三种目标的右页不是同一个：
        // IPv6 的房主/房客共用 PageLinkRight（靠 SetMode 切视图），
        // 陶瓦是另一个页面，必须真正换 Content 而不是 SetMode。
        if (ModMain.frmMain.pageCurrent.page == FormMain.PageType.Link)
        {
            pageID = id;
            if (id == FormMain.PageSubType.LinkTerracotta)
            {
                ModMain.frmLinkTerracotta ??= new PageLinkTerracotta();
                PageChangeRun(ModMain.frmLinkTerracotta);
            }
            else
            {
                ModMain.frmLinkRight ??= new PageLinkRight();
                ModMain.frmLinkRight.SetMode(id);
                PageChangeRun(ModMain.frmLinkRight);
            }

            return;
        }

        ModMain.frmMain.PageChange(FormMain.PageType.Link, id);
    }

    /// <summary>
    ///     把右半边换成指定页面，走和设置页同一套切换动画。
    ///     <para>
    ///     照抄 <c>PageSetupLeft.PageChangeRun</c>：直接改 Child 不触发 PageOnExit/PageOnEnter，
    ///     陶瓦页的 Loaded 逻辑（读配置、起状态轮询）就不会跑，页面看起来是空的。
    ///     动画里那两段延迟也照搬——第二段 <c>after: true</c> 保证它排在控件 Loaded 之后，
    ///     否则 <c>PageOnEnter</c> 会在控件还没就绪时被调用。
    ///     </para>
    /// </summary>
    private static void PageChangeRun(MyPageRight target)
    {
        ModAnimation.AniStop("FrmMain PageChangeRight");
        if (target.Parent is not null)
            target.SetValue(ContentPresenter.ContentProperty, null);
        ModMain.frmMain.pageRight = target;
        if (ModMain.frmMain.PanMainRight.Child is MyPageRight current)
            current.PageOnExit();
        ModAnimation.AniStart(new[]
        {
            ModAnimation.AaCode(() =>
            {
                if (ModMain.frmMain.PanMainRight.Child is MyPageRight prev)
                    prev.PageOnForceExit();
                ModMain.frmMain.PanMainRight.Child = ModMain.frmMain.pageRight;
                ModMain.frmMain.pageRight.Opacity = 0d;
            }, 130),
            ModAnimation.AaCode(() =>
            {
                ModMain.frmMain.pageRight.Opacity = 1d;
                ModMain.frmMain.pageRight.PageOnEnter();
            }, 30, true)
        }, "PageLeft PageChange");
    }

    private void BtnRefreshNet_Click(object sender, MouseButtonEventArgs e)
    {
        ModMain.frmLinkRight ??= new PageLinkRight();
        ModMain.frmLinkRight.RefreshIpv6ByUser();
    }
}
