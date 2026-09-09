using System.Text;

namespace Linker.Util
{
    public static class CommandLineUtils
    {
        public static string ExtractExecutablePath(string command)
        {
            var trimmed = command.Trim();
            if (trimmed.StartsWith("\""))
            {
                var closingQuote = trimmed.IndexOf('"', 1);
                return closingQuote > 0 ? trimmed.Substring(1, closingQuote - 1) : trimmed.Trim('"');
            }
            var spaceIndex = trimmed.IndexOf(' ');
            return spaceIndex > 0 ? trimmed.Substring(0, spaceIndex) : trimmed;
        }

        public static System.Collections.Generic.List<string> SplitArguments(string argumentString)
        {
            var result = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(argumentString)) return result;

            var current = new StringBuilder();
            var inQuotes = false;
            foreach (var ch in argumentString)
            {
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (char.IsWhiteSpace(ch) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }
                current.Append(ch);
            }
            if (current.Length > 0) result.Add(current.ToString());
            return result;
        }
    }
}
