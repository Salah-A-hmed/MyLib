using System; // (1) لازم دي تكون موجودة عشان DateTime
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // (2) لازم دي تكون موجودة عشان [NotMapped] و [Column]

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
        // 1. ده بيمثل أنا أملك كام نسخة
        public int TotalCopies { get; set; } = 1; // (هنغير اسمه من StockCount)

        // 2. ده بيمثل كام نسخة معارة حالياً
        public int CheckedOutCopies { get; set; } = 0;

        // 3. ده هيتحسب أوتوماتيك ومش هيتخزن فالداتا بيز
        [NotMapped]
        public int AvailableCopies => TotalCopies - CheckedOutCopies;

        // 4. عشان الرسوم البيانية الزمنية
        public DateTime DateAdded { get; set; }

        // 5. عشان الرسوم البيانية المالية
        public decimal? Price { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public AppUser? User { get; set; }

        //Relations 
        public ICollection<BookCollection> Collections { get; set; } = new List<BookCollection>();
        public ICollection<Borrowing> Borrowings { get; set; } = new List<Borrowing>();

    }
}
