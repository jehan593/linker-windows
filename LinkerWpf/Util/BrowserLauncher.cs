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
                var args = browser.ExtraArguments ?? string.Empty;
                var quotedUrl = CommandLineUtils.QuoteArgument(url);
                if (args.Contains("%1"))
                {
                    info.Arguments = args.Replace("%1", quotedUrl);
                }
                else
                {
                    info.Arguments = string.IsNullOrWhiteSpace(args)
                        ? quotedUrl
                        : args + " " + quotedUrl;
                }
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