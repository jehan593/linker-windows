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

                // The chooser is a popup, so it can't use the minimize trick.
                // Present it fully transparent until the first frame is
                // rendered, so the raw white surface Windows draws before
                // that frame is never visible. CenterScreen still applies,
                // and ContentRendered reveals the dark frame once it's ready.
                window.Opacity = 0;
            }
            else
            {
                window = new MainWindow(container);

                // Windows shows a raw white surface until the window's first
                // fully-rendered frame is presented (known WPF/Windows bug),
                // so the window visibly flashes white at startup. Start
                // minimized so no surface is ever exposed, and restore in
                // OnWindowLoaded once the dark frame is ready. A minimized
                // window never applies CenterScreen, so center it explicitly.
                var wa = SystemParameters.WorkArea;
                window.Left = wa.Left + (wa.Width - window.Width) / 2;
                window.Top = wa.Top + (wa.Height - window.Height) / 2;
                window.WindowState = WindowState.Minimized;
            }

            window.Show();

            if (window is LinkChooserWindow)
            {
                window.ContentRendered += (s, e2) => window.Opacity = 1;
            }
        }
    }
}
