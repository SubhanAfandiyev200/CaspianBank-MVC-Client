using System.Text.RegularExpressions;

namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Tərcümələr səhifəsində {0}, {1} kimi dəyişən hissənin nə olduğunu ingilis mətnin ətrafına baxıb təxmin edir
    // ("{0} AZN" -> məbləğ, "on {1}" -> tarix və s.). Dəqiq siyahı yoxdur, ona görə nəticə "çox güman" izahdır
    public static class TranslationHints
    {
        private static readonly Regex Placeholder = new Regex(@"\{(\d+)\}", RegexOptions.Compiled);

        // Cavab: dəyişənin nömrəsi və izahın ingilis mətni (səhifədə T[...] ilə tərcümə olunur)
        public static List<(int Index, string Meaning)> Describe(string key)
        {
            var result = new List<(int Index, string Meaning)>();
            if (string.IsNullOrEmpty(key))
            {
                return result;
            }

            var seen = new HashSet<int>();
            foreach (Match match in Placeholder.Matches(key))
            {
                var index = int.Parse(match.Groups[1].Value);
                if (!seen.Add(index))
                {
                    continue;
                }
                result.Add((index, Guess(key, match.Index, match.Length)));
            }
            return result.OrderBy(item => item.Index).ToList();
        }

        private static string Guess(string key, int start, int length)
        {
            var before = key.Substring(Math.Max(0, start - 24), Math.Min(start, 24));
            var after = key.Substring(start + length, Math.Min(key.Length - start - length, 24));

            if (Regex.IsMatch(after, @"^\s*AZN\b") || Regex.IsMatch(before, @"AZN\s*$"))
            {
                return "an amount of money (AZN)";
            }
            if (after.StartsWith("%") || Regex.IsMatch(after, @"^\s*a year"))
            {
                return "a percent";
            }
            if (Regex.IsMatch(after, @"^\s*card\b", RegexOptions.IgnoreCase) && !Regex.IsMatch(after, @"^\s*cards\b", RegexOptions.IgnoreCase))
            {
                return "a card type (Standard, Silver, Gold or Cashback)";
            }
            if (Regex.IsMatch(before, @"\b(approved|offered)\s*$", RegexOptions.IgnoreCase))
            {
                return "an amount of money (AZN)";
            }
            if (Regex.IsMatch(before, @"••••\s*$"))
            {
                return "the last 4 digits of a card";
            }
            if (Regex.IsMatch(before, @"\bFIN\s*$"))
            {
                return "a FIN code";
            }
            if (Regex.IsMatch(before, @"\bloan\s*$", RegexOptions.IgnoreCase))
            {
                return "a loan code";
            }
            if (Regex.IsMatch(after, @"^\s*(months?|days?|years?|min\b|h\b|payments?|\(s\)|transactions?|delays?|cards?|open|characters|digit|points?|installments?)", RegexOptions.IgnoreCase))
            {
                return "a number";
            }
            if (Regex.IsMatch(before, @"\b(on|until|by|date)\s*$", RegexOptions.IgnoreCase) && !Regex.IsMatch(after, @"^\s*(your|the)\b"))
            {
                return "a date";
            }
            if (Regex.IsMatch(before, @"\b(of|is|limit|than|up to|at most|least|at)\s*$", RegexOptions.IgnoreCase))
            {
                return "a number";
            }
            if (Regex.IsMatch(before, @"\b(from|to|for|about|with)\s*$", RegexOptions.IgnoreCase) || start == 0)
            {
                return "a name or an item";
            }
            return "a value the page fills in";
        }
    }
}
