using System.ComponentModel.DataAnnotations.Schema;

namespace Biblio.Models
{
    public class Notification
    {
        public int ID { get; set; }   // PK

        [ForeignKey("Visitor")]
        public int VisitorID { get; set; }   // FK => Visitor table
        public Visitor? Visitor { get; set; }
        public string Message { get; set; }
        public DateTime? Date { get; set; }
        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public AppUser? User { get; set; }

    }


    public enum NotificationStatus
    {
        Unread,
        Read
    }
}