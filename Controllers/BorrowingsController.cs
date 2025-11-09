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

namespace Biblio.Controllers
{
    public class BorrowingsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public BorrowingsController(AppDbContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: Borrowings
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appDbContext = _context.Borrowings.Include(b => b.Book).Include(b => b.User).Include(b => b.Visitor).Where(b => b.UserId == userId);
            foreach (var item in appDbContext)
            {
                if (item.Status == BorrowingStatus.Borrowed && item.DueDate < DateTime.Now)
                {
                    item.Status = BorrowingStatus.Overdue;
                    item.FineAmount= ((DateTime.Now - item.DueDate).Days) * 5.0; 
                    _context.Update(item);
                }
            }

            await _context.SaveChangesAsync();

            return View(await appDbContext.ToListAsync());
        }

        // GET: Borrowings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var borrowing = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.User)
                .Include(b => b.Visitor)
                .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);
            if (borrowing == null)
            {
                return NotFound();
            }

            return View(borrowing);
        }

        // GET: Borrowings/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            var BookSelectListItems = _context.Books
                .Where(b => b.UserId == userId)
                .Select(b => new SelectListItem { Value = b.ID.ToString(), Text = b.Title }).ToList();
            var VisitorSelectListItems = _context.Visitors
                .Where(v => v.UserId == userId)
                .Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name }).ToList();
            ViewBag.BookSelectList = BookSelectListItems;
            ViewBag.VisitorSelectList = VisitorSelectListItems;
            return View();
        }

        // POST: Borrowings/Create      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Borrowing borrowing)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            borrowing.UserId = userId;
            if (ModelState.IsValid)
            {
                var borrowedBook = await _context.Books.FirstOrDefaultAsync(b => b.ID == borrowing.BookID && b.UserId == userId);
                if (borrowedBook.StockCount > 0)
                {
                    borrowedBook.StockCount--;
                    _context.Update(borrowedBook);
                    _context.Add(borrowing);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("", "The selected book is out of stock.");
                }
            }
            var BookSelectListItems = _context.Books
                .Where(b => b.UserId == userId)
                .Select(b => new SelectListItem { Value = b.ID.ToString(), Text = b.Title })
                .ToList();

            var VisitorSelectListItems = _context.Visitors
                .Where(v => v.UserId == userId)
                .Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name })
                .ToList();

            ViewBag.BookSelectList = BookSelectListItems;
            ViewBag.VisitorSelectList = VisitorSelectListItems;
            return View(borrowing);
        }

         //GET: Borrowings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var borrowing = await _context.Borrowings.FirstOrDefaultAsync(b => b.ID == id && b.UserId == userId);
            if (borrowing == null)
            {
                return NotFound();
            }
            var BookSelectListItems = _context.Books
                .Where(b => b.UserId == userId)
                .Select(b => new SelectListItem { Value = b.ID.ToString(), Text = b.Title , Selected = (b.ID == borrowing.BookID) }).ToList();
            var VisitorSelectListItems = _context.Visitors
                .Where(v => v.UserId == userId)
                .Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name , Selected = (v.ID == borrowing.VisitorID)}).ToList();
            ViewBag.BookSelectList = BookSelectListItems;
            ViewBag.VisitorSelectList = VisitorSelectListItems;
            return View(borrowing);
        }

        // POST: Borrowings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Borrowing borrowing)
        {
            if (id != borrowing.ID)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            borrowing.UserId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    var oldBorrowing = await _context.Borrowings
                        .Include(b => b.Book)
                        .FirstOrDefaultAsync(b => b.ID == id);

                    if (oldBorrowing == null)
                    {
                        return NotFound();
                    }

                    var oldStatus = oldBorrowing.Status;
                    var newStatus = borrowing.Status;

                    if ( (oldStatus == BorrowingStatus.Borrowed || oldStatus == BorrowingStatus.Overdue) && newStatus == BorrowingStatus.Returned)
                    {
                        var book = await _context.Books.FindAsync(borrowing.BookID);
                        if (book != null)
                        {
                            book.StockCount++;
                            _context.Update(book);
                        }
                    }

                    else if (oldStatus == BorrowingStatus.Returned && (newStatus == BorrowingStatus.Borrowed || newStatus == BorrowingStatus.Overdue))
                    {
                        var book = await _context.Books.FindAsync(borrowing.BookID);
                        if (book != null && book.StockCount > 0)
                        {
                            book.StockCount--;
                            _context.Update(book);
                        }
                        else
                        {
                            ModelState.AddModelError("", "This book is not available for borrowing right now.");
                            return View(borrowing);
                        }
                    }

                    _context.Entry(oldBorrowing).CurrentValues.SetValues(borrowing);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BorrowingExists(borrowing.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // في حالة الخطأ
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
        // GET: Borrowings/Delete/5 (Replaced by a Modal)
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    var borrowing = await _context.Borrowings
        //        .Include(b => b.Book)
        //        .Include(b => b.User)
        //        .Include(b => b.Visitor)
        //        .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);
        //    if (borrowing == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(borrowing);
        //}

        // POST: Borrowings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var borrowing = await _context.Borrowings.FirstOrDefaultAsync(b => b.ID == id && b.UserId == userId);
            if (borrowing != null)
            {
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
