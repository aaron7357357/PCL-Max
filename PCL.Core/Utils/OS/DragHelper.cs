using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using PCL.Core.Logging;

namespace PCL.Core.Utils.OS;

// ReSharper disable InconsistentNaming
public partial class DragHelper
{
    public event EventHandler? DragDrop;

    public string[]? DropFilePaths { get; private set; }
    public Point DropDragPoint { get; private set; }

    public HwndSource? HwndSource { get; set; }

    #region Public API

    public void AddHook()
    {
        if (HwndSource is null)
            throw new InvalidOperationException("HwndSource 未设置");

        RemoveHook();

        HwndSource.AddHook(WndProc);
        IntPtr hwnd = HwndSource.Handle;

        if (IsUserAnAdmin())
            RevokeDragDrop(hwnd);

        DragAcceptFiles(hwnd, true);
        ChangeMessageFilter(hwnd);
    }

    public void RemoveHook()
    {
        if (HwndSource is null)
            return;

        HwndSource.RemoveHook(WndProc);
        DragAcceptFiles(HwndSource.Handle, false);
    }

    #endregion

    #region WndProc

    private IntPtr WndProc(
        IntPtr hwnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (TryGetDropInfo(msg, wParam, out var files, out var pt))
        {
            DropFilePaths = files;
            DropDragPoint = new Point(pt.X, pt.Y);
            DragDrop?.Invoke(this, EventArgs.Empty);
            handled = true;
        }

        return IntPtr.Zero;
    }

    #endregion

    #region Message filter (UAC)

    /// <summary>
    ///     允许低权限进程（资源管理器）把拖放消息发给本窗口。
    ///     <para>
    ///     <b>失败不能抛异常。</b>这个调用只是为了让「从资源管理器拖文件进窗口」在提权场景下也能用；
    ///     拿不到权限时最坏结果就是拖放不工作，而抛出去会让整个启动器在
    ///     <c>OnSourceInitialized</c> 里直接崩成一屏「未捕获的异常」。
    ///     实测触发条件：上一个实例以管理员身份运行并占着单例锁，新实例以普通权限启动，
    ///     此时改窗口消息过滤器会被 UIPI 拦下（ERROR_ACCESS_DENIED）。
    ///     </para>
    /// </summary>
    private static unsafe void ChangeMessageFilter(IntPtr hwnd)
    {
        var ver = Environment.OSVersion.Version;
        if (ver < new Version(6, 0))
            return;

        var win7OrHigher = ver >= new Version(6, 1);

        var filter = new CHANGEFILTERSTRUCT
        {
            cbSize = (uint)sizeof(CHANGEFILTERSTRUCT)
        };

        uint[] messages = [
            WM_DROPFILES,
            WM_COPYGLOBALDATA,
            WM_COPYDATA
        ];

        foreach (var msg in messages)
        {
            var ok = win7OrHigher
                ? ChangeWindowMessageFilterEx(hwnd, msg, MSGFLT_ALLOW, ref filter)
                : ChangeWindowMessageFilter(msg, MSGFLT_ADD);

            // 只是少了拖放能力，不值得让启动器起不来。留一行 Debug 级日志便于需要时排查。
            if (!ok)
            {
                var err = Marshal.GetLastWin32Error();
                Log(err);
                return;
            }
        }
    }

    /// <summary>
    ///     记录一次「改消息过滤器失败」。放成独立方法是为了不在循环里写 try/catch。
    /// </summary>
    private static void Log(int win32Error)
    {
        try
        {
            LogWrapper.Debug("DragHelper",
                $"未能放行拖放消息（Win32 错误 {win32Error}），拖放功能不可用，其余功能不受影响");
        }
        catch
        {
            // 日志系统本身出问题时也不能影响启动
        }
    }

    #endregion

    #region Drop parsing

    private static bool TryGetDropInfo(
        int msg,
        IntPtr hDrop,
        out string[]? filePaths,
        out DragPoint dropPoint)
    {
        filePaths = null;
        dropPoint = default;

        if (msg != WM_DROPFILES)
            return false;

        var count = DragQueryFile(hDrop, uint.MaxValue, IntPtr.Zero, 0);
        filePaths = new string[count];

        const int maxPath = 32768, smallerMaxPath = 1024;

        Span<char> gBuffer = stackalloc char[smallerMaxPath];
        for (uint i = 0; i < count; i++)
        {
            var len = DragQueryFile(hDrop, i, IntPtr.Zero, 0) + 1;
            if (len > maxPath) len = maxPath;
            var buffer = len <= smallerMaxPath ? gBuffer[..(int)len] : new char[len];
            _ = DragQueryFile(hDrop, i, buffer, len);
            filePaths[i] = new string(buffer[..(int)(len - 1)]);
        }

        DragFinish(hDrop);
        return true;
    }

    #endregion

    #region Win32

    private const uint WM_COPYGLOBALDATA = 0x0049;
    private const uint WM_COPYDATA = 0x004A;
    private const uint WM_DROPFILES = 0x0233;

    private const uint MSGFLT_ALLOW = 1;
    private const uint MSGFLT_ADD = 1;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ChangeWindowMessageFilter(
        uint msg,
        uint flags);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ChangeWindowMessageFilterEx(
        IntPtr hwnd,
        uint msg,
        uint action,
        ref CHANGEFILTERSTRUCT filter);

    [LibraryImport("shell32.dll")]
    private static partial void DragAcceptFiles(
        IntPtr hwnd,
        [MarshalAs(UnmanagedType.Bool)] bool accept);

    [LibraryImport("shell32.dll", EntryPoint = "DragQueryFileW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial uint DragQueryFile(IntPtr hDrop, uint iFile, Span<char> lpszFile, uint cch);
    
    [LibraryImport("shell32.dll", EntryPoint = "DragQueryFileW")]
    private static partial uint DragQueryFile(IntPtr hDrop, uint iFile, IntPtr lpszFile, uint cch);

    [LibraryImport("shell32.dll")]
    private static partial void DragFinish(IntPtr hDrop);

    [LibraryImport("ole32.dll")]
    private static partial int RevokeDragDrop(IntPtr hwnd);

    [LibraryImport("shell32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsUserAnAdmin();

    #endregion

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    private struct DragPoint
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CHANGEFILTERSTRUCT
    {
        public uint cbSize;
        public uint ExtStatus;
    }

    #endregion
}
