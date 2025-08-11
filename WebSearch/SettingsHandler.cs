using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebSearch
{
    public class SettingsHandler
    {
        private static readonly string _settingsFilePath = Path.Combine(Constants.AppDataFolder, "settings.json");

        private static Settings? _currentSettings;

        public static Settings CurrentSettings
        {
            get
            {
                if (_currentSettings == null)
                    _currentSettings = LoadSettings();
                return _currentSettings;
            }
            set
            {
                _currentSettings = value;
                SaveSettings(value);
            }
        }

        private static Settings LoadSettings()
        {
            try
            {
                Directory.CreateDirectory(Constants.AppDataFolder);

                if (!File.Exists(_settingsFilePath))
                {
                    Logger.Print("Settings file not found, creating default settings.");
                    var defaults = new Settings();
                    SaveSettings(defaults);        
                    return defaults;
                }

                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<Settings>(json);
                return settings ?? new Settings();
            }
            catch (Exception ex)
            {
                Logger.Print($"Error loading settings: {ex.Message}");
                return new Settings();
            }
        }


        private static void SaveSettings(Settings settings)
        {
            try
            {
                Directory.CreateDirectory(Constants.AppDataFolder);
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                Logger.Print($"Error saving settings: {ex.Message}");
            }
        }
    }

    public class Settings
    {
        public int MaxRecentItems { get; set; } = Constants.DefaultMaxRecentItems;
        public int MaxFrequentItems { get; set; } = Constants.DefaultMaxFrequentItems;
        public int MaxTotalItems { get; set; } = Constants.DefaultMaxTotalItems;
        public int PositionOfWebSearchItem { get; set; } = Constants.PositionOfWebSearchItem;
        public double DropDownFormOpacity { get; set; } = Constants.DefaultFormOpacity;
        public bool IncludeRecentItems { get; set; } = true;
        public bool IncludeOpenTabs { get; set; } = true;
        public bool IncludeFrequentItems { get; set; } = true;
        public bool IncludeBookmarks { get; set; } = true;
        public bool IncludeHistory { get; set; } = true;
        public SearchOrder SearchOrder { get; set; } = new SearchOrder();
    }

    public class SearchOrder
    {
        public int OpenTabs { get; set; } = 1;
        public int RecentItems { get; set; } = 2;
        public int FrequentSites { get; set; } = 3;
        public int Bookmarks { get; set; } = 4;
        public int History { get; set; } = 5;
    }
}
