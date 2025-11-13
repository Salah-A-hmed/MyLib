using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Biblio.Data;
using Biblio.Models;
using Biblio.Models.ViewModels; // (إضافة جديدة)

namespace Biblio.Controllers
{
    public class BorrowingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private const double FinePerDay = 5.0; // (ثابت عشان الغرامة)

        public BorrowingsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // (تم تعديل الدالة دي بالكامل)
        // GET: Borrowings
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var today = DateTime.Now.Date;

            // --- (الخطوة الأهم) تحديث الحالات المتأخرة (Overdue) تلقائياً ---
            var borrowedItemsToUpdate = await _context.Borrowings
                .Where(b => b.UserId == userId &&
                            b.Status == BorrowingStatus.Borrowed && // (الحالة "Borrowed" فقط)
                            b.DueDate.Date < today) // (تاريخ الاستحقاق فات)
                .ToListAsync();

            if (borrowedItemsToUpdate.Any())
            {
                foreach (var item in borrowedItemsToUpdate)
                {
                    item.Status = BorrowingStatus.Overdue;
                    item.FineAmount = ((today - item.DueDate.Date).Days) * FinePerDay;
                    _context.Update(item);
                }
                await _context.SaveChangesAsync(); // (احفظ التغييرات دي الأول)
            }

            // --- بعد التحديث، هنجيب كل البيانات عشان نعرضها ---
            var allUserBorrowings = await _context.Borrowings
                .Include(b => b.Book)    // (مهم عشان اسم وصورة الكتاب)
                .Include(b => b.Visitor) // (مهم عشان اسم الزائر)
                .Where(b => b.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            // 1. فلترة البيانات
            var activeBorrowings = allUserBorrowings
                .Where(b => b.Status == BorrowingStatus.Borrowed || b.Status == BorrowingStatus.Overdue)
                .OrderBy(b => b.DueDate) // (الأقدم أولاً)
                .ToList();

            var returnedBorrowings = allUserBorrowings
                .Where(b => b.Status == BorrowingStatus.Returned)
                .OrderByDescending(b => b.ReturnDate) // (الأحدث أولاً)
                .ToList();

            // 2. تجهيز الـ ViewModel
            var viewModel = new BorrowingsDashboardViewModel
            {
                ActiveBorrowings = activeBorrowings,
                ReturnedBorrowings = returnedBorrowings,

                // 3. حساب KPIs التاب الثاني (Active)
                KpiTotalActive = activeBorrowings.Count,
                KpiTotalOverdue = activeBorrowings.Count(b => b.Status == BorrowingStatus.Overdue),
                KpiDueSoon = activeBorrowings.Count(b =>
                    b.Status == BorrowingStatus.Borrowed &&
                    b.DueDate.Date >= today &&
                    b.DueDate.Date <= today.AddDays(2)), // (اللي معاده النهاردة أو بكرة أو بعده)

                // 4. حساب KPIs التاب الثالث (Returned)
                KpiTotalReturned = returnedBorrowings.Count,
                KpiOnTime = returnedBorrowings.Count(b => b.ReturnDate.HasValue && b.ReturnDate.Value.Date <= b.DueDate.Date),
                KpiTotalCollectedFines = returnedBorrowings.Sum(b => b.FineAmount)
            };

            return View(viewModel);
        }
        // (جديد) الدالة دي بتجيب بيانات التاب الأول (Active)
        [HttpGet]
        public async Task<IActionResult> GetActiveTabPartial()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var today = DateTime.Now.Date;

            // (ده نفس لوجيك تحديث الغرامات اللي في Index)
            var borrowedItemsToUpdate = await _context.Borrowings
                .Where(b => b.UserId == userId &&
                            b.Status == BorrowingStatus.Borrowed &&
                            b.DueDate.Date < today)
                .ToListAsync();

            if (borrowedItemsToUpdate.Any())
            {
                foreach (var item in borrowedItemsToUpdate)
                {
                    item.Status = BorrowingStatus.Overdue;
                    item.FineAmount = ((today - item.DueDate.Date).Days) * FinePerDay;
                    _context.Update(item);
                }
                await _context.SaveChangesAsync();
            }

            var activeBorrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.Visitor)
                .Where(b => b.UserId == userId && (b.Status == BorrowingStatus.Borrowed || b.Status == BorrowingStatus.Overdue))
                .OrderBy(b => b.DueDate)
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new BorrowingsDashboardViewModel
            {
                ActiveBorrowings = activeBorrowings,
                KpiTotalActive = activeBorrowings.Count,
                KpiTotalOverdue = activeBorrowings.Count(b => b.Status == BorrowingStatus.Overdue),
                KpiDueSoon = activeBorrowings.Count(b =>
                    b.Status == BorrowingStatus.Borrowed &&
                    b.DueDate.Date >= today &&
                    b.DueDate.Date <= today.AddDays(2)),
            };

            return PartialView("_ActiveTabPartial", viewModel);
        }

        // (جديد) الدالة دي بتجيب بيانات التاب التاني (Returned)
        [HttpGet]
        public async Task<IActionResult> GetReturnedTabPartial()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var returnedBorrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.Visitor)
                .Where(b => b.UserId == userId && b.Status == BorrowingStatus.Returned)
                .OrderByDescending(b => b.ReturnDate)
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new BorrowingsDashboardViewModel
            {
                ReturnedBorrowings = returnedBorrowings,
                KpiTotalReturned = returnedBorrowings.Count,
                KpiOnTime = returnedBorrowings.Count(b => b.ReturnDate.HasValue && b.ReturnDate.Value.Date <= b.DueDate.Date),
                KpiTotalCollectedFines = returnedBorrowings.Sum(b => b.FineAmount)
            };

            return PartialView("_ReturnedTabPartial", viewModel);
        }
        // (جديد) دالة الـ AJAX بتاعة "Return Book"
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var borrowing = await _context.Borrowings
                .Include(b => b.Book) // (لازم نجيب الكتاب عشان نعدل النسخ)
                .FirstOrDefaultAsync(b => b.ID == id && b.UserId == userId);

            if (borrowing == null)
            {
                return NotFound(new { success = false, message = "Borrowing record not found." });
            }

            if (borrowing.Status == BorrowingStatus.Returned)
            {
                return BadRequest(new { success = false, message = "Book already returned." });
            }

            bool wasOverdue = borrowing.Status == BorrowingStatus.Overdue;

            // (اللوجيك الأهم) تحديث الحالة وتاريخ الإرجاع وتحديث النسخ
            borrowing.Status = BorrowingStatus.Returned;
            borrowing.ReturnDate = DateTime.Now;

            if (borrowing.Book != null)
            {
                borrowing.Book.CheckedOutCopies--; // (تعديل: بننقص النسخ المعارة)
                _context.Update(borrowing.Book);
            }

            _context.Update(borrowing);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, wasOverdue = wasOverdue });
        }


        // GET: Borrowings/Details/5 (زي ما هو)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var borrowing = await _context.Borrowings
                .Include(b => b.Book).Include(b => b.User).Include(b => b.Visitor)
                .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);
            if (borrowing == null) return NotFound();
            return View(borrowing);
        }

        // (جديد) دالة الـ AJAX للبحث عن الكتب المتاحة
        [HttpGet]
        public async Task<IActionResult> SearchAvailableBooks(string query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var queryLower = query?.ToLower() ?? "";

            var booksQuery = _context.Books
                .Where(b => b.UserId == userId &&
                            (b.TotalCopies - b.CheckedOutCopies) > 0); // (مهم: الكتب المتاحة فقط)

            if (!string.IsNullOrEmpty(queryLower))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.ToLower().Contains(queryLower) ||
                    (b.Author != null && b.Author.ToLower().Contains(queryLower)) ||
                    (b.ISBN != null && b.ISBN.Contains(queryLower))
                );
            }

            var books = await booksQuery
                .Take(20) // (هات أول 20 نتيجة بس)
                .AsNoTracking()
                .ToListAsync();

            return PartialView("_LendingBookSearchResultsPartial", books);
        }

        // (جديد) دالة الـ AJAX للبحث عن الزوار
        [HttpGet]
        public async Task<IActionResult> SearchVisitors(string query)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var queryLower = query?.ToLower() ?? "";

            var visitorsQuery = _context.Visitors
                .Where(v => v.UserId == userId);

            if (!string.IsNullOrEmpty(queryLower))
            {
                visitorsQuery = visitorsQuery.Where(v =>
                    v.Name.ToLower().Contains(queryLower) ||
                    (v.Email != null && v.Email.ToLower().Contains(queryLower))
                );
            }

            var visitors = await visitorsQuery
                .Take(10) // (هات أول 10 نتايج)
                .AsNoTracking()
                .ToListAsync();

            return PartialView("_LendingVisitorSearchResultsPartial", visitors);
        }
        // GET: Borrowings/Create (زي ما هو)
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            var BookSelectListItems = _context.Books
                .Where(b => b.UserId == userId && (b.TotalCopies - b.CheckedOutCopies) > 0)
                .Select(b => new SelectListItem { Value = b.ID.ToString(), Text = b.Title }).ToList();
            var VisitorSelectListItems = _context.Visitors
                .Where(v => v.UserId == userId)
                .Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name }).ToList();
            ViewBag.BookSelectList = BookSelectListItems;
            ViewBag.VisitorSelectList = VisitorSelectListItems;
           return PartialView("_CreatePartial");
        }

        // POST: Borrowings/Create (زي ما هو)
        // POST: Borrowings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Borrowing borrowing)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // --- (التعديل هنا) ---
            // (1) تثبيت القيم اللي جاية من السيرفر
            borrowing.UserId = userId;
            borrowing.Status = BorrowingStatus.Borrowed;
            borrowing.BorrowDate = DateTime.Now;

            // (2) نتأكد إن الـ BookID و VisitorID اللي الـ JS بعتهم موجودين
            if (borrowing.BookID <= 0 || borrowing.VisitorID <= 0)
            {
                ModelState.AddModelError("", "Please select a valid book and visitor.");
            }
            // --- (نهاية التعديل) ---

            var borrowedBook = await _context.Books.FirstOrDefaultAsync(b => b.ID == borrowing.BookID && b.UserId == userId);

            if (ModelState.IsValid && borrowedBook != null)
            {
                if (borrowedBook.AvailableCopies > 0)
                {
                    borrowedBook.CheckedOutCopies++;
                    _context.Update(borrowedBook);
                    _context.Add(borrowing);
                    await _context.SaveChangesAsync();

                    // (ده الـ Success Path)
                    // هيرجع Redirect، والـ JS هيعمل ريفريش للصفحة
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "The selected book is out of stock.");
                }
            }

            // --- (Failure Path) ---
            // (ده الكود اللي هيشتغل لو فيه خطأ)
            // هيرجع الـ PartialView، والـ JS هيعرضه في التاب
            return PartialView("_CreatePartial", borrowing);
        }
        //GET: Borrowings/Edit/5 (زي ما هو)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var borrowing = await _context.Borrowings.FirstOrDefaultAsync(b => b.ID == id && b.UserId == userId);
            if (borrowing == null) return NotFound();
            var BookSelectListItems = _context.Books
                .Where(b => b.UserId == userId)
                .Select(b => new SelectListItem { Value = b.ID.ToString(), Text = b.Title, Selected = (b.ID == borrowing.BookID) }).ToList();
            var VisitorSelectListItems = _context.Visitors
                .Where(v => v.UserId == userId)
                .Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name, Selected = (v.ID == borrowing.VisitorID) }).ToList();
            ViewBag.BookSelectList = BookSelectListItems;
            ViewBag.VisitorSelectList = VisitorSelectListItems;
            return View(borrowing);
        }

        // POST: Borrowings/Edit/5 (زي ما هو)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Borrowing borrowing)
        {
            if (id != borrowing.ID) return NotFound();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            borrowing.UserId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    var oldBorrowing = await _context.Borrowings.AsNoTracking().FirstOrDefaultAsync(b => b.ID == id);
                    if (oldBorrowing == null) return NotFound();
                    var oldStatus = oldBorrowing.Status;
                    var newStatus = borrowing.Status;
                    var book = await _context.Books.FindAsync(borrowing.BookID);
                    if (book == null)
                    {
                        ModelState.AddModelError("", "Book not found.");
                        ViewBag.BookSelectList = await _context.Books.Where(b => b.UserId == userId).Select(b => new SelectListItem { Value = b.ID.ToString(), Text = b.Title, Selected = (b.ID == borrowing.BookID) }).ToListAsync();
                        ViewBag.VisitorSelectList = await _context.Visitors.Where(v => v.UserId == userId).Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name, Selected = (v.ID == borrowing.VisitorID) }).ToListAsync();
                        return View(borrowing);
                    }

                    if ((oldStatus == BorrowingStatus.Borrowed || oldStatus == BorrowingStatus.Overdue) && newStatus == BorrowingStatus.Returned)
                    {
                        book.CheckedOutCopies--;
                        _context.Update(book);
                    }
                    else if (oldStatus == BorrowingStatus.Returned && (newStatus == BorrowingStatus.Borrowed || newStatus == BorrowingStatus.Overdue))
                    {
                        if (book.AvailableCopies > 0)
                        {
                            book.CheckedOutCopies++;
                            _context.Update(book);
                        }
                        else
                        {
                            ModelState.AddModelError("", "This book is not available for borrowing right now.");
                            ViewBag.BookSelectList = await _context.Books.Where(b => b.UserId == userId).Select(b => new SelectListItem { Value = b.ID.ToString(), Text = b.Title, Selected = (b.ID == borrowing.BookID) }).ToListAsync();
                            ViewBag.VisitorSelectList = await _context.Visitors.Where(v => v.UserId == userId).Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name, Selected = (v.ID == borrowing.VisitorID) }).ToListAsync();
                            return View(borrowing);
                        }
                    }

                    _context.Update(borrowing);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowingExists(borrowing.ID)) return NotFound();
                    else throw;
                }
            }
            ViewBag.BookSelectList = await _context.Books.Where(b => b.UserId == userId).Select(b => new SelectListItem { Value = b.ID.ToString(), Text = b.Title, Selected = (b.ID == borrowing.BookID) }).ToListAsync();
            ViewBag.VisitorSelectList = await _context.Visitors.Where(v => v.UserId == userId).Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name, Selected = (v.ID == borrowing.VisitorID) }).ToListAsync();
            return View(borrowing);
        }

        // POST: Borrowings/Delete/5 (زي ما هو)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var borrowing = await _context.Borrowings.FirstOrDefaultAsync(b => b.ID == id && b.UserId == userId);
            if (borrowing != null)
            {
                if (borrowing.Status != BorrowingStatus.Returned)
                {
                    var book = await _context.Books.FindAsync(borrowing.BookID);
                    if (book != null)
                    {
                        book.CheckedOutCopies--;
                        _context.Update(book);
                    }
                }
                _context.Borrowings.Remove(borrowing);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BorrowingExists(int id)
        {
            return _context.Borrowings.Any(e => e.ID == id);
        }
    }
}