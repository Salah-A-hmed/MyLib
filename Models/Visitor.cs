using System.ComponentModel.DataAnnotations.Schema;

namespace Biblio.Models
{
    public class Visitor
    {
        // properties 
        public int ID { get; set; }

        public required string Name { get; set; }

        public required string Email { get; set; }

        public required string Phone { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }    
        public AppUser? User { get; set; }    

        //Relationships 

        // Visitor_Borrowings_relationship 

        public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();


        // Navigation property => A User can have many Notifications
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();




    }
}
