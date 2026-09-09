using System.Windows;

namespace Linker.Ui.SavedLinks
{
    public partial class EditSavedLinkDialog : Window
    {
        public string NewUrl { get; private set; } = string.Empty;

        public EditSavedLinkDialog(string currentUrl)
        {
            InitializeComponent();
            Theme.ImmersiveDarkMode.Apply(this, Theme.ThemeManager.IsDarkActive);
            UrlTextBox.Text = currentUrl;
            UrlTextBox.Focus();
            UrlTextBox.SelectAll();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            NewUrl = UrlTextBox.Text ?? string.Empty;
            DialogResult = true;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
