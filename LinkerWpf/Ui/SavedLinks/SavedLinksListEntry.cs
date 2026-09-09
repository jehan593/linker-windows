using Linker.Data.Entities;

namespace Linker.Ui.SavedLinks
{
    public abstract class SavedLinksListEntry { }

    public class DayHeaderEntry : SavedLinksListEntry
    {
        public string Label { get; }
        public DayHeaderEntry(string label) { Label = label; }
    }

    public class SavedLinkRowEntry : SavedLinksListEntry
    {
        public SavedLinkEntity Link { get; }
        public string FormattedTime { get; }
        public string SearchQuery { get; }

        public SavedLinkRowEntry(SavedLinkEntity link, string formattedTime, string searchQuery)
        {
            Link = link;
            FormattedTime = formattedTime;
            SearchQuery = searchQuery;
        }
    }
}
