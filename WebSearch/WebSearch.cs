using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WebSearch
{
    public partial class WebSearch : Form
    {
        private string defaultBrowserURL = string.Empty;
        private DropDownForm dropDownForm;

        public static List<TabInfo> openTabs = new List<TabInfo>();
        public static List<TabInfo> bookmarks = new List<TabInfo>();
        public static List<HistoryItem> history = new List<HistoryItem>();
        public static List<FrequentSitesItem> frequentSites = new List<FrequentSitesItem>();

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                const int WS_EX_TOOLWINDOW = Constants.WS_EX_TOOLWINDOW;
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


        private void UpdateLists()
        {
            openTabs = FirefoxHelper.GetFirefoxOpenTabs();
            bookmarks = BookmarkHelper.GetBookmarks();
            history = HistoryHelper.GetHistory();
            frequentSites = FrequentSitesHelper.GetFrequentSites();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Logger.Print("APP Started!");
            ShowInTaskbar = false;

            defaultBrowserURL = BrowserHelper.GetSystemDefaultBrowser();
            bool registered = RegisterHotKey(this.Handle, Constants.HOTKEY_ID, Constants.MOD_CONTROL, Constants.SPACE_BAR);
            if (!registered)
                Logger.Print("Failed to register hotkey");

            WindowState = FormWindowState.Minimized;
            this.Hide();

            UpdateLists();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Constants.WM_HOTKEY && m.WParam.ToInt32() == Constants.HOTKEY_ID)
            {
                this.Show();
                UpdateLists();
                WindowState = FormWindowState.Normal;
                Activate();
                MainTextBox.Focus();

                MainTextBox_TextChanged(null, null);
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, Constants.HOTKEY_ID);
            base.OnFormClosing(e);
        }

        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string searchValue = MainTextBox.Text;
                if (searchValue == "@all")
                {
                    dropDownForm.ShowAllTabs(openTabs);
                    dropDownForm.Show();
                }
                else if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    BrowserHelper.searchInANewTab(defaultBrowserURL, searchValue);
                    MainTextBox.Text = "";
                    WindowState = FormWindowState.Minimized;
                    this.Hide();
                    dropDownForm.Hide();
                }
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;

                WindowState = FormWindowState.Minimized;
                this.Hide();
                dropDownForm.Hide();
            }

            if (dropDownForm.Visible)
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    e.SuppressKeyPress = true;
                    dropDownForm.MoveSelection(e.KeyCode == Keys.Down);
                }
            }
        }

        private void MainTextBox_TextChanged(object? sender, EventArgs? e)
        {
            string query = MainTextBox.Text ?? "";

            var pos = this.PointToScreen(new System.Drawing.Point(0, MainTextBox.Height));
            dropDownForm.Location = pos;
            dropDownForm.ShowInTaskbar = false;
            dropDownForm.TopMost = true;
            dropDownForm.Width = this.Width;

            var combinedList = openTabs
                .Concat(bookmarks)
                .Concat(history)
                .Concat(frequentSites)
                .ToList();


            var filtered = string.IsNullOrWhiteSpace(query)
                ? new List<TabInfo>()
                : combinedList.Where(t =>
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
                MainTextBox.Focus();
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
