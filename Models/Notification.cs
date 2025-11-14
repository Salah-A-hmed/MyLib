using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblio.Models
{
    // (جديد) أنواع الإشعارات
    public enum NotificationType
    {
        Information, // (أزرق) معلومة عامة
        Warning,     // (أصفر) تحذير (e.g., Due Soon, Out of Stock)
        Alert        // (أحمر) خطر (e.g., Overdue, Threshold Reached)
    }

    // (جديد) تعديل حالة الإشعار (عشان `Read` منطقية أكتر)
    public enum NotificationStatus
    {
        Unread,
        Read
    }

    public class Notification
    {
        public int ID { get; set; }

        [Required]
        public string Message { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        public DateTime Date { get; set; } = DateTime.Now;

        // --- (التعديلات هنا) ---

        // (1. جديد) تحديد نوع الإشعار
        public NotificationType Type { get; set; } = NotificationType.Information;

        // (2. جديد) لينك "اختياري"
        public string? LinkUrl { get; set; } // (e.g., /Borrowings/Index, /Books/Details/5)

        // (3. تعديل) الروابط كلها بقت اختيارية (Nullable)

        [ForeignKey("Visitor")]
        public int? VisitorID { get; set; } // (بقى Nullable)
        public Visitor? Visitor { get; set; }

        [ForeignKey("Book")]
        public int? BookID { get; set; } // (جديد و Nullable)
        public Book? Book { get; set; }

        [ForeignKey("Borrowing")]
        public int? BorrowingID { get; set; } // (جديد و Nullable بناءً على اقتراحك)
        public Borrowing? Borrowing { get; set; }

        // --- (نهاية التعديلات) ---

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}