using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Linker.Data.Repositories;
using Linker.Theme;
using Linker.Ui.Components;
using Linker.Util;
using Microsoft.Win32;

namespace Linker.Ui.Browsers
{
    public partial class EditBrowserDialog : Window
    {
        private readonly BrowserListItem _browser;
        private bool IsAddMode => _browser == null;
        private bool IsCustomBrowser => _browser != null && _browser.IsCustom;

        private string _customIconPath;

        public string NewLabel { get; private set; } = string.Empty;
        public string NewExecutablePath { get; private set; } = string.Empty;
        public string NewIconPath { get; private set; }
        public string NewExtraArguments { get; private set; }
        public bool WasReset { get; private set; }
        public bool WasDeleted { get; private set; }

        public EditBrowserDialog() : this(null) { }

        public EditBrowserDialog(BrowserListItem browser)
        {
            _browser = browser;
            InitializeComponent();
            ImmersiveDarkMode.Apply(this, ThemeManager.IsDarkActive);

            if (IsAddMode)
            {
                Title = TitleText.Text = "Add browser";
                SaveButton.Content = "Add";
                ResetButton.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
                PreviewIconImage.Source = IconExtractor.GetPlaceholder();
            }
            else
            {
                var b = browser;
                LabelTextBox.Text = b.DisplayLabel;
                ExecutablePathTextBox.Text = !string.IsNullOrWhiteSpace(b.CustomExecutablePath)
                    ? b.CustomExecutablePath
                    : b.SystemExecutablePath;
                _customIconPath = b.CustomIconPath;
                ArgumentsTextBox.Text = b.ExtraArguments ?? string.Empty;
                PreviewIconImage.Source = b.Icon;

                if (IsCustomBrowser)
                {
                    ResetButton.Visibility = Visibility.Collapsed;
                    DeleteButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ResetButton.Visibility = Visibility.Visible;
                    DeleteButton.Visibility = Visibility.Collapsed;
                }
            }

            UpdateIconHint();
            LabelTextBox.Focus();
            LabelTextBox.SelectAll();
        }

        private void UpdateIconHint()
        {
            IconPathHintText.Text = string.IsNullOrWhiteSpace(_customIconPath)
                ? string.Empty
                : "Custom: " + Path.GetFileName(_customIconPath);
        }

        private void OnBrowseExecutableClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Choose browser executable",
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() != true) return;

            ExecutablePathTextBox.Text = dialog.FileName;
            if (string.IsNullOrWhiteSpace(_customIconPath))
            {
                var icon = IconExtractor.ExtractIcon(dialog.FileName);
                if (icon != null) PreviewIconImage.Source = icon;
            }
        }

        private void OnBrowseIconClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Choose icon",
                Filter = "Icon or executable files (*.ico;*.exe;*.dll)|*.ico;*.exe;*.dll|All files (*.*)|*.*"
            };
            if (dialog.ShowDialog() != true) return;

            var icon = IconExtractor.ExtractIcon(dialog.FileName);
            if (icon == null)
            {
                ToastService.Show("Couldn't load an icon");
                return;
            }
            _customIconPath = dialog.FileName;
            UpdateIconHint();
            PreviewIconImage.Source = icon;
        }

        private void OnClearIconClick(object sender, RoutedEventArgs e)
        {
            _customIconPath = null;
            UpdateIconHint();
            var fallbackSource = ExecutablePathTextBox.Text ?? string.Empty;
            PreviewIconImage.Source = IconExtractor.ExtractIcon(fallbackSource)
                ?? (_browser != null ? _browser.Icon : null)
                ?? IconExtractor.GetPlaceholder();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var typedPath = (ExecutablePathTextBox.Text ?? string.Empty).Trim();
            var typedLabel = (LabelTextBox.Text ?? string.Empty).Trim();

            if (IsAddMode || IsCustomBrowser)
            {
                if (string.IsNullOrWhiteSpace(typedPath))
                {
                    ToastService.Show("Choose an executable first");
                    return;
                }
                if (string.IsNullOrWhiteSpace(typedLabel))
                {
                    ToastService.Show("Give it a name");
                    return;
                }
                NewExecutablePath = typedPath;
            }
            else
            {
                NewExecutablePath = string.Equals(typedPath, _browser.SystemExecutablePath, StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : typedPath;
            }

            NewLabel = typedLabel;
            NewIconPath = _customIconPath;
            NewExtraArguments = ArgumentsTextBox.Text;
            DialogResult = true;
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            WasReset = true;
            DialogResult = true;
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            WasDeleted = true;
            DialogResult = true;
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
