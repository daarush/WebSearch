using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace WebSearch
{
    public class BookmarkHelper
    {
        public static List<BookmarkItem> GetBookmarks()
        {
            var bookmarks = new List<BookmarkItem>();
            string profilePath = FirefoxHelper.GetProfilePath();
            if (string.IsNullOrEmpty(profilePath)) return bookmarks;

            string placesDb = Path.Combine(profilePath, "places.sqlite");
            if (!File.Exists(placesDb)) return bookmarks;

            string tempDb = Path.Combine(Path.GetTempPath(), $"places_copy_{Guid.NewGuid()}.sqlite");

            try
            {
                using (var source = new FileStream(placesDb, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var dest = new FileStream(tempDb, FileMode.Create, FileAccess.Write, FileShare.None))
                    source.CopyTo(dest);

                using var connection = new SqliteConnection($"Data Source={tempDb};Mode=ReadOnly;Cache=Shared;");
                connection.Open();

                string query = @"
                    SELECT b.title, p.url
                    FROM moz_bookmarks b
                    JOIN moz_places p ON b.fk = p.id
                    WHERE b.type = 1
                    ORDER BY b.dateAdded DESC
                ";

                using var cmd = new SqliteCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string title = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    string url = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    bookmarks.Add(new BookmarkItem { Title = title, Url = url });
                }
            }
            catch (Exception ex)
            {
                Logger.Print("Error reading bookmarks: " + ex.Message);
            }
            finally
            {
                try { if (File.Exists(tempDb)) File.Delete(tempDb); } catch { }
            }

            return bookmarks;
        }
    }
}
