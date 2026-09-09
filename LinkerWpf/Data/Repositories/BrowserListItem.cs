using System.Windows.Media.Imaging;
using Linker.Util;

namespace Linker.Data.Repositories
{
    public class BrowserListItem
    {
        public string Id { get; }
        public string SystemLabel { get; }
        public string DisplayLabel { get; }
        public string SystemExecutablePath { get; }
        public string CustomExecutablePath { get; }
        public string CustomIconPath { get; }
        public string ExtraArguments { get; }
        public BitmapSource Icon { get; }
        public bool Hidden { get; }
        public int OrderIndex { get; }
        public bool IsCustom { get; }

        public BrowserListItem(
            string id, string systemLabel, string displayLabel, string systemExecutablePath,
            string customExecutablePath, string customIconPath, string extraArguments,
            BitmapSource icon, bool hidden, int orderIndex, bool isCustom)
        {
            Id = id;
            SystemLabel = systemLabel;
            DisplayLabel = displayLabel;
            SystemExecutablePath = systemExecutablePath;
            CustomExecutablePath = customExecutablePath;
            CustomIconPath = customIconPath;
            ExtraArguments = extraArguments;
            Icon = icon;
            Hidden = hidden;
            OrderIndex = orderIndex;
            IsCustom = isCustom;
        }
    }
}
