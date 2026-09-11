using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Linker.Data.Repositories;
using Linker.Mvvm;

namespace Linker.Ui.Browsers
{
    public class ManageBrowsersViewModel : ObservableObject
    {
        private readonly BrowserPrefsRepository _browserPrefsRepository;
        private bool _isMoving;

        public ObservableCollection<object> Browsers { get; } = new ObservableCollection<object>();

        public ManageBrowsersViewModel(BrowserPrefsRepository browserPrefsRepository)
        {
            _browserPrefsRepository = browserPrefsRepository;
        }

        public async Task LoadAsync(bool freshBrowsers = true)
        {
            var list = await _browserPrefsRepository.GetManageListAsync(freshBrowsers);
            Browsers.Clear();
            foreach (var item in list.Where(b => !b.Hidden))
            {
                Browsers.Add(item);
            }

            var hidden = list.Where(b => b.Hidden).ToList();
            if (hidden.Count > 0)
            {
                Browsers.Add(HiddenSectionHeader.Instance);
                foreach (var item in hidden)
                {
                    Browsers.Add(item);
                }
            }
        }

        public async Task ToggleHiddenAsync(BrowserListItem browser)
        {
            await _browserPrefsRepository.SetHiddenAsync(browser.Id, !browser.Hidden);
            await LoadAsync();
        }

        public async Task MoveBrowserAsync(BrowserListItem browser, int delta)
        {
            if (_isMoving) return;
            _isMoving = true;
            try
            {
                var visible = Browsers.OfType<BrowserListItem>().Where(b => !b.Hidden).ToList();
                var index = visible.FindIndex(b => b.Id == browser.Id);
                var targetIndex = index + delta;
                if (index < 0 || targetIndex < 0 || targetIndex >= visible.Count) return;

                visible.RemoveAt(index);
                visible.Insert(targetIndex, browser);

                var hidden = Browsers.OfType<BrowserListItem>().Where(b => b.Hidden).ToList();
                var orderedIds = visible.Concat(hidden).Select(b => b.Id).ToList();

                await _browserPrefsRepository.ApplyOrderAsync(orderedIds);
                await LoadAsync(freshBrowsers: false);
            }
            finally
            {
                _isMoving = false;
            }
        }

        public async Task SaveOverridesAsync(
            BrowserListItem browser, string label, string executablePath, string iconPath, string extraArguments)
        {
            await _browserPrefsRepository.SetOverridesAsync(browser.Id, label, executablePath, iconPath, extraArguments);
            await LoadAsync();
        }

        public async Task ResetOverridesAsync(BrowserListItem browser)
        {
            await _browserPrefsRepository.SetOverridesAsync(browser.Id, null, null, null, null);
            await LoadAsync();
        }

        public async Task AddCustomBrowserAsync(string label, string executablePath, string iconPath, string extraArguments)
        {
            await _browserPrefsRepository.AddCustomBrowserAsync(label, executablePath, iconPath, extraArguments);
            await LoadAsync();
        }

        public async Task DeleteCustomBrowserAsync(BrowserListItem browser)
        {
            await _browserPrefsRepository.DeleteCustomBrowserAsync(browser.Id);
            await LoadAsync();
        }
    }
}