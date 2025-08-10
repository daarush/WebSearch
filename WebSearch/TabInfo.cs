namespace WebSearch
{
    public class TabInfo
    {
        public required string Title { get; set; }
        public required string Url { get; set; }
    }

    public class OpenTab : TabInfo
    {}

    public class BookmarkItem : TabInfo
    {}

    public class HistoryItem : TabInfo
    {
        public DateTime VisitDate { get; set; }
    }

    public class FrequentSitesItem : TabInfo
    {
        public int VisitCount { get; set; }
    }
}
