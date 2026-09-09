using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Linker.Data.Browser;
using Linker.Data.Entities;
using Linker.Util;

namespace Linker.Data.Repositories
{
    public class BrowserPrefsRepository
    {
        private readonly JsonDataStore _store;
        private readonly InstalledBrowsersRepository _installedBrowsersRepository;

        public BrowserPrefsRepository(JsonDataStore store, InstalledBrowsersRepository installedBrowsersRepository)
        {
            _store = store;
            _installedBrowsersRepository = installedBrowsersRepository;
        }

        public async Task<List<BrowserListItem>> GetManageListAsync()
        {
            var prefs = _store.Read(d => d.BrowserPrefs);
            var browsers = await _installedBrowsersRepository.GetBrowsersAsync(forceRefresh: true);
            return Merge(browsers, prefs);
        }

        public async Task<List<BrowserListItem>> GetVisibleBrowsersAsync()
        {
            var prefs = _store.Read(d => d.BrowserPrefs);
            var browsers = await _installedBrowsersRepository.GetBrowsersAsync(forceRefresh: false);
            return Merge(browsers, prefs).Where(b => !b.Hidden).ToList();
        }

        private static List<BrowserListItem> Merge(List<BrowserInfo> browsers, List<BrowserPrefEntity> prefs)
        {
            var registryPrefs = prefs.Where(p => !p.IsCustom).ToDictionary(p => p.Id);
            var nextOrder = (prefs.Count > 0 ? prefs.Max(p => p.OrderIndex) : -1) + 1;
            var items = new List<BrowserListItem>();
            foreach (var info in browsers)
            {
                BrowserPrefEntity pref;
                registryPrefs.TryGetValue(info.Id, out pref);
                var orderIndex = pref != null ? pref.OrderIndex : nextOrder++;
                var displayLabel = pref != null && !string.IsNullOrWhiteSpace(pref.CustomLabel) ? pref.CustomLabel : info.SystemLabel;
                var systemExecutablePath = CommandLineUtils.ExtractExecutablePath(info.ExecutableCommand);

                var icon = info.Icon;
                if (pref != null && !string.IsNullOrWhiteSpace(pref.CustomIconPath))
                {
                    icon = IconExtractor.ExtractIcon(pref.CustomIconPath) ?? icon;
                }
                else if (pref != null && !string.IsNullOrWhiteSpace(pref.CustomExecutablePath))
                {
                    icon = IconExtractor.ExtractIcon(pref.CustomExecutablePath) ?? icon;
                }

                items.Add(new BrowserListItem(
                    info.Id, info.SystemLabel, displayLabel, systemExecutablePath,
                    pref != null ? pref.CustomExecutablePath : null,
                    pref != null ? pref.CustomIconPath : null,
                    pref != null ? pref.ExtraArguments : null,
                    icon, pref != null && pref.Hidden, orderIndex, isCustom: false));
            }

            foreach (var pref in prefs.Where(p => p.IsCustom))
            {
                var label = pref.CustomLabel ?? "Custom browser";
                var icon = IconExtractor.ExtractIcon(pref.CustomIconPath)
                    ?? IconExtractor.ExtractIcon(pref.CustomExecutablePath)
                    ?? IconExtractor.GetPlaceholder();
                items.Add(new BrowserListItem(
                    pref.Id, label, label, pref.CustomExecutablePath ?? string.Empty,
                    null, pref.CustomIconPath, pref.ExtraArguments,
                    icon, pref.Hidden, pref.OrderIndex, isCustom: true));
            }

            items.Sort((a, b) => a.OrderIndex.CompareTo(b.OrderIndex));
            return items;
        }

        public Task SetHiddenAsync(string id, bool hidden)
        {
            _store.Mutate(data => { FindOrCreatePref(data, id).Hidden = hidden; });
            return Task.CompletedTask;
        }

        public Task SetOverridesAsync(
            string id,
            string customLabel, string customExecutablePath, string customIconPath, string extraArguments)
        {
            _store.Mutate(data =>
            {
                var pref = FindOrCreatePref(data, id);
                pref.CustomLabel = EmptyToNull(customLabel);
                pref.CustomExecutablePath = EmptyToNull(customExecutablePath);
                pref.CustomIconPath = EmptyToNull(customIconPath);
                pref.ExtraArguments = EmptyToNull(extraArguments);
            });
            return Task.CompletedTask;
        }

        private static string EmptyToNull(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

        public Task AddCustomBrowserAsync(
            string label, string executablePath, string iconPath, string extraArguments)
        {
            _store.Mutate(data =>
            {
                var visibleMax = data.BrowserPrefs.Where(p => !p.Hidden).Select(p => p.OrderIndex).DefaultIfEmpty(-1).Max();
                data.BrowserPrefs.Add(new BrowserPrefEntity
                {
                    Id = "custom:" + Guid.NewGuid(),
                    CustomLabel = label,
                    Hidden = false,
                    OrderIndex = visibleMax + 1,
                    CustomExecutablePath = executablePath,
                    CustomIconPath = EmptyToNull(iconPath),
                    ExtraArguments = EmptyToNull(extraArguments),
                    IsCustom = true,
                });
            });
            return Task.CompletedTask;
        }

        public Task DeleteCustomBrowserAsync(string id)
        {
            _store.Mutate(data => data.BrowserPrefs.RemoveAll(p => p.Id == id && p.IsCustom));
            return Task.CompletedTask;
        }

        public Task ApplyOrderSwapAsync(string idA, int orderA, string idB, int orderB)
        {
            _store.Mutate(data =>
            {
                var a = FindOrCreatePref(data, idA);
                var b = FindOrCreatePref(data, idB);
                a.OrderIndex = orderA;
                b.OrderIndex = orderB;
            });
            return Task.CompletedTask;
        }

        private static BrowserPrefEntity FindOrCreatePref(LinkerData data, string id)
        {
            var pref = data.BrowserPrefs.FirstOrDefault(p => p.Id == id);
            if (pref == null)
            {
                pref = new BrowserPrefEntity
                {
                    Id = id,
                    Hidden = false,
                    OrderIndex = data.BrowserPrefs.Count > 0 ? data.BrowserPrefs.Max(p => p.OrderIndex) + 1 : 0,
                    IsCustom = false,
                };
                data.BrowserPrefs.Add(pref);
            }
            return pref;
        }
    }
}
