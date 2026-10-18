using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Linker.Data.Repositories;
using Linker.Di;
using Linker.Theme;
using Linker.Ui.Components;
using Linker.Util;

namespace Linker.Interceptor
{
        public partial class LinkChooserWindow : Window
        {
            private readonly AppContainer _container;
            public LinkChooserViewModel ViewModel { get; }

            public LinkChooserWindow(string url, AppContainer container)
            {
                _container = container;
                ViewModel = new LinkChooserViewModel(url, container.BrowserPrefsRepository, container.SavedLinksRepository);
                InitializeComponent();
                DataContext = ViewModel;
                ImmersiveDarkMode.Apply(this, ThemeManager.IsDarkActive);
            }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnWindowLoaded;

            // The window starts minimized so Windows never shows its raw
            // white surface. SizeToContent means the size is only known now,
            // so center it manually first, then restore once the browser list
            // is ready so the popup appears complete.
            var wa = SystemParameters.WorkArea;
            var targetWidth = Width;
            Left = wa.Left + (wa.Width - targetWidth) / 2;
            Top = wa.Top + (wa.Height - ActualHeight) / 2;

            await ViewModel.InitializeAsync();

            // No minimize/restore swoosh; pop the completed window in.
            WindowTransitions.DisableAnimations(this);
            WindowState = WindowState.Normal;
            if (Width != targetWidth)
            {
                // Restore can snap the width back to MinWidth, so re-assert it.
                Width = double.NaN;
                Width = targetWidth;
                Left = wa.Left + (wa.Width - ActualWidth) / 2;
            }

            // Start focused on the top browser so arrow keys pick one and
            // Enter opens it. Deferred to Loaded priority so the item
            // containers exist by then; focusing the ListBox itself would
            // swallow the first arrow keypress.
            _ = Dispatcher.BeginInvoke(new Action(() =>
            {
                if (ViewModel.Browsers.Count == 0) return;
                BrowserList.SelectedIndex = 0;
                if (BrowserList.ItemContainerGenerator.ContainerFromIndex(0) is ListBoxItem first)
                {
                    first.Focus();
                }
                else
                {
                    BrowserList.Focus();
                }
            }), DispatcherPriority.Loaded);
        }

        private void OnBrowserListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var browser = BrowserList.SelectedItem as BrowserListItem;
            if (browser == null) return;
            e.Handled = true;
            OpenInBrowser(browser);
        }

        private void OnBrowserListGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Tab lands on the ListBox itself; move focus straight to the
            // highlighted row so the first arrow key moves the selection.
            if (!ReferenceEquals(e.NewFocus, BrowserList)) return;
            var index = BrowserList.SelectedIndex >= 0 ? BrowserList.SelectedIndex : 0;
            if (BrowserList.ItemContainerGenerator.ContainerFromIndex(index) is ListBoxItem item)
            {
                item.Focus();
            }
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            var copied = ClipboardHelper.TryCopy(ViewModel.EditableUrl);
            ToastService.Show(copied ? "Copied to clipboard" : "Couldn't copy to clipboard");
        }

        private async void OnSaveClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.SaveLinkAsync();
            ToastService.Show(ViewModel.IsAlreadySaved ? "Link saved" : "Link removed");
        }

        private void OnBrowserRowClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            var browser = frameworkElement?.DataContext as BrowserListItem;
            if (browser == null) return;
            OpenInBrowser(browser);
        }

        private void OpenInBrowser(BrowserListItem browser)
        {
            if (!BrowserLauncher.TryOpen(browser, ViewModel.EditableUrl))
            {
                ToastService.Show("Couldn't open that link");
                return;
            }
            Close();
        }

        private void OnManageBrowsersClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow == null)
            {
                mainWindow = new MainWindow(_container);
                mainWindow.Show();
            }
            else
            {
                mainWindow.Activate();
            }
            Close();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
