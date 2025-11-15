using Biblio.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Biblio.Models.ViewModels
{
    public class UserManageViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public PlanType PlanType { get; set; }

        public string Role { get; set; }

        // Server-only SelectList for the edit form (not bound / validated)
        [ValidateNever]
        public SelectList? AvailableRoles { get; set; }
    }
}