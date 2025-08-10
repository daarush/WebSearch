using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WebSearch
{
    public partial class DropDownForm : Form
    {
        private List<TabInfo> currentItems = new();

        public event EventHandler<TabInfo>? SelectedTab;

        public DropDownForm()
        {
            InitializeComponent();
            listBoxSuggestions = new NoScrollBarListbox
            {
                BackColor = Color.Black,
                BorderStyle = BorderStyle.None,
                ForeColor = Color.White,
                FormattingEnabled = true,
                Location = new Point(-3, 1),
                Name = "listBoxSuggestions",
                Size = new Size(1025, 200),
                TabIndex = 0,
                ScrollAlwaysVisible = true,
            };
            Controls.Add(listBoxSuggestions);

            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            TopMost = true;

            listBoxSuggestions.Click += ListBoxSuggestions_Click;
            listBoxSuggestions.DoubleClick += ListBoxSuggestions_DoubleClick;
            listBoxSuggestions.KeyDown += ListBoxSuggestions_KeyDown;
        }

        public void UpdateSuggestions(IEnumerable<TabInfo> suggestions)
        {
            currentItems = new List<TabInfo>(suggestions);
            listBoxSuggestions.BeginUpdate();
            listBoxSuggestions.Items.Clear();

            foreach (var item in currentItems)
            {
                // display format: Title — Url (trim if too long if you want)
                listBoxSuggestions.Items.Add(string.IsNullOrWhiteSpace(item.Title) ? item.Url : $"{item.Title} — {item.Url}");
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
