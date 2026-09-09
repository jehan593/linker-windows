using Linker.Mvvm;
using Linker.Util;

namespace Linker.Ui
{
    public class MainViewModel : ObservableObject
    {
        private bool _isDefaultBrowser = DefaultBrowserRegistration.IsDefault();
        public bool IsDefaultBrowser
        {
            get => _isDefaultBrowser;
            set => SetProperty(ref _isDefaultBrowser, value);
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public void RefreshDefaultBrowserStatus()
        {
            IsDefaultBrowser = DefaultBrowserRegistration.IsDefault();
        }
    }
}
