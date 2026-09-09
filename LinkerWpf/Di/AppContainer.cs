using Linker.Data;
using Linker.Data.Browser;
using Linker.Data.Repositories;
using Linker.Util;

namespace Linker.Di
{
    public class AppContainer
    {
        public InstalledBrowsersRepository InstalledBrowsersRepository { get; }
        public BrowserPrefsRepository BrowserPrefsRepository { get; }
        public SavedLinksRepository SavedLinksRepository { get; }

        public AppContainer()
        {
            var store = new JsonDataStore(JsonDataStore.DefaultDataPath());
            store.EnsureCreated();

            InstalledBrowsersRepository = new InstalledBrowsersRepository(AppIdentity.RegistryAppId);
            BrowserPrefsRepository = new BrowserPrefsRepository(store, InstalledBrowsersRepository);
            SavedLinksRepository = new SavedLinksRepository(store);
        }
    }
}
