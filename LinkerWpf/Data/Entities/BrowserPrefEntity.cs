namespace Linker.Data.Entities
{
    public class BrowserPrefEntity
    {
        public string Id { get; set; } = string.Empty;
        public string CustomLabel { get; set; }
        public bool Hidden { get; set; }
        public int OrderIndex { get; set; }
        public string CustomExecutablePath { get; set; }
        public string CustomIconPath { get; set; }
        public string ExtraArguments { get; set; }
        public bool IsCustom { get; set; }
    }
}
