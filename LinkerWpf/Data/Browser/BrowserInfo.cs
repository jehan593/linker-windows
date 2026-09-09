using System.Windows.Media.Imaging;

namespace Linker.Data.Browser
{
    public class BrowserInfo
    {
        public string Id { get; }
        public string SystemLabel { get; }
        public string ExecutableCommand { get; }
        public BitmapSource Icon { get; }

        public BrowserInfo(string id, string systemLabel, string executableCommand, BitmapSource icon)
        {
            Id = id;
            SystemLabel = systemLabel;
            ExecutableCommand = executableCommand;
            Icon = icon;
        }
    }
}
