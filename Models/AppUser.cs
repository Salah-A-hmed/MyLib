using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Biblio.Models
{
    public class AppUser : IdentityUser
    {
        // Additional properties can be added here if needed
        public string? FullName { get; set; }
        public PlanType PlanType { get; set; } = PlanType.Free;
        public PayingPlanType? PayingPlanType { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextPaymentDate { get; set; }


        // Navigation property => A User can have many Collections
        public ICollection<Collection> Collections { get; set; } = new List<Collection>();
        public ICollection<Book> Books { get; set; } = new List<Book>();
        public ICollection<BookCollection> BookCollections { get; set; } = new List<BookCollection>();

        public ICollection<Visitor> Visitors { get; set; } = new List<Visitor>();
        public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
    public enum PlanType { Free, Library }
    public enum PayingPlanType { Monthly, Yearly }
}