namespace WebSearch
{
    partial class WebSearch
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            MainTextBox = new RichTextBox();
            SuspendLayout();
            // 
            // MainTextBox
            // 
            MainTextBox.BackColor = Color.Black;
            MainTextBox.BorderStyle = BorderStyle.None;
            MainTextBox.ForeColor = Color.White;
            MainTextBox.Location = new Point(12, 12);
            MainTextBox.Multiline = false;
            MainTextBox.Name = "MainTextBox";
            MainTextBox.ScrollBars = RichTextBoxScrollBars.None;
            MainTextBox.Size = new Size(1000, 40);
            MainTextBox.TabIndex = 0;
            MainTextBox.Text = "";
            MainTextBox.KeyDown += MainTextBox_KeyDown;
            // 
            // WebSearch
            // 
            AutoScaleDimensions = new SizeF(11F, 28F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1013, 56);
            ControlBox = false;
            Controls.Add(MainTextBox);
            Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4);
            Name = "WebSearch";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            WindowState = FormWindowState.Minimized;
            Load += Form1_Load;
            ResumeLayout(false);
        }

        #endregion

        private RichTextBox MainTextBox;
    }
}
