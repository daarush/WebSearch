namespace WebSearch
{
    public static class Constants
    {
        // WebSearch constants
        public const uint MOD_CONTROL = 0x0002;
        public const uint WM_HOTKEY = 0x0312;
        public const int HOTKEY_ID = 1;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public static readonly uint SPACE_BAR = (uint)Keys.Space;

        // DropDownForm constants
        public static readonly Color BackgroundColor = Color.Black;
        public static readonly Color ForegroundColor = Color.White;
        public static readonly Color DarkPurple = Color.FromArgb(40, 0, 40);

        public static readonly Size DropDownSize = new Size(1025, 200);
        public static readonly Point DropDownLocation = new Point(-3, 1);

        public const double FormOpacity = 0.9;

        //BrowserHelper constants
        public const string RegKeyUserChoice = @"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.htm\UserChoice";
        public const string RegKeyShellOpenCommand = @"shell\open\command";

        public const string GoogleSearchUrl = "https://google.com/search?q=";

        public const string ErrorMessageFormat = "ERROR: An exception of type: {0} occurred in method: {1} in the following module: {2}";
    }
}
