using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Linker.Ui.Components;
using Linker.Util;

namespace Linker.Ui.SavedLinks
{
    public partial class SavedLinksView : UserControl
    {
        private SavedLinksViewModel ViewModel => (SavedLinksViewModel)DataContext;

        public SavedLinksView()
        {
            InitializeComponent();
            Loaded += async (s, e) => await ViewModel.LoadAsync();
        }

        private void OnClearSearchClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchText = string.Empty;
        }

        private void OnLinkClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var row = fe?.DataContext as SavedLinkRowEntry;
            if (row == null) return;
            try
            {
                Process.Start(new ProcessStartInfo(row.Link.Url) { UseShellExecute = true });
            }
            catch (Exception)
            {
                ToastService.Show("Couldn't open that link");
            }
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var row = fe?.DataContext as SavedLinkRowEntry;
            if (row == null) return;
            var copied = ClipboardHelper.TryCopy(row.Link.Url);
            ToastService.Show(copied ? "Copied to clipboard" : "Couldn't copy to clipboard");
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var row = fe?.DataContext as SavedLinkRowEntry;
            if (row == null) return;
            var dialog = new EditSavedLinkDialog(row.Link.Url);
            var owner = Window.GetWindow(this);
            dialog.Owner = owner;
            if (dialog.ShowDialog() == true)
            {
                _ = ViewModel.EditAsync(row.Link, dialog.NewUrl);
            }
        }

        private async void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            var row = fe?.DataContext as SavedLinkRowEntry;
            if (row == null) return;
            await ViewModel.DeleteAsync(row.Link);
        }
    }
}
