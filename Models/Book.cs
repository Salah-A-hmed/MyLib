using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblio.Models
{
    public class Book
    {
        // Properties 
        public int ID { get; set; }
        public required string Title { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public int? PublishYear { get; set; }
        public int? Pages { get; set; }
        public string? ISBN { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Category { get; set; } // Fiction, Non-Fiction, Science, History, Biography, etc.
        public decimal? Rating { get; set; } // from 0 to 5
        public string? Review { get; set; }
        public string? Status { get; set; } // abandoned, Not Begun, In Progress, Completed
        public int? StockCount { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public AppUser? User { get; set; }

        //Relations 

        // Book_Collection_Relationship 
        public ICollection<BookCollection> Collections { get; set; } = new List<BookCollection>();

        // Book_Borrowings_relationship 

        public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    }
}
