using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace WebSearch
{
    public partial class DropDownForm : Form
    {
        private List<TabInfo> currentItems = new();
        private List<DisplayItem> displayItems = new();

        public event EventHandler<TabInfo>? SelectedTab;
        private string TextBuffer = string.Empty;

        private record DisplayItem(string Text, TabInfo? Info, bool IsWebSearch = false);

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
            // de-duplicate + sort
            currentItems = suggestions
                .GroupBy(item => item.Url, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderByDescending(item =>
                    item is RecentItem ? 5 :
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
            int getRecentCount = currentItems.OfType<RecentItem>().Count();

            TextBuffer = $"Total Items: {totalItemsFiltered}  [{getRecentCount} Recent Entries, {getTabCount} Open Tabs, {getBookmarkCount} Bookmarks, {getHistoryCount} History Items, {getFrequentCount} Frequent Sites]";
            SelectedIndexInfoLabel.Text = TextBuffer;

            // build displayItems (keeps mapping safe)
            displayItems = new List<DisplayItem>(currentItems.Count + 1);
            foreach (var item in currentItems)
            {
                string text;
                switch (item)
                {
                    case HistoryItem historyItem:
                        text = string.IsNullOrWhiteSpace(historyItem.Title)
                            ? $"     [{historyItem.VisitDate:yyyy-MM-dd HH:mm}]  {historyItem.Url}"
                            : $"     [{historyItem.VisitDate:yyyy-MM-dd HH:mm}]  {historyItem.Title}    —    {historyItem.Url}";
                        break;
                    case FrequentSitesItem frequent:
                        text = string.IsNullOrWhiteSpace(frequent.Title)
                            ? $"    ⚡ {frequent.Url}"
                            : $"    ⚡ {frequent.Title} — {frequent.Url} (Visits: {frequent.VisitCount})";
                        break;
                    case RecentItem recent:
                        text = string.IsNullOrWhiteSpace(recent.Title)
                            ? $"    🔥 {recent.Url}"
                            : $"    🔥 {recent.Title} — {recent.Url}";
                        break;
                    case OpenTab openTab:
                        text = string.IsNullOrWhiteSpace(openTab.Title)
                            ? $"    🗂️ {openTab.Url}"
                            : $"    🗂️ {openTab.Title} — {openTab.Url}";
                        break;
                    case BookmarkItem bookmark:
                        text = string.IsNullOrWhiteSpace(bookmark.Title)
                            ? $"    ★ {bookmark.Url}"
                            : $"    ★ {bookmark.Title} — {bookmark.Url}";
                        break;
                    default:
                        text = string.IsNullOrWhiteSpace(item.Title)
                            ? item.Url
                            : $"    {item.Title} — {item.Url}";
                        break;
                }

                displayItems.Add(new DisplayItem(text, item, false));
            }

            // Optionally insert the "Web Search" special entry at a stable index
            if (displayItems.Count > 1)
            {
                int insertIndex = Math.Clamp(Constants.SearchWebIndexPosition, 0, displayItems.Count);
                displayItems.Insert(insertIndex, new DisplayItem("    🌐  Web Search", null, true));
            }

            // push to ListBox
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
                var outputString = selectedItem switch
                {
                    BookmarkItem _ => "Bookmark",
                    HistoryItem _ => "History",
                    FrequentSitesItem _ => "Frequent Site",
                    OpenTab _ => "Opened Tab",
                    RecentItem _ => "Recent Entry",
                    _ => "Unknown Type"
                };

                SelectedIndexInfoLabel.Text = $"Type: {outputString}    |   {TextBuffer}";
            }
            else
            {
                SelectedIndexInfoLabel.Text = $"Type: None Selected   |   {TextBuffer}";
            }
        }

        private void ListBoxSuggestions_DoubleClick(object? sender, EventArgs e) => CommitSelection();

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
            int selectedIndex = listBoxSuggestions.SelectedIndex;
            if (selectedIndex < 0 || selectedIndex >= displayItems.Count) return;

            var di = displayItems[selectedIndex];
            if (di.IsWebSearch)
            {
                if (this.Owner is WebSearch webSearchForm)
                {
                    string val = webSearchForm.GetMainTextBoxText();
                    if (!string.IsNullOrWhiteSpace(val))

                        {
                            BrowserHelper.searchInANewTab(BrowserHelper.GetSystemDefaultBrowser(), val, false);
                        this.Hide();
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
    }
}
