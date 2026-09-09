using System.Diagnostics;
using Microsoft.Win32;

namespace Linker.Util
{
    public static class DefaultBrowserRegistration
    {
        private const string StartMenuInternetKeyPath = @"Software\Clients\StartMenuInternet\" + AppIdentity.RegistryAppId;
        private const string RegisteredApplicationsKeyPath = @"Software\RegisteredApplications";
        private const string ProgIdKeyPath = @"Software\Classes\" + AppIdentity.ProgId;
        private const string UrlAssociationsKeyPath = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations";

        public static void EnsureRegistered()
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath)) return;

            var openCommand = "\"" + exePath + "\" \"%1\"";

            using (var clientKey = Registry.CurrentUser.CreateSubKey(StartMenuInternetKeyPath))
            {
                clientKey.SetValue(null, AppIdentity.AppName);
                using (var iconKey = clientKey.CreateSubKey("DefaultIcon"))
                {
                    iconKey.SetValue(null, exePath + ",0");
                }
                using (var shellKey = clientKey.CreateSubKey(@"shell\open\command"))
                {
                    shellKey.SetValue(null, openCommand);
                }
                using (var capabilitiesKey = clientKey.CreateSubKey("Capabilities"))
                {
                    capabilitiesKey.SetValue("ApplicationName", AppIdentity.AppName);
                    capabilitiesKey.SetValue("ApplicationDescription",
                        "Shows a chooser before opening links, so you can edit, save, or pick a browser first.");
                    using (var urlAssocKey = capabilitiesKey.CreateSubKey("URLAssociations"))
                    {
                        urlAssocKey.SetValue("http", AppIdentity.ProgId);
                        urlAssocKey.SetValue("https", AppIdentity.ProgId);
                    }
                }
            }

            using (var registeredAppsKey = Registry.CurrentUser.CreateSubKey(RegisteredApplicationsKeyPath))
            {
                registeredAppsKey.SetValue(AppIdentity.AppName, StartMenuInternetKeyPath + @"\Capabilities");
            }

            using (var progIdKey = Registry.CurrentUser.CreateSubKey(ProgIdKeyPath))
            {
                progIdKey.SetValue(null, "URL:Linker Protocol");
                progIdKey.SetValue("URL Protocol", "");
                using (var iconKey = progIdKey.CreateSubKey("DefaultIcon"))
                {
                    iconKey.SetValue(null, exePath + ",0");
                }
                using (var shellKey = progIdKey.CreateSubKey(@"shell\open\command"))
                {
                    shellKey.SetValue(null, openCommand);
                }
            }
        }

        public static bool IsDefault()
        {
            return IsDefaultFor("http") && IsDefaultFor("https");
        }

        private static bool IsDefaultFor(string scheme)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(UrlAssociationsKeyPath + @"\" + scheme + @"\UserChoice"))
            {
                var progId = key?.GetValue("ProgId") as string;
                return string.Equals(progId, AppIdentity.ProgId, System.StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void OpenDefaultAppsSettings()
        {
            Process.Start(new ProcessStartInfo("ms-settings:defaultapps") { UseShellExecute = true });
        }
    }
}
