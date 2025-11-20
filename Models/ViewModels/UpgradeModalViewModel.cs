// Models/ViewModels/UpgradeModalViewModel.cs (Add these two classes)

namespace Biblio.Models.ViewModels
{
    public class UpgradeModalViewModel
    {
        public string PayingPlanType { get; set; } = "Monthly";
        public decimal Price { get; set; }
        public string Period { get; set; } = "/ Month";
        public string BillingText { get; set; } = "Billed Monthly";
        public string ActionUrl { get; set; } = "";
    }

    public class DowngradeModalViewModel
    {
        public string ActionUrl { get; set; } = "";
    }
}