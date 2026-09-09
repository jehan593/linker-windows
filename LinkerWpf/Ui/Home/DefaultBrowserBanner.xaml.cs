using System.Windows;
using System.Windows.Controls;

namespace Linker.Ui.Home
{
    public partial class DefaultBrowserBanner : UserControl
    {
        public event RoutedEventHandler RequestDefaultClicked;

        public DefaultBrowserBanner()
        {
            InitializeComponent();
        }

        private void OnRequestDefaultClick(object sender, RoutedEventArgs e)
        {
            RequestDefaultClicked?.Invoke(this, e);
        }
    }
}
