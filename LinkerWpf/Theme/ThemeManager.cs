using System.Windows;

namespace Linker.Theme
{
    public static class ThemeManager
    {
        private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string LightThemeValueName = "AppsUseLightTheme";
        private static readonly string[] DarkColors = new[] { "Theme/ColorsDark.xaml" };
        private static readonly string[] LightColors = new[] { "Theme/ColorsLight.xaml" };

        public static bool IsDarkActive { get; private set; }

        public static bool IsSystemDarkTheme()
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath))
            {
                var value = key?.GetValue(LightThemeValueName);
                return value is int i && i == 0 || value == null;
            }
        }

        public static void ApplyCurrentSystemTheme()
        {
            Apply(IsSystemDarkTheme());
        }

        public static void Apply(bool darkTheme)
        {
            IsDarkActive = darkTheme;

            var app = Application.Current;
            var dicts = app.Resources.MergedDictionaries;

            // Remove old color dictionary (index 0)
            if (dicts.Count > 0)
            {
                dicts.RemoveAt(0);
            }

            // Insert new color dictionary at index 0
            var colorDict = new ResourceDictionary
            {
                Source = new System.Uri(darkTheme ? DarkColors[0] : LightColors[0], System.UriKind.Relative)
            };
            dicts.Insert(0, colorDict);

            // Update window title bars
            foreach (Window window in app.Windows)
            {
                ImmersiveDarkMode.Apply(window, darkTheme);
            }
        }

        public static void WatchForSystemThemeChanges()
        {
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += (sender, e) =>
            {
                if (e.Category == Microsoft.Win32.UserPreferenceCategory.General)
                {
                    Application.Current.Dispatcher.Invoke(ApplyCurrentSystemTheme);
                }
            };
        }
    }
}
