using System.ComponentModel.DataAnnotations.Schema;

namespace Biblio.Models
{
    public class Borrowing
    {
        public int ID { get; set; }

        public int VisitorID { get; set; }
        public Visitor? Visitor { get; set; }

        public int BookID { get; set; }
        public Book? Book { get; set; }

        public double? FineAmount { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public BorrowingStatus Status { get; set; } = BorrowingStatus.Borrowed;

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public AppUser? User { get; set; }

    }

    public enum BorrowingStatus { Borrowed, Returned, Overdue }
}