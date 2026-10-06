namespace CaspianBank_MVC_FinalProject.Helpers
{
    public static class ApiUrl
    {
        // Bazada yalnız yol var (/images/cards/x.png). Fayllar API-dən verildiyi üçün tam ünvan lazımdır.
        public static string ToAbsolute(string? baseUrl, string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return string.Empty;
            if (path.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return path;
            return (baseUrl ?? string.Empty).TrimEnd('/') + "/" + path.TrimStart('/');
        }
    }
}
