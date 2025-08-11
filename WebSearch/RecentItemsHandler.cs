using System.Text.Json;

namespace WebSearch
{
    public static class RecentItemsHandler
    {
        private static readonly object _fileLock = new();
        private static readonly string _recentFilePath = Path.Combine(Constants.AppDataFolder, Constants.RecentItemsFileName);

        public static void AddToRecentSites(RecentItem item)
        {
            item.LastAccessed = DateTime.UtcNow;
            WebSearch.recentSites.RemoveAll(x => string.Equals(x.Url, item.Url, StringComparison.OrdinalIgnoreCase));
            WebSearch.recentSites.Insert(0, item);

            if (WebSearch.recentSites.Count > SettingsHandler.CurrentSettings.MaxRecentItems)
                WebSearch.recentSites = WebSearch.recentSites.Take(SettingsHandler.CurrentSettings.MaxRecentItems).ToList();

            UpdateRecentItemsJSONFile();
        }


        public static List<RecentItem> LoadRecentItemsJSONFile()
        {
            Directory.CreateDirectory(Constants.AppDataFolder);

            lock (_fileLock)
            {
                if (!File.Exists(_recentFilePath)) return new List<RecentItem>();

                try
                {
                    var json = File.ReadAllText(_recentFilePath);
                    if (string.IsNullOrWhiteSpace(json)) return new List<RecentItem>();
                    var items = JsonSerializer.Deserialize<List<RecentItem>>(json);
                    return items ?? new List<RecentItem>();
                }
                catch (Exception ex)
                {
                    Logger.Print($"Error reading recent items from file: {ex.Message}");
                    return new List<RecentItem>();
                }
            }
        }

        private static void UpdateRecentItemsJSONFile()
        {
            Directory.CreateDirectory(Constants.AppDataFolder);

            lock (_fileLock)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(WebSearch.recentSites, options);
                    File.WriteAllText(_recentFilePath, json);
                }
                catch (Exception ex)
                {
                    Logger.Print($"Error writing recent items to file: {ex.Message}");
                }
            }
        }
    }
}
