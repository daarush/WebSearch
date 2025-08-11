using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSearch
{
    public class FrequentSitesHelper
    {
        public static List<FrequentSitesItem> GetFrequentSites()
        {
            var frequentSites = new List<FrequentSitesItem>();
            string profilePath = FirefoxHelper.GetProfilePath();
            if (string.IsNullOrEmpty(profilePath)) return frequentSites;

            string placesDb = Path.Combine(profilePath, "places.sqlite");
            if (!File.Exists(placesDb)) return frequentSites;

            string tempDb = Path.Combine(Path.GetTempPath(), $"places_copy_{Guid.NewGuid()}.sqlite");

            try
            {
                using (var source = new FileStream(placesDb, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var dest = new FileStream(tempDb, FileMode.Create, FileAccess.Write, FileShare.None))
                    source.CopyTo(dest);

                using var connection = new SqliteConnection($"Data Source={tempDb};Mode=ReadOnly;Cache=Shared;");
                connection.Open();

                string query = @$"
                    SELECT url, title, visit_count
                    FROM moz_places
                    WHERE visit_count > 0
                    ORDER BY visit_count DESC
                    LIMIT {SettingsHandler.CurrentSettings.MaxFrequentItems};
                ";

                using var cmd = new SqliteCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string url = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string title = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    int visitCount = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                    frequentSites.Add(new FrequentSitesItem
                    {
                        Url = url,
                        Title = title,
                        VisitCount = visitCount
                    });
                }
            }
            catch (Exception ex)
            {
                Logger.Print("Error reading frequent sites: " + ex.Message);
            }
            finally
            {
                try { if (File.Exists(tempDb)) File.Delete(tempDb); } catch { }
            }

            return frequentSites;
        }

    }
}
