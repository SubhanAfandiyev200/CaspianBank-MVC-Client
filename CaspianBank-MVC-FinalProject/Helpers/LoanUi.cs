using System.Globalization;

namespace CaspianBank_MVC_FinalProject.Helpers
{
    // Kredit səhifələrinin ortaq mətn və rəng köməkçiləri (müştəri və işçi görünüşləri eyni sözləri işlədir)
    public static class LoanUi
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        public static string Money(decimal value)
        {
            return value.ToString("N2", Inv) + " AZN";
        }

        public static string Date(DateTime value)
        {
            return value.ToString("dd MMM yyyy", Inv);
        }

        public static string Date(DateTime? value)
        {
            return value.HasValue ? Date(value.Value) : "—";
        }

        // Faiz: 10 -> "10", 9.5 -> "9.5"
        public static string Percent(decimal value)
        {
            return value.ToString("0.##", Inv) + "%";
        }

        public static string StatusText(string status)
        {
            switch (status)
            {
                case "Pending":
                    return "Under review";
                case "Offered":
                    return "Offer waiting";
                case "Active":
                    return "Active";
                case "Closed":
                    return "Paid off";
                case "Rejected":
                    return "Not approved";
                case "Declined":
                    return "Declined";
                case "Court":
                    return "In court";
                default:
                    return status;
            }
        }

        // Mövcud .badge rəngləri
        public static string StatusBadge(string status)
        {
            switch (status)
            {
                case "Pending":
                    return "badge-pending";
                case "Offered":
                    return "badge-info";
                case "Active":
                    return "badge-success";
                case "Closed":
                    return "badge-neutral";
                case "Rejected":
                case "Declined":
                case "Court":
                    return "badge-danger";
                default:
                    return "badge-neutral";
            }
        }

        // İşçi tərəfində statuslar daha dəqiq yazılır (müştəri "Not approved" görür, işçi "Rejected")
        public static string StaffStatusText(string status)
        {
            switch (status)
            {
                case "Pending":
                    return "Pending";
                case "Offered":
                    return "Offered";
                case "Active":
                    return "Active";
                case "Closed":
                    return "Closed";
                case "Rejected":
                    return "Rejected";
                case "Declined":
                    return "Declined by customer";
                case "Court":
                    return "Court";
                default:
                    return status;
            }
        }

        public static string PurposeText(string purpose)
        {
            switch (purpose)
            {
                case "Consumer":
                    return "Consumer";
                case "Car":
                    return "Car";
                case "Mortgage":
                    return "Mortgage";
                case "Education":
                    return "Education";
                case "Medical":
                    return "Medical";
                case "Repair":
                    return "Repair";
                case "Business":
                    return "Business";
                case "Other":
                    return "Other";
                default:
                    return purpose;
            }
        }

        public static string InstallmentText(string status)
        {
            switch (status)
            {
                case "Paid":
                    return "Paid";
                case "Prepaid":
                    return "Paid early";
                case "Overdue":
                    return "Overdue";
                case "Upcoming":
                    return "Upcoming";
                default:
                    return status;
            }
        }

        public static string InstallmentBadge(string status)
        {
            switch (status)
            {
                case "Paid":
                case "Prepaid":
                    return "badge-success";
                case "Overdue":
                    return "badge-danger";
                default:
                    return "badge-neutral";
            }
        }

        public static string PaymentTypeText(string type)
        {
            switch (type)
            {
                case "Installment":
                    return "Monthly payment";
                case "PartialEarly":
                    return "Early payment";
                case "FullEarly":
                    return "Full payoff";
                default:
                    return type;
            }
        }

        // Cashback kartına kredit pulu düşmür və o kartdan kredit ödənmir; bloklanmış kart da olmaz
        public static bool CanUseCard(bool isBlocked, string tier)
        {
            return !isBlocked && !string.Equals(tier, "Cashback", StringComparison.OrdinalIgnoreCase);
        }
    }
}
