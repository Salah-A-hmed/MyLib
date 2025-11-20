namespace Biblio.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string? FullName { get; set; }
        public string ? UserName { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Email { get; set; }
        public  bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PlanType { get; set; }
        public string? PaymentPlan { get; set; }
        public string? NextPayment { get; set; }
        public decimal? PaymentPrice { get; set; }

    }
}
