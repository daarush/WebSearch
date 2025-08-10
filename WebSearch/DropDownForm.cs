namespace WebSearch
{
    public partial class DropDownForm : Form
    {
        private List<TabInfo> currentItems = new();

        public event EventHandler<TabInfo>? SelectedTab;

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
            TopMost = true;

            ResultsPanel.BackColor = Constants.DarkPurple;
            ResultsText.BackColor = Constants.DarkPurple;
            ResultsText.ForeColor = Constants.ForegroundColor;
            ResultsText.TextAlign = ContentAlignment.MiddleCenter;

            listBoxSuggestions.Click += ListBoxSuggestions_Click;
            listBoxSuggestions.DoubleClick += ListBoxSuggestions_DoubleClick;
            listBoxSuggestions.KeyDown += ListBoxSuggestions_KeyDown;
        }

        public void UpdateSuggestions(IEnumerable<TabInfo> suggestions)
        {
            int totalItemsFiltered = suggestions.Count();
            ResultsText.Text = totalItemsFiltered == 1 ? "1 result found" : $"{totalItemsFiltered} results found";

            currentItems = suggestions
                .OrderByDescending(item => item is FrequentSitesItem)
                .ThenBy(item => item.Title) 
                .ToList();

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

        private void ListBoxSuggestions_Click(object? sender, EventArgs e)
        {
            CommitSelection();
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

        public void ShowAllTabs(List<TabInfo> tabs)
        {
            currentItems = new List<TabInfo>(tabs);
            listBoxSuggestions.BeginUpdate();
            listBoxSuggestions.Items.Clear();

            foreach (var tab in currentItems)
            {
                listBoxSuggestions.Items.Add(string.IsNullOrWhiteSpace(tab.Title) ? tab.Url : $"{tab.Title} — {tab.Url}");
            }

            listBoxSuggestions.EndUpdate();

            if (currentItems.Count > 0)
            {
                if (!Visible) Show();
                listBoxSuggestions.SelectedIndex = 0;
            }
            else
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
