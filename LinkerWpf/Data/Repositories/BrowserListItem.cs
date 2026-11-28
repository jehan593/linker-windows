using System;
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
        public string DefaultArguments { get; }
        public string PrivateWindowArgument { get; }
        public string PrivateWindowArgumentOverride { get; }
        public bool PrivateWindowEnabled { get; }
        public BitmapSource Icon { get; }
        public bool Hidden { get; }
        public int OrderIndex { get; }
        public bool IsCustom { get; }

        public BrowserListItem(
            string id, string systemLabel, string displayLabel, string systemExecutablePath,
            string customExecutablePath, string customIconPath, string extraArguments,
            BitmapSource icon, bool hidden, int orderIndex, bool isCustom,
            string privateWindowArgumentOverride = null, bool? privateWindowEnabled = null,
            string defaultArguments = null)
        {
            Id = id;
            SystemLabel = systemLabel;
            DisplayLabel = displayLabel;
            SystemExecutablePath = systemExecutablePath;
            CustomExecutablePath = customExecutablePath;
            CustomIconPath = customIconPath;
            ExtraArguments = extraArguments;
            DefaultArguments = defaultArguments ?? extraArguments;
            Icon = icon;
            Hidden = hidden;
            OrderIndex = orderIndex;
            IsCustom = isCustom;

            var launchPath = !string.IsNullOrWhiteSpace(customExecutablePath)
                ? customExecutablePath
                : systemExecutablePath;

            var overrideArgument = Clean(privateWindowArgumentOverride);
            var legacyNone = string.Equals(overrideArgument, "none", StringComparison.OrdinalIgnoreCase);
            if (legacyNone) overrideArgument = null;
            PrivateWindowArgumentOverride = overrideArgument;

            var hasTypedFlag = !string.IsNullOrEmpty(overrideArgument);
            var detected = PrivateWindowSupport.GetArgument(launchPath, systemLabel);
            var flag = hasTypedFlag ? overrideArgument : detected;

            // Unticked by default. A previously typed flag still counts as opted in.
            PrivateWindowEnabled = privateWindowEnabled ?? (hasTypedFlag && !legacyNone);
            PrivateWindowArgument = PrivateWindowEnabled && !string.IsNullOrWhiteSpace(flag)
                ? flag
                : null;
        }

        private static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
