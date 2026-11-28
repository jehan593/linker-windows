using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Linker.Data.Repositories;

namespace Linker.Util
{
    public static class BrowserLauncher
    {
        private const int AsfwAny = -1;

        [DllImport("user32.dll")]
        private static extern bool AllowSetForegroundWindow(int processId);

        public static bool TryOpen(BrowserListItem browser, string url)
        {
            return TryOpen(browser, url, null);
        }

        public static bool TryOpenPrivate(BrowserListItem browser, string url)
        {
            if (string.IsNullOrWhiteSpace(browser.PrivateWindowArgument)) return false;
            return TryOpen(browser, url, browser.PrivateWindowArgument);
        }

        private static bool TryOpen(BrowserListItem browser, string url, string leadingArgument)
        {
            var exePath = !string.IsNullOrWhiteSpace(browser.CustomExecutablePath)
                ? browser.CustomExecutablePath
                : browser.SystemExecutablePath;
            try
            {
                var info = new ProcessStartInfo(exePath) { UseShellExecute = true };
                var args = browser.ExtraArguments ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(leadingArgument))
                {
                    args = string.IsNullOrWhiteSpace(args)
                        ? leadingArgument
                        : leadingArgument + " " + args;
                }

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

                // The chooser is the foreground window on click. Let the browser,
                // including an already-running instance, raise its own window.
                AllowSetForegroundWindow(AsfwAny);
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
