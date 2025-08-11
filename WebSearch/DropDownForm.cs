namespace WebSearch
{
    public partial class DropDownForm : Form
    {
        // Fields
        private List<TabInfo> currentItems = new();
        private List<DisplayItem> displayItems = new();
        private string TextBuffer = string.Empty;

        // Event
        public event EventHandler<TabInfo>? SelectedTab;

        // Helper record
        private record DisplayItem(string Text, TabInfo? Info, bool IsWebSearch = false);

        // Constructor
        public DropDownForm()
        {
            InitializeComponent();

            Opacity = SettingsHandler.CurrentSettings.DropDownFormOpacity;

            listBoxSuggestions = new NoScrollBarListbox
            {
                BackColor = Constants.ColorBackground,
                BorderStyle = BorderStyle.None,
                ForeColor = Constants.ColorForeground,
                FormattingEnabled = true,
                Location = Constants.DropDownFormLocation,
                Name = "listBoxSuggestions",
                Size = Constants.DropDownFormSize,
                TabIndex = 0,
                ScrollAlwaysVisible = true,
            };
            Controls.Add(listBoxSuggestions);

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;

            ResultsPanel.BackColor = Constants.ColorDarkPurple;
            SelectedIndexInfoLabel.BackColor = Constants.ColorDarkPurple;
            SelectedIndexInfoLabel.ForeColor = Constants.ColorForeground;
            SelectedIndexInfoLabel.TextAlign = ContentAlignment.MiddleCenter;

            listBoxSuggestions.DoubleClick += ListBoxSuggestions_DoubleClick;
            listBoxSuggestions.KeyDown += ListBoxSuggestions_KeyDown;
            listBoxSuggestions.SelectedIndexChanged += UpdateInfo;
        }

        // Override window params
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

        // Update suggestions shown in list
        public void UpdateSuggestions(IEnumerable<TabInfo> suggestions)
        {
            var order = SettingsHandler.CurrentSettings.SearchOrder;

            // Group by URL, pick highest VisitCount for FrequentSitesItem, sort by category order, then by visit count, then title
            currentItems = suggestions
                .GroupBy(item => item.Url, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(i => i is FrequentSitesItem fs ? fs.VisitCount : 0).First())
                .OrderBy(item =>
                    item is OpenTab ? order.OpenTabs :
                    item is RecentItem ? order.RecentItems :
                    item is FrequentSitesItem ? order.FrequentSites :
                    item is BookmarkItem ? order.Bookmarks :
                    item is HistoryItem ? order.History : 0)
                .ThenByDescending(item => item is FrequentSitesItem f ? f.VisitCount : 0)
                .ThenBy(item => (item.Title ?? "").ToLowerInvariant())
                .ToList();

            int totalItems = currentItems.Count;
            int historyCount = currentItems.OfType<HistoryItem>().Count();
            int frequentCount = currentItems.OfType<FrequentSitesItem>().Count();
            int bookmarkCount = currentItems.OfType<BookmarkItem>().Count();
            int tabCount = currentItems.OfType<OpenTab>().Count();
            int recentCount = currentItems.OfType<RecentItem>().Count();

            TextBuffer = $"Total Items: {totalItems}  [{recentCount} Recent Entries, {tabCount} Open Tabs, {bookmarkCount} Bookmarks, {historyCount} History Items, {frequentCount} Frequent Sites]";
            SelectedIndexInfoLabel.Text = TextBuffer;

            displayItems = new List<DisplayItem>(totalItems + 1);

            foreach (var item in currentItems)
            {
                string text = item switch
                {
                    HistoryItem h => string.IsNullOrWhiteSpace(h.Title)
                        ? $"     [{h.VisitDate:yyyy-MM-dd HH:mm}]  {h.Url}"
                        : $"     [{h.VisitDate:yyyy-MM-dd HH:mm}]  {h.Title}    —    {h.Url}",
                    FrequentSitesItem f => string.IsNullOrWhiteSpace(f.Title)
                        ? $"    ⚡ {f.Url}"
                        : $"    ⚡ {f.Title} — {f.Url} (Visits: {f.VisitCount})",
                    RecentItem r => string.IsNullOrWhiteSpace(r.Title)
                        ? $"    🔥 {r.Url}"
                        : $"    🔥 {r.Title} — {r.Url}",
                    OpenTab o => string.IsNullOrWhiteSpace(o.Title)
                        ? $"    🗂️ {o.Url}"
                        : $"    🗂️ {o.Title} — {o.Url}",
                    BookmarkItem b => string.IsNullOrWhiteSpace(b.Title)
                        ? $"    ★ {b.Url}"
                        : $"    ★ {b.Title} — {b.Url}",
                    _ => string.IsNullOrWhiteSpace(item.Title) ? item.Url : $"    {item.Title} — {item.Url}"
                };

                displayItems.Add(new DisplayItem(text, item, false));
            }

            // Insert Web Search special entry if enabled
            if (displayItems.Count > 1)
            {
                int insertIndex = Math.Clamp(SettingsHandler.CurrentSettings.PositionOfWebSearchItem, 0, displayItems.Count);
                displayItems.Insert(insertIndex, new DisplayItem("    🌐  Web Search", null, true));
            }

            // Update listbox
            listBoxSuggestions.BeginUpdate();
            listBoxSuggestions.Items.Clear();
            listBoxSuggestions.Items.AddRange(displayItems.Select(di => (object)di.Text).ToArray());
            listBoxSuggestions.EndUpdate();

            if (displayItems.Count > 0)
            {
                if (!Visible) Show();
                listBoxSuggestions.SelectedIndex = -1;
            }
            else
            {
                Hide();
            }
        }

        // Move selection up/down
        public void MoveSelection(bool moveDown)
        {
            if (listBoxSuggestions.Items.Count == 0) return;

            int newIndex = listBoxSuggestions.SelectedIndex;
            newIndex = moveDown
                ? Math.Min(newIndex + 1, listBoxSuggestions.Items.Count - 1)
                : Math.Max(newIndex - 1, 0);

            listBoxSuggestions.SelectedIndex = newIndex;
            listBoxSuggestions.Focus();
        }

        // Update info label based on selection
        private void UpdateInfo(object? sender, EventArgs e)
        {
            int idx = listBoxSuggestions.SelectedIndex;

            if (idx >= 0 && idx < displayItems.Count)
            {
                var di = displayItems[idx];
                if (di.IsWebSearch)
                {
                    SelectedIndexInfoLabel.Text = $"Type: Web Search    |   {TextBuffer}";
                    return;
                }

                var selectedItem = di.Info!;
                string typeText = selectedItem switch
                {
                    BookmarkItem _ => "Bookmark",
                    HistoryItem _ => "History",
                    FrequentSitesItem _ => "Frequent Site",
                    OpenTab _ => "Opened Tab",
                    RecentItem _ => "Recent Entry",
                    _ => "Unknown Type"
                };

                SelectedIndexInfoLabel.Text = $"Type: {typeText}    |   {TextBuffer}";
            }
            else
            {
                SelectedIndexInfoLabel.Text = $"Type: None Selected   |   {TextBuffer}";
            }
        }

        // Double-click commits selection
        private void ListBoxSuggestions_DoubleClick(object? sender, EventArgs e) => CommitSelection();

        // Handle Enter and Escape keys
        private void ListBoxSuggestions_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                CommitSelection();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Hide();
            }
        }

        // Commit selected item or web search
        private void CommitSelection()
        {
            int selectedIndex = listBoxSuggestions.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= displayItems.Count) return;

            var di = displayItems[selectedIndex];

            if (di.IsWebSearch)
            {
                if (Owner is WebSearch webSearchForm)
                {
                    string val = webSearchForm.GetMainTextBoxText();
                    if (!string.IsNullOrWhiteSpace(val))
                    {
                        BrowserHelper.searchInANewTab(BrowserHelper.GetSystemDefaultBrowser(), val, false);
                        Hide();
                    }
                }
                return;
            }

            var selected = di.Info!;
            RecentItemsHandler.AddToRecentSites(new RecentItem
            {
                Title = selected.Title,
                Url = selected.Url
            });

            SelectedTab?.Invoke(this, selected);
        }

        // Commit first item in the list
        public void CommitFirstIndex()
        {
            if (listBoxSuggestions.Items.Count == 0) return;
            listBoxSuggestions.SelectedIndex = 0;
            CommitSelection();
        }

        // Get current selected index
        public int GetSelectedIndex() => listBoxSuggestions.SelectedIndex;
    }
}
