using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Linker.Util;
using Microsoft.Win32;

namespace Linker.Data.Browser
{
    public class InstalledBrowsersRepository
    {
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
        private static readonly string[] RegistryRoots = new[]
        {
            @"SOFTWARE\Clients\StartMenuInternet",
            @"SOFTWARE\WOW6432Node\Clients\StartMenuInternet",
        };

        private readonly SemaphoreSlim _mutex = new SemaphoreSlim(1, 1);
        private readonly string _selfId;
        private List<BrowserInfo> _cache;
        private DateTime _cachedAt;

        public InstalledBrowsersRepository(string selfId)
        {
            _selfId = selfId;
        }

        public async Task<List<BrowserInfo>> GetBrowsersAsync(bool forceRefresh = false)
        {
            await _mutex.WaitAsync();
            try
            {
                if (!forceRefresh && _cache != null && DateTime.UtcNow - _cachedAt < CacheTtl)
                {
                    return _cache;
                }
                var fresh = await Task.Run(QueryBrowsers);
                _cache = fresh;
                _cachedAt = DateTime.UtcNow;
                return fresh;
            }
            finally
            {
                _mutex.Release();
            }
        }

        private List<BrowserInfo> QueryBrowsers()
        {
            var seen = new Dictionary<string, BrowserInfo>(StringComparer.OrdinalIgnoreCase);

            void ScanHive(RegistryKey hive)
            {
                foreach (var rootPath in RegistryRoots)
                {
                    using (var root = hive.OpenSubKey(rootPath))
                    {
                        if (root == null) continue;
                        foreach (var subKeyName in root.GetSubKeyNames())
                        {
                            if (seen.ContainsKey(subKeyName) || string.Equals(subKeyName, _selfId, StringComparison.OrdinalIgnoreCase))
                                continue;
                            using (var browserKey = root.OpenSubKey(subKeyName))
                            {
                                var info = ReadBrowserInfo(subKeyName, browserKey);
                                if (info != null) seen[subKeyName] = info;
                            }
                        }
                    }
                }
            }

            ScanHive(Registry.LocalMachine);
            ScanHive(Registry.CurrentUser);

            var result = new List<BrowserInfo>(seen.Values);
            result.Sort((a, b) => string.Compare(a.SystemLabel, b.SystemLabel, StringComparison.OrdinalIgnoreCase));
            return result;
        }

        private BrowserInfo ReadBrowserInfo(string id, RegistryKey browserKey)
        {
            if (browserKey == null) return null;
            var label = browserKey.GetValue(null) as string;
            using (var commandKey = browserKey.OpenSubKey(@"shell\open\command"))
            {
                var command = commandKey?.GetValue(null) as string;
                if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(command)) return null;

                var exePath = CommandLineUtils.ExtractExecutablePath(command);
                var iconPath = ReadIconPath(browserKey) ?? exePath;
                var icon = IconExtractor.ExtractIcon(iconPath) ?? IconExtractor.ExtractIcon(exePath);
                if (icon == null) return null;

                return new BrowserInfo(id, label, command, icon);
            }
        }

        private static string ReadIconPath(RegistryKey browserKey)
        {
            using (var iconKey = browserKey.OpenSubKey("DefaultIcon"))
            {
                var raw = iconKey?.GetValue(null) as string;
                if (string.IsNullOrWhiteSpace(raw)) return null;
                var commaIndex = raw.LastIndexOf(',');
                return commaIndex > 1 ? raw.Substring(0, commaIndex) : raw;
            }
        }
    }
}
