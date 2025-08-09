using K4os.Compression.LZ4;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace WebSearch
{
    public partial class WebSearch : Form
    {
        private const uint MOD_CONTROL = 0x0002;
        private const uint WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 1;
        private readonly uint spaceBar = (uint)Keys.Space;

        private string defaultBrowserURL = string.Empty;
        private DropDownForm dropDownForm;
        public static List<TabInfo> openTabs = new List<TabInfo>();

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

        public WebSearch()
        {
            InitializeComponent();

            dropDownForm = new DropDownForm();
            dropDownForm.SelectedTab += DropDownForm_SelectedTab;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Logger.Print("APP Started!");
            ShowInTaskbar = false;

            defaultBrowserURL = BrowserHelper.GetSystemDefaultBrowser();
            bool registered = RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL, spaceBar);
            if (!registered)
                Logger.Print("Failed to register hotkey");

            WindowState = FormWindowState.Minimized;
            this.Hide();

            openTabs = FirefoxHelper.GetFirefoxOpenTabs();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                this.Show();

                openTabs = FirefoxHelper.GetFirefoxOpenTabs();

                WindowState = FormWindowState.Normal;
                Activate();
                MainTextBox.Focus();
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            base.OnFormClosing(e);
        }

        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string searchValue = MainTextBox.Text;
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    BrowserHelper.searchInANewTab(defaultBrowserURL, searchValue);
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

        private void MainTextBox_TextChanged(object sender, EventArgs e)
        {
            string query = MainTextBox.Text ?? "";

            // Keep textbox position unchanged, position dropdown below textbox
            var pos = MainTextBox.PointToScreen(new System.Drawing.Point(0, MainTextBox.Height));
            dropDownForm.Location = pos;
            dropDownForm.ShowInTaskbar = false;
            dropDownForm.TopMost = true;
            dropDownForm.Width = MainTextBox.Width;

            // Filter tabs by query, case-insensitive; hide dropdown if no matches or empty input
            var filtered = string.IsNullOrWhiteSpace(query)
                ? new List<TabInfo>()
                : openTabs.Where(t =>
                    (t.Title ?? "").Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    (t.Url ?? "").Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (filtered.Any())
            {
                dropDownForm.UpdateSuggestions(filtered);
                dropDownForm.BringToFront();
                MainTextBox.Focus();
            }
            else
            {
                dropDownForm.Hide();
            }
        }

        private void DropDownForm_SelectedTab(object? sender, TabInfo selected)
        {
            if (selected == null) return;

            dropDownForm.Hide();
            MainTextBox.Text = "";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = selected.Url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Logger.Print("Failed to open URL: " + ex.ToString());
            }
        }
    }
}
