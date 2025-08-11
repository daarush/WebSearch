namespace WebSearch
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Create the main form
            var mainForm = new WebSearch();

            // Create NotifyIcon and context menu
            var trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon(Path.Combine(Constants.APP_DATA_FOLDER, Constants.App_Icon_File))
,
                Visible = true,
                Text = "WebSearch"
            };

            var trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Exit", null, (s, e) =>
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();

                if (!mainForm.IsDisposed)
                    mainForm.Close();

                Application.Exit();
            });
            trayIcon.ContextMenuStrip = trayMenu;

            // Optionally: Hide the form on startup if you want it hidden
            // mainForm.WindowState = FormWindowState.Minimized;
            // mainForm.ShowInTaskbar = false;
            // mainForm.Hide();

            // Run the app with the main form and tray icon alive
            Application.Run(mainForm);

            // Cleanup tray icon after form closes
            trayIcon.Dispose();
        }

    }
}