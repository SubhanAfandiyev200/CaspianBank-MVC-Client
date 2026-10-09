using CaspianBank_MVC_FinalProject.ViewModels.CardDesigns;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace CaspianBank_MVC_FinalProject.ViewModels.CardTiers
{
    public class CardTierEditVM
    {
        public int Id { get; set; }

        // Rəqəmlər mətn kimi gəlir: "10,5" və "10.5" ikisi də qəbul olunsun (server mədəniyyətindən asılı olmasın), controller çevirir
        [Required(ErrorMessage = "Enter the issue fee.")]
        public string IssueFee { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the cashback percent.")]
        public string CashbackPercent { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the transfer limit.")]
        public string TransferLimit { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter the commission percent.")]
        public string CommissionPercent { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Choose a card design.")]
        public int CardDesignId { get; set; }

        // Yalnız göstərmək üçündür, formadan oxunmur: növün adı dəyişmir, dizaynlar API-dən alınır
        [BindNever]
        public string Tier { get; set; } = string.Empty;

        [BindNever]
        public List<CardDesignVM> Designs { get; set; } = new List<CardDesignVM>();

        [BindNever]
        public string CurrentDesignTitle { get; set; } = string.Empty;

        [BindNever]
        public string CurrentDesignImage { get; set; } = string.Empty;
    }
}
