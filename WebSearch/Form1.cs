using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WebSearch
{
    public partial class Form1 : Form
    {
        private const uint MOD_CONTROL = 0x0002;
        private const uint WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 1;
        private readonly uint spaceBar = (uint)Keys.Space;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                return cp;
            }
        }


        public Form1()
        {
            InitializeComponent();
        }

        private void print(string msg)
        {
            Debug.WriteLine(msg);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
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
            print("APP Started!");
            ShowInTaskbar = false;

            bool registered = RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL, spaceBar);
            if (!registered)
                print("Failed to register hotkey");

            WindowState = FormWindowState.Minimized;
            this.Hide();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            base.OnFormClosing(e);
        }

        private void searchInANewTab(string searchValue)
        {
            var psi = new ProcessStartInfo
            {
                FileName = @"C:\Program Files\Mozilla Firefox\firefox.exe",
                Arguments = $"https://google.com/search?q={Uri.EscapeDataString(searchValue)}",
                CreateNoWindow = true,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string searchValue = MainTextBox.Text;
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    searchInANewTab(searchValue);
                    MainTextBox.Text = "";
                }

                WindowState = FormWindowState.Minimized;
                this.Hide();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;

                WindowState = FormWindowState.Minimized;
                this.Hide();
            }
        }
    }
}
