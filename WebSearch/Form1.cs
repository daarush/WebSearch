using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WebSearch
{
    public partial class Form1 : Form
    {
        uint MOD_CONTROL = 0x0002;
        uint spaceBar = (uint)Keys.Space;
        const uint WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public Form1()
        {
            InitializeComponent();
        }

        private void print(string msg)
        {
            System.Diagnostics.Debug.WriteLine(msg);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                this.Show();
                WindowState = FormWindowState.Normal;
                Activate();
                MainTextBox.Focus();
            }
            base.WndProc(ref m);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            RegisterHotKey(this.Handle, 1, MOD_CONTROL, spaceBar);
            this.Hide();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);
            base.OnFormClosing(e);
        }

        private void searchInANewTab(string searchValue)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = @"C:\Program Files\Mozilla Firefox\firefox.exe";
            psi.Arguments = $"https://google.com/search?q={Uri.EscapeDataString(searchValue)}";
            psi.CreateNoWindow = true;
            psi.UseShellExecute = true;
            Process.Start(psi);

        }

        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string searchValue = MainTextBox.Text;
                e.SuppressKeyPress = true;

                WindowState = FormWindowState.Minimized;


                if (searchValue != "")
                {
                    searchInANewTab(searchValue);
                    MainTextBox.Text = "";
                    searchValue = MainTextBox.Text;
                }
            }
            if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Return)
            {
                WindowState = FormWindowState.Minimized;
            }
        }
    }
}
