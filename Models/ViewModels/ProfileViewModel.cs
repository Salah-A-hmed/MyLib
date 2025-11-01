namespace Biblio.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PlanType { get; set; }
        public IList<string>? Roles { get; set; }

    }
}
