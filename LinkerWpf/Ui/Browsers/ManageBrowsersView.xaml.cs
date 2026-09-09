using System.Windows;
using System.Windows.Controls;
using Linker.Data.Repositories;
using Linker.Ui.Browsers;

namespace Linker.Ui.Browsers
{
    public partial class ManageBrowsersView : UserControl
    {
        private ManageBrowsersViewModel ViewModel => (ManageBrowsersViewModel)DataContext;

        public ManageBrowsersView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await ViewModel.LoadAsync();
        }

        private async void OnRefreshClick(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadAsync();
        }

        private async void OnAddBrowserClick(object sender, RoutedEventArgs e)
        {
            var dialog = new EditBrowserDialog();
            var owner = Window.GetWindow(this);
            dialog.Owner = owner;
            var saved = dialog.ShowDialog() == true;
            if (!saved) return;

            await ViewModel.AddCustomBrowserAsync(dialog.NewLabel, dialog.NewExecutablePath, dialog.NewIconPath, dialog.NewExtraArguments);
        }

        private async void OnVisibilityToggled(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var browser = fe?.DataContext as BrowserListItem;
            if (browser == null) return;
            await ViewModel.ToggleHiddenAsync(browser);
        }

        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var browser = fe?.DataContext as BrowserListItem;
            if (browser == null) return;
            var dialog = new EditBrowserDialog(browser);
            var owner = Window.GetWindow(this);
            dialog.Owner = owner;
            var saved = dialog.ShowDialog() == true;
            if (!saved) return;

            if (dialog.WasDeleted)
            {
                await ViewModel.DeleteCustomBrowserAsync(browser);
            }
            else if (dialog.WasReset)
            {
                await ViewModel.ResetOverridesAsync(browser);
            }
            else
            {
                await ViewModel.SaveOverridesAsync(browser, dialog.NewLabel, dialog.NewExecutablePath, dialog.NewIconPath, dialog.NewExtraArguments);
            }
        }

        private async void OnMoveUpClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var browser = fe?.DataContext as BrowserListItem;
            if (browser == null) return;
            await ViewModel.MoveBrowserAsync(browser, -1);
        }

        private async void OnMoveDownClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var browser = fe?.DataContext as BrowserListItem;
            if (browser == null) return;
            await ViewModel.MoveBrowserAsync(browser, +1);
        }
    }
}
