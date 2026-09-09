using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Linker.Data.Entities;

namespace Linker.Data.Repositories
{
    public class SavedLinksRepository
    {
        private readonly JsonDataStore _store;

        public SavedLinksRepository(JsonDataStore store)
        {
            _store = store;
        }

        public Task<List<SavedLinkEntity>> GetAllAsync()
        {
            var result = _store.Read(d => d.SavedLinks.OrderByDescending(l => l.SavedAtMillis).ToList());
            return Task.FromResult(result);
        }

        public Task<bool> IsSavedAsync(string url)
        {
            var trimmed = url.Trim();
            var result = _store.Read(d => d.SavedLinks.Any(l => l.Url == trimmed));
            return Task.FromResult(result);
        }

        public Task SaveAsync(string url, long? savedAtMillis = null)
        {
            var trimmed = url.Trim();
            var at = savedAtMillis ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _store.Mutate(data =>
            {
                var existing = data.SavedLinks.FirstOrDefault(l => l.Url == trimmed);
                if (existing != null)
                {
                    existing.SavedAtMillis = at;
                }
                else
                {
                    data.SavedLinks.Add(new SavedLinkEntity { Id = data.NextSavedLinkId++, Url = trimmed, SavedAtMillis = at });
                }
            });
            return Task.CompletedTask;
        }

        public Task UpdateUrlAsync(long id, string newUrl)
        {
            _store.Mutate(data =>
            {
                var link = data.SavedLinks.FirstOrDefault(l => l.Id == id);
                if (link != null)
                {
                    link.Url = newUrl.Trim();
                }
            });
            return Task.CompletedTask;
        }

        public Task DeleteAsync(long id)
        {
            _store.Mutate(data => data.SavedLinks.RemoveAll(l => l.Id == id));
            return Task.CompletedTask;
        }

        public Task DeleteByUrlAsync(string url)
        {
            var trimmed = url.Trim();
            _store.Mutate(data => data.SavedLinks.RemoveAll(l => l.Url == trimmed));
            return Task.CompletedTask;
        }
    }
}
