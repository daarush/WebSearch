using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace WebSearch
{
    public class HistoryHelper
    {
        public static List<HistoryItem> GetHistory()
        {
            var historyEntry = new List<HistoryItem>();
            string profilePath = FirefoxHelper.GetProfilePath();
            if (string.IsNullOrEmpty(profilePath)) return historyEntry;

            string placesDb = Path.Combine(profilePath, "places.sqlite");
            if (!File.Exists(placesDb)) return historyEntry;

            string tempDb = Path.Combine(Path.GetTempPath(), $"places_copy_{Guid.NewGuid()}.sqlite");

            try
            {
                using (var source = new FileStream(placesDb, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var dest = new FileStream(tempDb, FileMode.Create, FileAccess.Write, FileShare.None))
                    source.CopyTo(dest);

                using var connection = new SqliteConnection($"Data Source={tempDb};Mode=ReadOnly;Cache=Shared;");
                connection.Open();

                string query = @"
                    SELECT p.url, p.title, h.visit_date
                    FROM moz_places p
                    JOIN moz_historyvisits h ON p.id = h.place_id
                    ORDER BY h.visit_date DESC
                    LIMIT 1000;
                ";

                using var cmd = new SqliteCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    long visitDateMicroseconds = reader.GetInt64(2);
                    DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(visitDateMicroseconds / 1000);
                    DateTime visitDateTime = dateTimeOffset.LocalDateTime;

                    string url = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string title = reader.IsDBNull(1) ? "" : reader.GetString(1);

                    historyEntry.Add(new HistoryItem { Title = title, Url = url, VisitDate = visitDateTime });
                }
            }
            catch (Exception ex)
            {
                Logger.Print("Error reading History: " + ex.Message);
            }
            finally
            {
                try { if (File.Exists(tempDb)) File.Delete(tempDb); } catch { }
            }

            return historyEntry;
        }

    }

}
