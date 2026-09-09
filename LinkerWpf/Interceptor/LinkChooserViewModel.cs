using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Linker.Data.Repositories;
using Linker.Mvvm;

namespace Linker.Interceptor
{
    public class LinkChooserViewModel : ObservableObject
    {
        private readonly BrowserPrefsRepository _browserPrefsRepository;
        private readonly SavedLinksRepository _savedLinksRepository;

        private string _editableUrl;
        public string EditableUrl
        {
            get => _editableUrl;
            set
            {
                if (SetProperty(ref _editableUrl, value))
                {
                    _ = CheckAlreadySavedAsync(value);
                }
            }
        }

        public ObservableCollection<BrowserListItem> Browsers { get; } = new ObservableCollection<BrowserListItem>();

        private bool _loadingBrowsers = true;
        public bool LoadingBrowsers
        {
            get => _loadingBrowsers;
            set => SetProperty(ref _loadingBrowsers, value);
        }

        private bool _isAlreadySaved;
        public bool IsAlreadySaved
        {
            get => _isAlreadySaved;
            set => SetProperty(ref _isAlreadySaved, value);
        }

        private CancellationTokenSource _savedCheckCts;

        public LinkChooserViewModel(
            string initialUrl,
            BrowserPrefsRepository browserPrefsRepository,
            SavedLinksRepository savedLinksRepository)
        {
            _editableUrl = initialUrl;
            _browserPrefsRepository = browserPrefsRepository;
            _savedLinksRepository = savedLinksRepository;
        }

        public async Task InitializeAsync()
        {
            var loadBrowsers = LoadBrowsersAsync();
            var checkSaved = CheckAlreadySavedAsync(EditableUrl);
            await Task.WhenAll(loadBrowsers, checkSaved);
        }

        private async Task LoadBrowsersAsync()
        {
            var list = await _browserPrefsRepository.GetVisibleBrowsersAsync();
            Browsers.Clear();
            foreach (var b in list) Browsers.Add(b);
            LoadingBrowsers = false;
        }

        public async Task SaveLinkAsync()
        {
            var url = EditableUrl;
            if (string.IsNullOrWhiteSpace(url)) return;
            var isSaved = await _savedLinksRepository.IsSavedAsync(url);
            if (isSaved)
            {
                await _savedLinksRepository.DeleteByUrlAsync(url);
                IsAlreadySaved = false;
            }
            else
            {
                await _savedLinksRepository.SaveAsync(url);
                IsAlreadySaved = true;
            }
        }

        private async Task CheckAlreadySavedAsync(string url)
        {
            _savedCheckCts?.Cancel();
            var cts = new CancellationTokenSource();
            _savedCheckCts = cts;
            try
            {
                var isSaved = await _savedLinksRepository.IsSavedAsync(url);
                if (!cts.IsCancellationRequested)
                {
                    IsAlreadySaved = isSaved;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
