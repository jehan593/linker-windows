using System.Collections.Generic;
using Linker.Data.Entities;

namespace Linker.Data
{
    public class LinkerData
    {
        public List<BrowserPrefEntity> BrowserPrefs { get; set; } = new List<BrowserPrefEntity>();
        public List<SavedLinkEntity> SavedLinks { get; set; } = new List<SavedLinkEntity>();
        public long NextSavedLinkId { get; set; } = 1;
    }
}
