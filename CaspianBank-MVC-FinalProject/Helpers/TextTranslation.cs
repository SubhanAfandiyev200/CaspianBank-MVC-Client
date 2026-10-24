using System.Text;
using System.Text.RegularExpressions;
using CaspianBank_MVC_FinalProject.Services;

namespace CaspianBank_MVC_FinalProject.Helpers
{
    // API-dən hazır ingilis cümlə kimi gələn mətnlər (əməliyyat təsviri, istifadəçi adı) üçün tərcümə köməkçiləri.
    // Əməliyyat təsviri bazada ingilis yazılır ("Transfer to your Gold card •••• 1387"): dəyişən hissələri ayırıb
    // şablonu ("Transfer to your {0} card •••• {1}") lüğətdən tərcümə edirik. Lüğətdə olmayan mətn olduğu kimi qalır.
    public static class TextTranslation
    {
        private sealed class Rule
        {
            public Regex Pattern { get; }
            public string Template { get; }
            public bool[] TranslateArg { get; }

            public Rule(string pattern, string template, params bool[] translateArg)
            {
                Pattern = new Regex(pattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);
                Template = template;
                TranslateArg = translateArg;
            }
        }

        // Sıra vacibdir: daha dəqiq qaydalar əvvəldə. Arqumentin true olması həmin hissənin (məs. "Gold") ayrıca tərcümə olunması deməkdir
        private static readonly Rule[] Rules =
        {
            new Rule(@"^Transfer to your (\w+) card •••• (\d{4})$", "Transfer to your {0} card •••• {1}", true, false),
            new Rule(@"^Transfer from your (\w+) card •••• (\d{4})$", "Transfer from your {0} card •••• {1}", true, false),
            new Rule(@"^Transfer to •••• (\d{4})$", "Transfer to •••• {0}", false),
            new Rule(@"^Transfer to (.+) •••• (\d{4})$", "Transfer to {0} •••• {1}", false, false),
            new Rule(@"^Transfer from (.+)$", "Transfer from {0}", false),
            new Rule(@"^(\w+) card opening fee$", "{0} card opening fee", true),
            new Rule(@"^Cashback · (.+)$", "Cashback · {0}", false),
            new Rule(@"^Term deposit · (\d+) months$", "Term deposit · {0} months", false),
            new Rule(@"^Deposit returned · (\d+) months$", "Deposit returned · {0} months", false),
            new Rule(@"^Deposit interest · (.+)% a year$", "Deposit interest · {0}% a year", false),
            new Rule(@"^Loan (\S+) paid out$", "Loan {0} paid out", false),
            new Rule(@"^Early payment on loan (\S+)$", "Early payment on loan {0}", false),
            new Rule(@"^Loan (\S+) repaid in full$", "Loan {0} repaid in full", false),
            new Rule(@"^Payment on loan (\S+)$", "Payment on loan {0}", false),
        };

        public static string Describe(this ITranslator t, string? description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return string.Empty;
            }

            foreach (var rule in Rules)
            {
                var match = rule.Pattern.Match(description);
                if (!match.Success)
                {
                    continue;
                }

                var args = new object[match.Groups.Count - 1];
                for (var i = 1; i < match.Groups.Count; i++)
                {
                    var value = match.Groups[i].Value;
                    args[i - 1] = rule.TranslateArg[i - 1] ? t[value] : value;
                }
                return t[rule.Template, args];
            }

            return t[description];
        }

        // Kart səviyyəsi (Standard, Gold...) və s. tək söz: lüğətdə yoxdursa özü qalır
        public static string Word(this ITranslator t, string? text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : t[text];
        }

        // Kart etiketi "Gold •••• 7800": yalnız başdakı səviyyə adı tərcümə olunur
        public static string CardLabel(this ITranslator t, string? label)
        {
            if (string.IsNullOrEmpty(label))
            {
                return string.Empty;
            }
            var space = label.IndexOf(' ');
            return space <= 0 ? t[label] : t[label.Substring(0, space)] + label.Substring(space);
        }

        // Salamlama: günün vaxtına görə
        public static string Greeting(this ITranslator t, int hour)
        {
            return t[hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening"];
        }

        // Ad-soyad rus dilində kirillcə yazılır (Aysel Məmmədova -> Айсел Мемедова). Az və En dillərində dəyişmir
        public static string Person(this ITranslator t, string? name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            return t.Lang == "ru" ? ToCyrillic(name) : name;
        }

        private static readonly Dictionary<char, string> Letters = new Dictionary<char, string>
        {
            ['a'] = "а", ['b'] = "б", ['c'] = "дж", ['ç'] = "ч", ['d'] = "д", ['e'] = "е", ['ə'] = "е", ['f'] = "ф", ['g'] = "г",
            ['ğ'] = "г", ['h'] = "х", ['x'] = "х", ['ı'] = "ы", ['i'] = "и", ['j'] = "ж", ['k'] = "к", ['q'] = "г", ['l'] = "л",
            ['m'] = "м", ['n'] = "н", ['o'] = "о", ['ö'] = "ё", ['p'] = "п", ['r'] = "р", ['s'] = "с", ['ş'] = "ш", ['t'] = "т",
            ['u'] = "у", ['ü'] = "ю", ['v'] = "в", ['y'] = "й", ['z'] = "з", ['w'] = "в"
        };

        private static string ToCyrillic(string text)
        {
            var sb = new StringBuilder(text.Length + 4);

            foreach (var ch in text)
            {
                var lower = ch == 'İ' ? 'i' : char.ToLowerInvariant(ch);
                if (!Letters.TryGetValue(lower, out var cyr))
                {
                    sb.Append(ch);
                    continue;
                }

                // Böyük hərf: söz əvvəli və ya orijinalda böyük olan
                if (char.IsUpper(ch))
                {
                    cyr = char.ToUpperInvariant(cyr[0]) + cyr.Substring(1);
                }
                sb.Append(cyr);
            }
            return sb.ToString();
        }
    }
}
