namespace Linker.Data.Entities
{
    public class SavedLinkEntity
    {
        public long Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public long SavedAtMillis { get; set; }
    }
}
