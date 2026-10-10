namespace CaspianBank_MVC_FinalProject.ViewModels.Cards
{
    // API-nin cavabı: kod emailə göndərildi (api/cards/{id}/block/code və unblock/code)
    public class CardCodeSentUIVM
    {
        public string MaskedEmail { get; set; } = string.Empty;
        public string? DevCode { get; set; }
    }

    // Kartı bloklamaq / blokdan çıxarmaq pəncərəsi (iki addım: kodu göndər, kodu yaz). Views/Shared/_CardCodeModal.cshtml
    public class CardCodeModalVM
    {
        public string Id { get; set; } = string.Empty;           // pəncərənin html id-si (düymə data-open-modal ilə açır)
        public int CardId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Lede { get; set; } = string.Empty;
        public string SendAction { get; set; } = string.Empty;   // kodu emailə göndərən action
        public string ConfirmAction { get; set; } = string.Empty; // kodu yoxlayan action
        public string ConfirmLabel { get; set; } = string.Empty;
        public string ConfirmClass { get; set; } = "btn-danger";
        public string? SentTo { get; set; }                      // doludursa kod göndərilib: 2-ci addım
        public string? DevCode { get; set; }                     // yalnız Development (SMTP yoxdur)
        public string? Error { get; set; }
    }
}
