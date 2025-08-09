using K4os.Compression.LZ4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSearch
{
    public class FirefoxHelper
    {
        public static string getFirefoxSessionFile()
        {
            string firefoxProfilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla", "Firefox", "Profiles");
            DirectoryInfo firefoxInfo = new DirectoryInfo(firefoxProfilePath);
            var getRecentProfile = firefoxInfo.GetDirectories()
                .OrderByDescending(d => d.LastWriteTime)
                .FirstOrDefault()?.FullName ?? "";

            var getTabSessionFile = Path.Combine(getRecentProfile, "sessionstore-backups", "recovery.jsonlz4");
            return getTabSessionFile;
        }

        public static List<TabInfo> ReadFirefoxOpenTabs(string filePath)
        {
            var tabsList = new List<TabInfo>();

            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                if (fileBytes.Length < 12) return tabsList;

                int uncompressedSize = BitConverter.ToInt32(fileBytes, 8);
                if (uncompressedSize <= 0) return tabsList;

                const int headerSize = 12;
                int compressedLength = fileBytes.Length - headerSize;
                if (compressedLength <= 0) return tabsList;

                byte[] compressedData = new byte[compressedLength];
                Array.Copy(fileBytes, headerSize, compressedData, 0, compressedLength);

                byte[] decompressedData = new byte[uncompressedSize];
                int decodedLength = LZ4Codec.Decode(
                    compressedData, 0, compressedLength,
                    decompressedData, 0, uncompressedSize);

                if (decodedLength <= 0) return tabsList;

                string json = Encoding.UTF8.GetString(decompressedData, 0, decodedLength);

                using JsonDocument doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (!root.TryGetProperty("windows", out var windows)) return tabsList;

                foreach (var window in windows.EnumerateArray())
                {
                    if (!window.TryGetProperty("tabs", out var tabs)) continue;

                    foreach (var tab in tabs.EnumerateArray())
                    {
                        int index = tab.TryGetProperty("index", out var idxProp) ? idxProp.GetInt32() : 1;
                        if (!tab.TryGetProperty("entries", out var entries)) continue;
                        if (index <= 0 || index > entries.GetArrayLength()) continue;

                        var currentEntry = entries[index - 1];
                        string url = currentEntry.TryGetProperty("url", out var u) ? u.GetString() ?? "" : "";
                        string title = currentEntry.TryGetProperty("title", out var t) ? t.GetString() ?? "" : "";

                        tabsList.Add(new TabInfo { Title = title, Url = url });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error reading Firefox tabs: " + ex.ToString());
            }

            return tabsList;
        }

        public static List<TabInfo> GetFirefoxOpenTabs()
        {
            string sessionFile = getFirefoxSessionFile();
            if (File.Exists(sessionFile))
                return ReadFirefoxOpenTabs(sessionFile);
            else
                return new List<TabInfo>();
        }

    }
}
