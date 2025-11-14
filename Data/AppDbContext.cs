using Biblio.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Biblio.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Notification>()
                .Property(n => n.Status)
                .HasConversion<string>()
                .HasDefaultValue(NotificationStatus.Unread);

            modelBuilder.Entity<Notification>()
                .Property(n => n.Date)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Borrowing>()
                .Property(b => b.Status)
                .HasConversion<string>()
                .HasDefaultValue(BorrowingStatus.Borrowed);

            modelBuilder.Entity<Borrowing>()
                .Property(n => n.FineAmount)
                .HasDefaultValue(0.0);


            modelBuilder.Entity<AppUser>()
                .Property(u => u.PlanType)
                .HasConversion<string>()
                .HasDefaultValue(PlanType.Free);

            modelBuilder.Entity<Book>()
                .Property(b => b.TotalCopies) // (تغيير الاسم)
                .HasDefaultValue(1);

            modelBuilder.Entity<Book>()
                .Property(b => b.CheckedOutCopies) // (إضافة جديدة)
                .HasDefaultValue(0);

            modelBuilder.Entity<Book>()
                .Property(b => b.DateAdded) // (إضافة جديدة)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Book>()
                .Property(b => b.Price) // (إضافة جديدة)
                .HasColumnType("decimal(10, 2)");

            modelBuilder.Entity<Collection>()
                .HasOne(c => c.User)
                .WithMany(u => u.Collections)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.User)
                .WithMany(u => u.Books)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.User)
                .WithMany(u => u.Borrowings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Visitor>()
                .HasOne(v => v.User)
                .WithMany(u => u.Visitors)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Visitor → Notifications (1-to-many)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Visitor)
                .WithMany(v => v.Notifications)
                .HasForeignKey(n => n.VisitorID)
                .OnDelete(DeleteBehavior.NoAction); // (تعديل: لو الزائر اتمسح، سيب الإشعار)

            // Book → Notifications (1-to-many)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Book)
                .WithMany() // (الكتاب مش محتاج لستة إشعارات)
                .HasForeignKey(n => n.BookID)
                .OnDelete(DeleteBehavior.NoAction);

            // Borrowing → Notifications (1-to-many)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Borrowing)
                .WithMany() // (الاستعارة مش محتاجة لستة إشعارات)
                .HasForeignKey(n => n.BorrowingID)
                .OnDelete(DeleteBehavior.NoAction); // changed from SetNull
            // Book → Borrowing (1-to-many) 
            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.Book)
                .WithMany(bk => bk.Borrowings)
                .HasForeignKey(b => b.BookID)
                .OnDelete(DeleteBehavior.Restrict);

            // Visitor → Borrowing (1-to-many) 
            modelBuilder.Entity<Borrowing>()
                .HasOne(b => b.Visitor)
                .WithMany(v => v.Borrowings)
                .HasForeignKey(b => b.VisitorID)
                .OnDelete(DeleteBehavior.Restrict);

            // BookCollection (many-to-many) // Simulate cascade delete in BookController & CollectionController
            modelBuilder.Entity<BookCollection>()
                .HasKey(bc => bc.ID);
            modelBuilder.Entity<BookCollection>()
                .HasOne(bc => bc.User)
                .WithMany(u => u.BookCollections)
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BookCollection>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.Collections)
                .HasForeignKey(bc => bc.BookID)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<BookCollection>()
                .HasOne(bc => bc.Collection)
                .WithMany(c => c.Books)
                .HasForeignKey(bc => bc.CollectionID)
                .OnDelete(DeleteBehavior.Restrict);

        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<BookCollection> BookCollections { get; set; }
        public DbSet<Visitor> Visitors { get; set; }
        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<Notification> Notifications { get; set; }

    }



}
