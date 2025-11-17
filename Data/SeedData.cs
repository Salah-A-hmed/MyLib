using Biblio.Data; // (مهم) إضافة AppDbContext
using Biblio.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // (مهم) إضافة EntityFrameworkCore
using System; // (مهم) إضافة System
using System.Linq; // (مهم) إضافة Linq
using System.Threading.Tasks; // (مهم) إضافة Tasks

namespace Biblio.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            // (تعديل) جبنا الـ DbContext
            var _context = serviceProvider.GetRequiredService<AppDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

            // --- 1. Seed Roles ---
            string[] roles = { "Admin", "Librarian", "Reader" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // --- 2. Seed Admin User ---
            var adminUserName = "admin@biblio.com";
            var adminEmail = "admin@biblio.com";
            var adminPassword = "Admin@123";
            AppUser adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser { UserName = adminUserName, Email = adminEmail, FullName = "System Administrator", EmailConfirmed = true, PlanType = PlanType.Library };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // --- 3. (جديد) التحقق قبل إضافة باقي البيانات ---
            // لو فيه أي كتب، ده معناه إننا عملنا Seed قبل كده، هنوقف
            if (await _context.Books.AnyAsync())
            {
                return;
            }

            // --- 4. (جديد) إضافة Collections ---
            var colProgramming = new Collection { Name = "Programming", Description = "All about code", UserId = adminUser.Id };
            var colSciFi = new Collection { Name = "Science Fiction", Description = "Future and space", UserId = adminUser.Id };
            var colHistory = new Collection { Name = "History", Description = "About the past", UserId = adminUser.Id };
            await _context.Collections.AddRangeAsync(colProgramming, colSciFi, colHistory);
            await _context.SaveChangesAsync(); // (لازم Save عشان الـ IDs)

            // --- 5. (جديد) إضافة Visitors ---
            var visitor1 = new Visitor { Name = "Salah Ahmed", Email = "salah@test.com", Phone = "12345", UserId = adminUser.Id };
            var visitor2 = new Visitor { Name = "Test User", Email = "test@test.com", Phone = "67890", UserId = adminUser.Id };
            await _context.Visitors.AddRangeAsync(visitor1, visitor2);
            await _context.SaveChangesAsync(); // (لازم Save عشان الـ IDs)

            // --- 6. (جديد) إضافة Books ---
            var rand = new Random();
            var book1 = new Book { Title = "Clean Code", Author = "Robert C. Martin", TotalCopies = 3, DateAdded = DateTime.Now.AddMonths(-2).AddDays(-rand.Next(1, 28)), Price = 250, Rating = 5, Status = "Completed", UserId = adminUser.Id };
            var book2 = new Book { Title = "Dune", Author = "Frank Herbert", TotalCopies = 5, DateAdded = DateTime.Now.AddMonths(-10).AddDays(-rand.Next(1, 28)), Price = 180, Rating = 4.5m, Status = "In Progress", UserId = adminUser.Id };
            var book3 = new Book { Title = "1984", Author = "George Orwell", TotalCopies = 2, DateAdded = DateTime.Now.AddYears(-1).AddDays(-rand.Next(1, 28)), Price = 120, Rating = 4, Status = "Not Begun", UserId = adminUser.Id };
            var book4 = new Book { Title = "Sapiens", Author = "Yuval Noah Harari", TotalCopies = 1, DateAdded = DateTime.Now.AddMonths(-5).AddDays(-rand.Next(1, 28)), Price = 220, Rating = 4.5m, Status = "Completed", UserId = adminUser.Id };
            var book5 = new Book { Title = "The Pragmatic Programmer", Author = "Andrew Hunt", TotalCopies = 4, DateAdded = DateTime.Now.AddYears(-2).AddDays(-rand.Next(1, 28)), Price = 300, Rating = null, Status = "No Status", UserId = adminUser.Id };
            var book6 = new Book { Title = "Atomic Habits", Author = "James Clear", TotalCopies = 10, DateAdded = DateTime.Now.AddDays(-rand.Next(1, 28)), Price = 150, Rating = 4, Status = "In Progress", UserId = adminUser.Id };
            var book7 = new Book { Title = "A Brief History of Time", Author = "Stephen Hawking", TotalCopies = 2, DateAdded = DateTime.Now.AddMonths(-7).AddDays(-rand.Next(1, 28)), Price = 170, Rating = 3.5m, Status = "Abandoned", UserId = adminUser.Id };

            await _context.Books.AddRangeAsync(book1, book2, book3, book4, book5, book6, book7);
            await _context.SaveChangesAsync(); // (لازم Save عشان الـ IDs)

            // --- 7. (جديد) ربط الكتب بالكولكشنز (BookCollections) ---
            await _context.BookCollections.AddRangeAsync(
                new BookCollection { BookID = book1.ID, CollectionID = colProgramming.ID, UserId = adminUser.Id },
                new BookCollection { BookID = book5.ID, CollectionID = colProgramming.ID, UserId = adminUser.Id },
                new BookCollection { BookID = book2.ID, CollectionID = colSciFi.ID, UserId = adminUser.Id },
                new BookCollection { BookID = book3.ID, CollectionID = colSciFi.ID, UserId = adminUser.Id },
                new BookCollection { BookID = book4.ID, CollectionID = colHistory.ID, UserId = adminUser.Id },
                new BookCollection { BookID = book7.ID, CollectionID = colHistory.ID, UserId = adminUser.Id },
                new BookCollection { BookID = book6.ID, CollectionID = colProgramming.ID, UserId = adminUser.Id },
                new BookCollection { BookID = book6.ID, CollectionID = colHistory.ID, UserId = adminUser.Id } // (Atomic Habits في اتنين كولكشن)
            );

            // --- 8. (جديد) إضافة Borrowings (وتحديث الـ CheckedOutCopies) ---

            // استعارة نشطة (لازم نعدل الكتاب)
            var borrowing1 = new Borrowing { BookID = book1.ID, VisitorID = visitor1.ID, BorrowDate = DateTime.Now.AddDays(-10), DueDate = DateTime.Now.AddDays(4), Status = BorrowingStatus.Borrowed, UserId = adminUser.Id };
            book1.CheckedOutCopies++; // (مهم جداً)
            _context.Books.Update(book1);

            // استعارة متأخرة (لازم نعدل الكتاب)
            var borrowing2 = new Borrowing { BookID = book2.ID, VisitorID = visitor2.ID, BorrowDate = DateTime.Now.AddDays(-20), DueDate = DateTime.Now.AddDays(-6), Status = BorrowingStatus.Overdue, UserId = adminUser.Id };
            book2.CheckedOutCopies++; // (مهم جداً)
            _context.Books.Update(book2);

            // استعارة قديمة ورجعت (مش بنعدل الكتاب)
            var borrowing3 = new Borrowing { BookID = book3.ID, VisitorID = visitor1.ID, BorrowDate = DateTime.Now.AddMonths(-2), DueDate = DateTime.Now.AddMonths(-1), ReturnDate = DateTime.Now.AddMonths(-1), Status = BorrowingStatus.Returned, UserId = adminUser.Id };

            await _context.Borrowings.AddRangeAsync(borrowing1, borrowing2, borrowing3);

            // --- 9. (جديد) الحفظ النهائي ---
            await _context.SaveChangesAsync();
        }
    }
}