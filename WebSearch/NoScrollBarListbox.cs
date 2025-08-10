using System.Runtime.InteropServices;

namespace WebSearch
{
    public class NoScrollBarListbox : ListBox
    {
        private const int SB_VERT = 1;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            HideVScrollBar();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Keep the scrollbar hidden when control layout changes
            HideVScrollBar();
        }

        private void HideVScrollBar()
        {
            // Do NOT remove WS_VSCROLL. Just hide the visible scrollbar and force a redraw.
            ShowScrollBar(this.Handle, SB_VERT, false);
            SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }
    }
}
