using System.Runtime.InteropServices;

namespace WordFlash.App;

internal static partial class NativeMethods
{
    private const int GwlExStyle = -20;
    private const nint WsExTransparent = 0x00000020;
    private const nint WsExToolWindow = 0x00000080;
    private const nint WsExLayered = 0x00080000;
    private const nint WsExNoActivate = 0x08000000;

    /// <summary>
    /// Lets mouse input fall through to the windows underneath, keeps the window out of Alt+Tab,
    /// and stops it from taking focus.
    /// </summary>
    public static void MakeClickThrough(nint hwnd)
    {
        var style = GetWindowLongPtr(hwnd, GwlExStyle);
        SetWindowLongPtr(hwnd, GwlExStyle, style | WsExTransparent | WsExToolWindow | WsExLayered | WsExNoActivate);
    }

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static partial nint GetWindowLongPtr(nint hwnd, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static partial nint SetWindowLongPtr(nint hwnd, int index, nint newValue);
}
