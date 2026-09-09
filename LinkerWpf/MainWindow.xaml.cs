using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Linker.Data;
using Linker.Di;
using Linker.Theme;
using Linker.Ui.Components;
using Linker.Util;

namespace Linker
{
    public partial class MainWindow : Window
    {
        private readonly FileSystemWatcher _dataWatcher;
        private readonly DispatcherTimer _reloadDebounce;
        public Ui.MainViewModel ViewModel { get; }

        public MainWindow(AppContainer container)
        {
            ViewModel = new Ui.MainViewModel();
            InitializeComponent();
            ImmersiveDarkMode.Apply(this, ThemeManager.IsDarkActive);

            BrowsersView.DataContext = new Ui.Browsers.ManageBrowsersViewModel(container.BrowserPrefsRepository);
            SavedLinksView.DataContext = new Ui.SavedLinks.SavedLinksViewModel(container.SavedLinksRepository);
            DataContext = ViewModel;

            _reloadDebounce = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(350)
            };
            _reloadDebounce.Tick += (s, e) =>
            {
                _reloadDebounce.Stop();
                _ = ReloadAsync();
            };

            _dataWatcher = new FileSystemWatcher(
                Path.GetDirectoryName(JsonDataStore.DefaultDataPath()),
                JsonDataStore.DataFileName)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime
            };
            _dataWatcher.Changed += OnDataFileChanged;
            _dataWatcher.Created += OnDataFileChanged;
            _dataWatcher.EnableRaisingEvents = true;
        }

        private void OnDataFileChanged(object sender, FileSystemEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                _reloadDebounce.Stop();
                _reloadDebounce.Start();
            }));
        }

        private async Task ReloadAsync()
        {
            ViewModel.RefreshDefaultBrowserStatus();
            await ((Ui.Browsers.ManageBrowsersViewModel)BrowsersView.DataContext).LoadAsync();
            await ((Ui.SavedLinks.SavedLinksViewModel)SavedLinksView.DataContext).LoadAsync();
        }

        private async void OnWindowActivated(object sender, EventArgs e)
        {
            await ReloadAsync();
        }

        protected override void OnClosed(EventArgs e)
        {
            _dataWatcher.Dispose();
            base.OnClosed(e);
        }

        private void OnRequestDefaultClicked(object sender, RoutedEventArgs e)
        {
            DefaultBrowserRegistration.EnsureRegistered();
            DefaultBrowserRegistration.OpenDefaultAppsSettings();
        }

        private async void OnBrowsersTabClick(object sender, RoutedEventArgs e)
        {
            BrowsersTab.IsChecked = true;
            SavedLinksTab.IsChecked = false;
            BrowsersView.Visibility = Visibility.Visible;
            SavedLinksView.Visibility = Visibility.Collapsed;
            await ((Ui.Browsers.ManageBrowsersViewModel)BrowsersView.DataContext).LoadAsync();
        }

        private async void OnSavedLinksTabClick(object sender, RoutedEventArgs e)
        {
            BrowsersTab.IsChecked = false;
            SavedLinksTab.IsChecked = true;
            BrowsersView.Visibility = Visibility.Collapsed;
            SavedLinksView.Visibility = Visibility.Visible;
            await ((Ui.SavedLinks.SavedLinksViewModel)SavedLinksView.DataContext).LoadAsync();
        }
    }
}
