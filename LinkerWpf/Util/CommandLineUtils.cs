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

        public static string ExtractArguments(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return string.Empty;
            var trimmed = command.Trim();
            var index = 0;

            if (trimmed[0] == '"')
            {
                var closingQuote = trimmed.IndexOf('"', 1);
                if (closingQuote < 0) return string.Empty;
                index = closingQuote + 1;
            }
            else
            {
                while (index < trimmed.Length && !char.IsWhiteSpace(trimmed[index])) index++;
            }

            return index < trimmed.Length ? trimmed.Substring(index).Trim() : string.Empty;
        }

        public static string QuoteArgument(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "%22") + "\"";
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
