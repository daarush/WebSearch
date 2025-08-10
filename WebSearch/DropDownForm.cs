namespace WebSearch
{
    public partial class DropDownForm : Form
    {
        private List<TabInfo> currentItems = new();

        public event EventHandler<TabInfo>? SelectedTab;
        private string TextBuffer = string.Empty;
        
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

        public DropDownForm()
        {
            InitializeComponent();
            Opacity = Constants.FormOpacity;

            listBoxSuggestions = new NoScrollBarListbox
            {
                BackColor = Constants.BackgroundColor,
                BorderStyle = BorderStyle.None,
                ForeColor = Constants.ForegroundColor,
                FormattingEnabled = true,
                Location = Constants.DropDownLocation,
                Name = "listBoxSuggestions",
                Size = Constants.DropDownSize,
                TabIndex = 0,
                ScrollAlwaysVisible = true,
            };
            Controls.Add(listBoxSuggestions);

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;

            ResultsPanel.BackColor = Constants.DarkPurple;
            SelectedIndexInfoLabel.BackColor = Constants.DarkPurple;
            SelectedIndexInfoLabel.ForeColor = Constants.ForegroundColor;
            SelectedIndexInfoLabel.TextAlign = ContentAlignment.MiddleCenter;

            listBoxSuggestions.DoubleClick += ListBoxSuggestions_DoubleClick;
            listBoxSuggestions.KeyDown += ListBoxSuggestions_KeyDown;
            listBoxSuggestions.SelectedIndexChanged += UpdateInfo;
        }

        public void UpdateSuggestions(IEnumerable<TabInfo> suggestions)
        {
            currentItems = suggestions
                .GroupBy(item => item.Url, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderByDescending(item =>
                    item is OpenTab ? 4 :
                    item is FrequentSitesItem ? 3 :
                    item is HistoryItem ? 2 :
                    item is BookmarkItem ? 1 : 0
                )
                .ThenBy(item => item.Title)
                .ToList();

            int totalItemsFiltered = currentItems.Count;
            int getHistoryCount = currentItems.OfType<HistoryItem>().Count();
            int getFrequentCount = currentItems.OfType<FrequentSitesItem>().Count();
            int getBookmarkCount = currentItems.OfType<BookmarkItem>().Count();
            int getTabCount = currentItems.OfType<OpenTab>().Count();

            TextBuffer = $"Total Items: {totalItemsFiltered}  [{getTabCount} Tabs, {getBookmarkCount} Bookmarks, {getHistoryCount} History Items, {getFrequentCount} Frequent Sites]";
            SelectedIndexInfoLabel.Text = TextBuffer;

            listBoxSuggestions.BeginUpdate();
            listBoxSuggestions.Items.Clear();

            foreach (var item in currentItems)
            {

                if (item is HistoryItem historyItem)
                {
                    listBoxSuggestions.Items.Add(
                        string.IsNullOrWhiteSpace(historyItem.Title)
                        ? $"     [{historyItem.VisitDate:yyyy-MM-dd HH:mm}]  {historyItem.Url}"
                        : $"     [{historyItem.VisitDate:yyyy-MM-dd HH:mm}]  {historyItem.Title}    —    {historyItem.Url}"
                    );
                }
                else if (item is FrequentSitesItem frequent)
                {
                    listBoxSuggestions.Items.Add(
                        string.IsNullOrWhiteSpace(frequent.Title)
                        ? $"    ★ {frequent.Url}"
                        : $"    ★ {frequent.Title} — {frequent.Url} (Visits: {frequent.VisitCount})"
                    );
                }
                else
                {
                    listBoxSuggestions.Items.Add(
                        string.IsNullOrWhiteSpace(item.Title)
                        ? item.Url
                        : $"    {item.Title} — {item.Url}"
                    );
                }
            }

            listBoxSuggestions.EndUpdate();

            if (currentItems.Count > 0)
            {
                if (!Visible) Show();
                listBoxSuggestions.SelectedIndex = -1;
            }
            else
            {
                Hide();
            }
        }

        public void MoveSelection(bool moveDown)
        {
            if (listBoxSuggestions.Items.Count == 0) return;

            int newIndex = listBoxSuggestions.SelectedIndex;

            if (moveDown)
                newIndex = Math.Min(newIndex + 1, listBoxSuggestions.Items.Count - 1);
            else
                newIndex = Math.Max(newIndex - 1, 0);

            listBoxSuggestions.SelectedIndex = newIndex;
            listBoxSuggestions.Focus();
        }

        private void UpdateInfo(object? sender, EventArgs e)
        {
            if (listBoxSuggestions.SelectedIndex >= 0 && listBoxSuggestions.SelectedIndex < currentItems.Count)
            {
                var selectedItem = currentItems[listBoxSuggestions.SelectedIndex];
                var outputString = "";

                switch (selectedItem)
                {
                    case BookmarkItem b:
                        outputString = "Bookmark";
                        break;
                    case HistoryItem h:
                        outputString = "History";
                        break;
                    case FrequentSitesItem f:
                        outputString = "Frequent Site";
                        break;
                    case OpenTab o:
                        outputString = "Opened Tab";
                        break;
                    default:
                        outputString = "Unknown Type";
                        break;
                } 
                    SelectedIndexInfoLabel.Text = $"Type: {outputString}    |   {TextBuffer}";
            }
            else
            {
                SelectedIndexInfoLabel.Text = $"Type: None Selected   |   {TextBuffer}";
            }
        }

        private void ListBoxSuggestions_DoubleClick(object? sender, EventArgs e)
        {
            CommitSelection();
        }

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

        private void CommitSelection()
        {
            if (listBoxSuggestions.SelectedIndex >= 0 && listBoxSuggestions.SelectedIndex < currentItems.Count)
            {
                var selected = currentItems[listBoxSuggestions.SelectedIndex];
                SelectedTab?.Invoke(this, selected);
            }
        }
    }
}
