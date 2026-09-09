using System;
using System.Linq;
using System.Windows;
using Linker.Di;
using Linker.Interceptor;
using Linker.Theme;
using Linker.Util;

namespace Linker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeManager.ApplyCurrentSystemTheme();
            ThemeManager.WatchForSystemThemeChanges();
            DefaultBrowserRegistration.EnsureRegistered();

            var container = new AppContainer();
            var url = e.Args.FirstOrDefault(a => Uri.TryCreate(a, UriKind.Absolute, out _));

            Window window;
            if (!string.IsNullOrEmpty(url))
            {
                window = new LinkChooserWindow(url, container);
            }
            else
            {
                window = new MainWindow(container);
            }

            window.Show();
        }
    }
}
