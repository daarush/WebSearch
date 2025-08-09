using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using K4os.Compression.LZ4;

namespace WebSearch
{
    public partial class WebSearch : Form
    {
        private const uint MOD_CONTROL = 0x0002;
        private const uint WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 1;
        private readonly uint spaceBar = (uint)Keys.Space;

        private string defaultBrowserURL = string.Empty;
        string[] chromiumBrowsers = { "chrome", "chromium", "msedge", "brave", "opera", "vivaldi" };
        private bool isChromiumBrowser = false;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                const int WS_EX_TOOLWINDOW = 0x00000080;
                cp.ExStyle |= WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        public WebSearch()
        {
            InitializeComponent();
        }

        private void print(string msg)
        {
            Debug.WriteLine(msg);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            print("APP Started!");
            ShowInTaskbar = false;

            defaultBrowserURL = GetSystemDefaultBrowser();
            bool registered = RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL, spaceBar);
            if (!registered)
                print("Failed to register hotkey");

            WindowState = FormWindowState.Minimized;
            this.Hide();

            string sessionFile = getFirefoxSessionFile();
            print($"Firefox session file: {sessionFile}");

            if (File.Exists(sessionFile))
            {
                ReadFirefoxOpenTabs(sessionFile);
            }
            else
            {
                print("Firefox recovery.jsonlz4 session file not found.");
            }
        }

        internal string GetSystemDefaultBrowser()
        {
            string name = string.Empty;
            RegistryKey? regKey = null;

            try
            {
                var regDefault = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.htm\\UserChoice", false);
                var stringDefault = regDefault?.GetValue("ProgId") as string;

                if (!string.IsNullOrEmpty(stringDefault))
                {
                    regKey = Registry.ClassesRoot.OpenSubKey(stringDefault + "\\shell\\open\\command", false);
                    var value = regKey?.GetValue(null);
                    if (value != null)
                    {
                        var valueStr = value.ToString();
                        if (!string.IsNullOrEmpty(valueStr))
                        {
                            name = valueStr.ToLower().Replace("" + (char)34, "");

                            if (!name.EndsWith("exe"))
                                name = name.Substring(0, name.LastIndexOf(".exe") + 4);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                name = string.Format("ERROR: An exception of type: {0} occurred in method: {1} in the following module: {2}", ex.GetType(), ex.TargetSite, this.GetType());
            }
            finally
            {
                if (regKey != null)
                    regKey.Close();
            }

            return name;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                this.Show();
                WindowState = FormWindowState.Normal;
                Activate();
                MainTextBox.Focus();
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            base.OnFormClosing(e);
        }

        private void MainTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string searchValue = MainTextBox.Text;
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    searchInANewTab(searchValue);
                    MainTextBox.Text = "";
                }

                WindowState = FormWindowState.Minimized;
                this.Hide();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;

                WindowState = FormWindowState.Minimized;
                this.Hide();
            }
        }

        private void searchInANewTab(string searchValue)
        {
            bool isChromiumBrowser = chromiumBrowsers.Any(name => defaultBrowserURL.Contains(name));
            print($"Default Browser URL: {defaultBrowserURL}");
            print($"Is Chromium Browser: {isChromiumBrowser}");

            if (isChromiumBrowser)
            {
                Process.Start(defaultBrowserURL, "\"? searchterm\"");
            }
            else
            {
                var psi = new ProcessStartInfo
                {
                    FileName = defaultBrowserURL,
                    Arguments = $"https://google.com/search?q={Uri.EscapeDataString(searchValue)}",
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }

        protected string getFirefoxSessionFile()
        {
            string firefoxProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Mozilla\\Firefox\\Profiles";
            DirectoryInfo firefoxInfo = new DirectoryInfo(firefoxProfilePath);
            var getRecentProfile = firefoxInfo.GetDirectories()
                .OrderByDescending(d => d.LastWriteTime)
                .First().FullName;

            var getTabSessionFile = Path.Combine(getRecentProfile, "sessionstore-backups", "recovery.jsonlz4");
            return getTabSessionFile;
        }



        private void ReadFirefoxOpenTabs(string filePath)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);

                if (fileBytes.Length < 12)
                {
                    print("Session file too small or invalid.");
                    return;
                }

                // mozLz40\0 (8 bytes) then 4 bytes little-endian uncompressed size
                int uncompressedSize = BitConverter.ToInt32(fileBytes, 8);
                if (uncompressedSize <= 0)
                {
                    print("Invalid uncompressed size in header.");
                    return;
                }

                const int headerSize = 12; // 8 magic + 4 size
                int compressedLength = fileBytes.Length - headerSize;
                if (compressedLength <= 0)
                {
                    print("No compressed data found.");
                    return;
                }

                // copy compressed part
                byte[] compressedData = new byte[compressedLength];
                Array.Copy(fileBytes, headerSize, compressedData, 0, compressedLength);

                // allocate exact buffer for decompressed data
                byte[] decompressedData = new byte[uncompressedSize];

                int decodedLength = LZ4Codec.Decode(
                    compressedData, 0, compressedLength,
                    decompressedData, 0, uncompressedSize);

                if (decodedLength <= 0)
                {
                    print("LZ4 decompression returned zero or negative length.");
                    return;
                }

                string json = Encoding.UTF8.GetString(decompressedData, 0, decodedLength);

                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (!root.TryGetProperty("windows", out var windows))
                {
                    print("No 'windows' property in session JSON.");
                    return;
                }

                foreach (var window in windows.EnumerateArray())
                {
                    if (!window.TryGetProperty("tabs", out var tabs))
                        continue;

                    print($"Window has {tabs.GetArrayLength()} tabs:");
                    foreach (var tab in tabs.EnumerateArray())
                    {
                        int index = 1;
                        if (tab.TryGetProperty("index", out var idxProp))
                            index = idxProp.GetInt32();

                        if (!tab.TryGetProperty("entries", out var entries))
                            continue;

                        if (index <= 0 || index > entries.GetArrayLength())
                            continue;

                        var currentEntry = entries[index - 1];

                        string url = currentEntry.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";
                        string title = currentEntry.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";

                        print($"  - {title}: {url}");
                    }
                }
            }
            catch (Exception ex)
            {
                print("Error reading Firefox tabs: " + ex.ToString());
            }
        }
    }
}
