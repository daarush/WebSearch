using Microsoft.Win32;

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
                Icon = new System.Drawing.Icon(Path.Combine(Constants.AppDataFolder, Constants.AppIconFileName))
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

            if (SettingsHandler.CurrentSettings.LaunchOnStartup)
            {
                StartupManager.EnableStartup();
            }
            else
            {
                StartupManager.DisableStartup();
            }

            // Run the app with the main form and tray icon alive
            Application.Run(mainForm);

            // Cleanup tray icon after form closes
            trayIcon.Dispose();
        }


        public static class StartupManager
        {
            private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
            private const string AppName = "WebSearch"; 

            public static void EnableStartup()
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (key == null) return; // something's wrong

                    string exePath = Application.ExecutablePath;
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
            }

            public static void DisableStartup()
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (key == null) return;

                    if (key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName);
                    }
                }
            }

            public static bool IsStartupEnabled()
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, false))
                {
                    if (key == null) return false;

                    var value = key.GetValue(AppName);
                    if (value == null) return false;

                    string exePath = Application.ExecutablePath;
                    return value.ToString().Equals($"\"{exePath}\"", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

    }
}