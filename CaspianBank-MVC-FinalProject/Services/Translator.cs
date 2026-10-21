using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Json;

namespace CaspianBank_MVC_FinalProject.Services
{
    // Saytın dilləri: tərcümələr verilənlər bazasındadır (API: api/i18n/{dil}). Açar ingilis mətnin özüdür ("Transfer money"):
    // tərcümə yoxdursa və ya API əlçatmazdırsa səhifədə ingilis mətn qalır, səhifə heç vaxt sınmır

    public interface ITranslationStore
    {
        // az və ru üçün tərcümə lüğəti (ingilis üçün boş). Beş dəqiqə yaddaşda saxlanılır
        Task<IReadOnlyDictionary<string, string>> GetAsync(string language);

        // Admin tərcüməni dəyişəndən sonra çağırılır ki, yeni mətn dərhal görünsün
        void Invalidate();
    }

    public class TranslationStore : ITranslationStore
    {
        private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TranslationStore> _logger;
        private static CancellationTokenSource _reset = new CancellationTokenSource();

        public TranslationStore(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<TranslationStore> logger)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetAsync(string language)
        {
            if (language != "az" && language != "ru")
            {
                return Empty;
            }

            var key = "i18n:" + language;
            if (_cache.TryGetValue(key, out IReadOnlyDictionary<string, string>? cached) && cached is not null)
            {
                return cached;
            }

            try
            {
                var client = _httpClientFactory.CreateClient("CaspianApi");
                var data = await client.GetFromJsonAsync<Dictionary<string, string>>("api/i18n/" + language);
                // Verilənlər bazasında açar böyük-kiçik hərfə həssas deyil ("Get Started" = "Get started"), lüğət də eyni olmalıdır
                var result = (IReadOnlyDictionary<string, string>)new Dictionary<string, string>(data ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
                _cache.Set(key, result, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }.AddExpirationToken(new Microsoft.Extensions.Primitives.CancellationChangeToken(_reset.Token)));
                return result;
            }
            catch (Exception ex)
            {
                // API işləmirsə ingilis mətn göstərilir; 30 saniyə sonra yenidən cəhd olunur (hər sorğuda gözləməyək)
                _logger.LogWarning(ex, "Tərcümələr alınmadı: {Language}", language);
                _cache.Set(key, Empty, TimeSpan.FromSeconds(30));
                return Empty;
            }
        }

        public void Invalidate()
        {
            var old = Interlocked.Exchange(ref _reset, new CancellationTokenSource());
            old.Cancel();
            old.Dispose();
        }
    }

    // Səhifədə @T["Transfer money"] kimi işlənir. Açar tapılmasa ingilis mətnin özü qaytarılır
    public interface ITranslator
    {
        string Lang { get; }
        string this[string text] { get; }
        string this[string text, params object[] args] { get; }

        // JavaScript üçün: "js:" ilə başlayan açarlar (klientdə t("...") işlədir)
        IReadOnlyDictionary<string, string> ForScripts();
    }

    public class Translator : ITranslator
    {
        public const string LangItem = "cb-lang";
        public const string DictionaryItem = "cb-dictionary";

        private readonly IHttpContextAccessor _accessor;

        public Translator(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public string Lang
        {
            get
            {
                return _accessor.HttpContext?.Items[LangItem] as string ?? "en";
            }
        }

        private IReadOnlyDictionary<string, string>? Dictionary
        {
            get
            {
                return _accessor.HttpContext?.Items[DictionaryItem] as IReadOnlyDictionary<string, string>;
            }
        }

        public string this[string text]
        {
            get
            {
                var dictionary = Dictionary;
                if (dictionary is not null && !string.IsNullOrEmpty(text))
                {
                    // Verilənlər bazasından gələn mətnlərin sonunda boşluq və ya yeni sətir ola bilər: ona görə kəsilmiş halı da yoxlanılır
                    if ((dictionary.TryGetValue(text, out var translated) || dictionary.TryGetValue(text.Trim(), out translated)) && !string.IsNullOrEmpty(translated))
                    {
                        return translated;
                    }
                }
                return text;
            }
        }

        public string this[string text, params object[] args]
        {
            get
            {
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, this[text], args);
            }
        }

        public IReadOnlyDictionary<string, string> ForScripts()
        {
            var dictionary = Dictionary;
            if (dictionary is null)
            {
                return new Dictionary<string, string>();
            }
            return dictionary.Where(p => p.Key.StartsWith("js:", StringComparison.Ordinal)).ToDictionary(p => p.Key, p => p.Value);
        }
    }

    // Hər sorğuda dili seçir: əvvəl cookie ("cb-lang"), yoxdursa brauzerin dili (az/ru), yoxsa ingilis. Lüğəti yükləyib sorğuya qoyur
    public class LanguageMiddleware
    {
        public const string CookieName = "cb-lang";
        public static readonly string[] Supported = { "en", "az", "ru" };

        private readonly RequestDelegate _next;

        public LanguageMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITranslationStore store)
        {
            var language = Choose(context);
            context.Items[Translator.LangItem] = language;
            context.Items[Translator.DictionaryItem] = await store.GetAsync(language);
            await _next(context);
        }

        private static string Choose(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(CookieName, out var saved) && Supported.Contains(saved))
            {
                return saved;
            }

            var header = context.Request.Headers.AcceptLanguage.ToString().ToLowerInvariant();
            foreach (var part in header.Split(','))
            {
                var code = part.Trim().Split(';')[0].Split('-')[0];
                if (code == "az" || code == "ru")
                {
                    return code;
                }
                if (code == "en")
                {
                    return "en";
                }
            }
            return "en";
        }
    }
}
