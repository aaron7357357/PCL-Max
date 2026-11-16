using System.Windows;
using PCL.Controls;

namespace PCL.Pages.PageLink;

public partial class PageLinkLeft
{
    public PageLinkLeft()
    {
        InitializeComponent();
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
}
