namespace WebSearch
{
    partial class DropDownForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SelectedIndexInfoLabel = new Label();
            ResultsPanel = new Panel();
            ResultsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // SelectedIndexInfoLabel
            // 
            SelectedIndexInfoLabel.BackColor = Color.Transparent;
            SelectedIndexInfoLabel.Dock = DockStyle.Fill;
            SelectedIndexInfoLabel.Location = new Point(0, 0);
            SelectedIndexInfoLabel.Name = "SelectedIndexInfoLabel";
            SelectedIndexInfoLabel.Size = new Size(1026, 26);
            SelectedIndexInfoLabel.TabIndex = 0;
            SelectedIndexInfoLabel.Text = "Results: ";
            // 
            // ResultsPanel
            // 
            ResultsPanel.BackColor = Color.DarkSlateGray;
            ResultsPanel.Controls.Add(SelectedIndexInfoLabel);
            ResultsPanel.ForeColor = Color.Black;
            ResultsPanel.Location = new Point(0, 205);
            ResultsPanel.Name = "ResultsPanel";
            ResultsPanel.Size = new Size(1026, 26);
            ResultsPanel.TabIndex = 1;
            // 
            // DropDownForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            ClientSize = new Size(1025, 230);
            Controls.Add(ResultsPanel);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.None;
            Name = "DropDownForm";
            RightToLeft = RightToLeft.No;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "DropDownForm";
            ResultsPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private NoScrollBarListbox listBoxSuggestions;
        private Label SelectedIndexInfoLabel;
        private Panel ResultsPanel;
    }
}