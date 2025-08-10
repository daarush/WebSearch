using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WebSearch
{
    public partial class WebSearch : Form
    {
        private string defaultBrowserURL = string.Empty;
        private DropDownForm dropDownForm;

        public static List<OpenTab> openTabs = new List<OpenTab>();
        public static List<BookmarkItem> bookmarks = new List<BookmarkItem>();
        public static List<HistoryItem> history = new List<HistoryItem>();
        public static List<FrequentSitesItem> frequentSites = new List<FrequentSitesItem>();
        public static List<RecentItem> recentSites = new List<RecentItem>();

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
            dropDownForm.Owner = this;

            this.Deactivate += async (s, e) =>
            {
                // small delay allows clicks that move focus to the dropdown to land
                await Task.Delay(60);

                bool mainHasFocus = this.ContainsFocus;
                bool dropdownHasFocus = dropDownForm != null && dropDownForm.ContainsFocus;

                if (!mainHasFocus && !dropdownHasFocus)
                {
                    // hide both
                    dropDownForm?.Hide();
                    this.Hide();
                }
            };

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

                string searchValue = MainTextBox.Text.Trim();
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    if (IsValidUrl(searchValue))
                    {
                        if (!searchValue.StartsWith("http://") && !searchValue.StartsWith("https://"))
                            searchValue = "https://" + searchValue;

                        BrowserHelper.searchInANewTab(defaultBrowserURL, searchValue, true);
                    }
                    else
                    {
                        BrowserHelper.searchInANewTab(defaultBrowserURL, searchValue, false);
                    }

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

        bool IsValidUrl(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                return true;
            }

            if (Uri.TryCreate("https://" + input, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
            {
                return true;
            }

            return false;
        }


        private void MainTextBox_TextChanged(object? sender, EventArgs? e)
        {
            string query = MainTextBox.Text ?? "";

            var pos = this.PointToScreen(new System.Drawing.Point(0, MainTextBox.Height));
            dropDownForm.Location = pos;
            dropDownForm.ShowInTaskbar = false;
            dropDownForm.TopMost = true;
            dropDownForm.Width = this.Width;

            List<TabInfo>? sourceList = null;
            string filterTerm = query;

            // Check if query starts with '@' and has a space
            if (query.StartsWith("@") && query.Contains(' '))
            {
                var splitIndex = query.IndexOf(' ');
                var command = query.Substring(0, splitIndex).ToLower();
                filterTerm = query.Substring(splitIndex + 1);

                switch (command)
                {
                    case "@o":
                        sourceList = openTabs.Cast<TabInfo>().ToList();
                        break;
                    case "@b":
                        sourceList = bookmarks.Cast<TabInfo>().ToList();
                        break;
                    case "@h":
                        sourceList = history.Cast<TabInfo>().ToList();
                        break;
                    case "@f":
                        sourceList = frequentSites.Cast<TabInfo>().ToList();
                        break;
                    case "@r":
                        Logger.Print("Recent sites: " + recentSites.Count);
                        sourceList = recentSites.Cast<TabInfo>().ToList();
                        break;
                    default:
                        sourceList = null;
                        break;
                }
            }

                if (sourceList == null)
            {
                sourceList = openTabs
                    .Cast<TabInfo>()
                    .Concat(recentSites)
                    .Concat(bookmarks)
                    .Concat(history)
                    .Concat(frequentSites)
                    .ToList();
            }

            var filtered = string.IsNullOrWhiteSpace(filterTerm)
                ? sourceList
                : sourceList.Where(t =>
                    (t.Title ?? "").Contains(filterTerm, StringComparison.OrdinalIgnoreCase) ||
                    (t.Url ?? "").Contains(filterTerm, StringComparison.OrdinalIgnoreCase))
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

        public static void AddToRecentSites(RecentItem item)
        {
            recentSites.RemoveAll(x => string.Equals(x.Url, item.Url, StringComparison.OrdinalIgnoreCase));
            recentSites.Insert(0, item);
            if (recentSites.Count > Constants.MaxRecentItems)
            {
                recentSites = recentSites.Take(Constants.MaxRecentItems).ToList();
            }
        }

    }
}
