using System;
using System.IO;

namespace Linker.Util
{
    public static class PrivateWindowSupport
    {
        private sealed class Rule
        {
            public readonly string[] Terms;
            public readonly string Argument;

            public Rule(string argument, params string[] terms)
            {
                Argument = argument;
                Terms = terms;
            }
        }

        private static readonly Rule[] Rules =
        {
            new Rule("-private-window", "firefox", "mullvad", "tor"),
            new Rule("--inprivate", "msedge", "edge"),
            new Rule("-private", "iexplore", "internet explorer"),
            new Rule("--private", "opera"),
            new Rule("--incognito", "brave", "chrome", "chromium", "vivaldi")
        };

        public static string GetArgument(string executablePath, string systemLabel)
        {
            var name = Path.GetFileNameWithoutExtension((executablePath ?? string.Empty).Trim('"'));
            if (string.IsNullOrWhiteSpace(name)) name = systemLabel ?? string.Empty;

            foreach (var rule in Rules)
            {
                if (MatchesAny(name, rule.Terms) || MatchesAny(systemLabel, rule.Terms))
                    return rule.Argument;
            }
            return null;
        }

        private static bool MatchesAny(string value, string[] terms)
        {
            if (string.IsNullOrEmpty(value)) return false;
            foreach (var term in terms)
            {
                if (value.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }
    }
}
