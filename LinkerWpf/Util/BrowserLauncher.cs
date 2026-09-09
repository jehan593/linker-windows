using System;
using System.Diagnostics;
using Linker.Data.Repositories;

namespace Linker.Util
{
    public static class BrowserLauncher
    {
        public static bool TryOpen(BrowserListItem browser, string url)
        {
            var exePath = !string.IsNullOrWhiteSpace(browser.CustomExecutablePath)
                ? browser.CustomExecutablePath
                : browser.SystemExecutablePath;
            try
            {
                var info = new ProcessStartInfo(exePath) { UseShellExecute = true };
                if (!string.IsNullOrWhiteSpace(browser.ExtraArguments))
                {
                    info.Arguments = browser.ExtraArguments;
                }
                info.Arguments += " " + url;
                Process.Start(info);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}