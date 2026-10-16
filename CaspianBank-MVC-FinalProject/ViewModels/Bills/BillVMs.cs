using CaspianBank_MVC_FinalProject.ViewModels.Cards;

namespace CaspianBank_MVC_FinalProject.ViewModels.Bills
{
    // API-dən gələn təchizatçı (Bills səhifəsindəki seçim)
    public class BillProviderUIVM
    {
        public int Id { get; set; }                                  // -1 = Caspian krediti (API-də təchizatçı deyil, controller əlavə edir)
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;         // Mobile, Electricity, Gas, Water, Internet, Loan
        public string? LogoUrl { get; set; }                         // controller tam ünvana çevirir
        public string IdentifierLabel { get; set; } = string.Empty;
        public string IdentifierHint { get; set; } = string.Empty;
        public string? PrefixesText { get; set; }
        public int IdentifierLength { get; set; }
        public string? UnitName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal MinPayment { get; set; }
        public decimal MaxPayment { get; set; }
    }

    // Nömrə və ya abunəçi kodu ilə axtarışın nəticəsi
    public class BillLookupUIVM
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string SubscriberName { get; set; } = string.Empty;
        public decimal Debt { get; set; }
        public decimal Credit { get; set; }
        public decimal SuggestedAmount { get; set; }
        public decimal? MonthlyFee { get; set; }
        public string? PackageName { get; set; }
        public string? LastChargePeriod { get; set; }
        public decimal? LastChargeAmount { get; set; }
        public decimal? LastChargeUsage { get; set; }
        public string? UnitName { get; set; }
        public decimal MinPayment { get; set; }
        public decimal MaxPayment { get; set; }

        // Kredit koduyla ödənişdə məbləğ aylıq borcdur və dəyişmir
        public bool FixedAmount { get; set; }
    }

    public class BillReceiptUIVM
    {
        public string Reference { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string SubscriberName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal DebtCleared { get; set; }
        public decimal CreditAdded { get; set; }
        public decimal RemainingDebt { get; set; }
        public decimal BalanceAfter { get; set; }
        public decimal CashbackEarned { get; set; }
        public string CardLabel { get; set; } = string.Empty;
        public decimal CardBalanceAfter { get; set; }
        public DateTime PaidAt { get; set; }

        // Yalnız kredit ödənişində: aylıq ödənişin tərkibi
        public decimal? Principal { get; set; }
        public decimal? Interest { get; set; }
        public decimal? Penalty { get; set; }

        // "Hər ay təkrarla" seçilibsə nəticə (API qaytarmır, controller doldurur): ödəniş keçsə də təkrar yazılmaya bilər
        public string? RecurringNote { get; set; }
        public bool RecurringFailed { get; set; }
    }

    // "Recurring payments" cədvəlinin sətri (api/bills/recurring)
    public class RecurringPaymentUIVM
    {
        public int Id { get; set; }
        public int? ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public decimal Amount { get; set; }                          // kredit üçün 0
        public string CardLabel { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;           // Active, Paused, Failed
        public string? LastError { get; set; }
        public DateTime NextRunDate { get; set; }
    }

    // "Son ödənişlər" cədvəlinin sətri
    public class BillPaymentUIVM
    {
        public int Id { get; set; }
        public DateTime PaidAt { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal CashbackEarned { get; set; }
        public string CardLabel { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
    }

    public class BillsOverviewUIVM
    {
        public decimal PaidThisMonth { get; set; }
        public int PaymentsThisMonth { get; set; }
        public decimal CashbackThisMonth { get; set; }
        public List<BillPaymentUIVM> RecentPayments { get; set; } = new();
    }

    // API-nin kredit kodu axtarışı (api/loans/lookup/{code}): Bills səhifəsi bunu BillLookupUIVM-ə çevirir
    public class LoanLookupUIVM
    {
        public string ContractNumber { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal AnnualInterestRate { get; set; }
        public decimal DueTotal { get; set; }
        public decimal OverdueTotal { get; set; }
        public DateTime? NextDueDate { get; set; }
        public bool CanPay { get; set; }
    }

    // Formada daxil edilənlər (xətadan sonra saxlanır)
    public class BillPayFormVM
    {
        public int ProviderId { get; set; }
        public string? Identifier { get; set; }
        public string? Amount { get; set; }
        public int CardId { get; set; }
        public bool Recurring { get; set; }                          // "Hər ay təkrarla" qutusu
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
    }

    public class BillsPageVM
    {
        public List<BillProviderUIVM> Providers { get; set; } = new();
        public BillsOverviewUIVM Overview { get; set; } = new();
        public List<RecurringPaymentUIVM> Recurring { get; set; } = new();
        public List<CardUIVM> Cards { get; set; } = new();
        public BillPayFormVM Form { get; set; } = new();

        // Xətadan sonra səhifə açılanda axtarışın nəticəsi yenidən göstərilir
        public BillLookupUIVM? Lookup { get; set; }
        public string? Error { get; set; }
        public bool ApiUnavailable { get; set; }
    }
}
