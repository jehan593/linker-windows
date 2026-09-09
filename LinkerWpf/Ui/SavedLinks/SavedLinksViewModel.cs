using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Linker.Data.Entities;
using Linker.Data.Repositories;
using Linker.Util;

namespace Linker.Ui.SavedLinks
{
    public class SavedLinksViewModel : Mvvm.ObservableObject
    {
        private readonly SavedLinksRepository _savedLinksRepository;
        private List<SavedLinkEntity> _all = new List<SavedLinkEntity>();

        public ObservableCollection<SavedLinksListEntry> Items { get; } = new ObservableCollection<SavedLinksListEntry>();

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        public SavedLinksViewModel(SavedLinksRepository savedLinksRepository)
        {
            _savedLinksRepository = savedLinksRepository;
        }

        public async Task LoadAsync()
        {
            _all = await _savedLinksRepository.GetAllAsync();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var query = SearchText;
            var filtered = string.IsNullOrEmpty(query)
                ? _all
                : _all.Where(l => l.Url.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            Items.Clear();
            DateTime? currentDay = null;
            foreach (var link in filtered)
            {
                var day = TimeUtils.DayKey(link.SavedAtMillis);
                if (currentDay == null || currentDay.Value != day)
                {
                    Items.Add(new DayHeaderEntry(TimeUtils.DayLabel(day)));
                    currentDay = day;
                }
                Items.Add(new SavedLinkRowEntry(link, TimeUtils.FormatSavedTime(link.SavedAtMillis), query));
            }
        }

        public async Task EditAsync(SavedLinkEntity link, string newUrl)
        {
            await _savedLinksRepository.UpdateUrlAsync(link.Id, newUrl);
            await LoadAsync();
        }

        public async Task DeleteAsync(SavedLinkEntity link)
        {
            await _savedLinksRepository.DeleteAsync(link.Id);
            await LoadAsync();
        }
    }
}
