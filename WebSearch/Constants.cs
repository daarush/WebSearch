namespace WebSearch
{
    public static class Constants
    {
        // Hotkey and Window constants
        public const uint MOD_CONTROL = 0x0002;
        public const uint WM_HOTKEY = 0x0312;
        public const int HOTKEY_ID = 1;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public static readonly uint VK_SPACE = (uint)Keys.Space;

        // UI Colors
        public static readonly Color ColorBackground = Color.Black;
        public static readonly Color ColorForeground = Color.White;
        public static readonly Color ColorDarkPurple = Color.FromArgb(40, 0, 40);

        // DropDownForm layout
        public static readonly Size DropDownFormSize = new Size(1025, 200);
        public static readonly Point DropDownFormLocation = new Point(-3, 1);

        // BrowserHelper registry keys
        public const string RegKeyUserChoice = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.htm\UserChoice";
        public const string RegKeyShellOpenCommand = @"shell\open\command";

        // URLs
        public const string GoogleSearchUrl = "https://google.com/search?q=";

        // Error formatting
        public const string ErrorMessageFormat = "ERROR: An exception of type: {0} occurred in method: {1} in the following module: {2}";

        // Default settings values
        public const int DefaultMaxRecentItems = 5;
        public const int DefaultMaxFrequentItems = 100;
        public const int DefaultMaxTotalItems = 1000;
        public const double DefaultFormOpacity = 0.9;
        public const int PositionOfWebSearchItem = 1;

        // App data paths and files
        public static readonly string AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebSearch");
        public const string RecentItemsFileName = "recentItems.json";
        public const string AppIconFileName = "websearch-icon.ico";
        public const string LoggerFileName = "log.txt";
    }
}
