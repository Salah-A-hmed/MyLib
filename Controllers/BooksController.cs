using Biblio.Data;
using Biblio.Models;
using Biblio.Models.ViewModels;
using Biblio.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace Biblio.Controllers
{
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IBookFilterService _bookFilterService; // <-- إضافة الـ Service
        public BooksController(AppDbContext context, IHttpClientFactory httpClientFactory, IBookFilterService bookFilterService)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _bookFilterService = bookFilterService;
        }

        // GET: Books
        public async Task<IActionResult> Index(string sortOrder, string statusFilter, string searchString, int? collectionId)
        {
            // get current user
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ----------------------------------------------------
            // قم بتعيين قيم افتراضية إذا كانت الباراميترات null (عند أول تحميل)
            // ----------------------------------------------------
            string effectiveSortOrder = sortOrder ?? "Title";
            string effectiveStatusFilter = statusFilter ?? "All";
            int? effectiveCollectionId = collectionId ?? 0; //  يمثل "الكل" 0

            // ----------------------------------------------------
            // الخطوة 1: تجهيز كل الـ ViewData (الفلاتر الحالية)
            // ----------------------------------------------------
            ViewData["CurrentSort"] = sortOrder ?? "Title";
            ViewData["StatusFilter"] = statusFilter ?? "All";
            ViewData["CurrentSearch"] = searchString;
            ViewData["CurrentCollection"] = collectionId;
            
            // ----------------------------------------------------
            // الخطوة 2: تجهيز قائمة الـ Collections للـ Dropdown
            // (ده الكود اللي سألت عليه، ومكانه هنا صحيح)
            // ----------------------------------------------------
            var collections = _context.Collections
                                      .Where(c => c.UserId == userId)
                                      .Select(c => new SelectListItem { Value = c.ID.ToString(), Text = c.Name }).ToList();
            collections.Insert(0, new SelectListItem { Value = "0", Text = "All Collections" });
            var selectedCollection = collections.FirstOrDefault(c => c.Value == effectiveCollectionId.ToString());
            if (selectedCollection != null)
            {
                selectedCollection.Selected = true;
            }
            ViewData["CollectionList"] = collections;
           
            // ------------------------------------------------------
            // الخطوة 3: استدعاء الـ Service لبناء الـ Query الرئيسية
            // ------------------------------------------------------
            var booksQuery = _bookFilterService.GetFilteredBooks(userId, sortOrder, statusFilter, searchString, collectionId);
            
            // ----------------------------------------------------
            // الخطوة 4: تنفيذ الـ Query وإرسال النتائج للـ View
            // ----------------------------------------------------            
            var books = await booksQuery.AsNoTracking().ToListAsync();
            return View(books);
        }

        // GET: Books/Details/5 (Replaced by a partial view)

        // -----------------------------------------------------------------
        // الخطوة 1: تعديل الـ Controller
        // -----------------------------------------------------------------

        // (جديد) GET: Books/AddBook
        // دي الصفحة الرئيسية الجديدة اللي فيها الـ Tabs
        public IActionResult AddBook()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // هنجهّز الـ ViewModel هنا عشان نمرره للـ Partial View بتاع الإضافة اليدوية
            var viewModel = new BookCreateViewModel
            {
                Collections = _context.Collections
                    .Where(c => c.UserId == userId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ID.ToString(),
                        Text = c.Name
                    }).ToList()
            };
            // هنرجع الـ View الجديد بتاعنا
            return View(viewModel);
        }


        // (تم الحذف) GET: Books/AddBySearch

        // POST: Books/AddBySearch  -> يبحث في Google Books ويرجع PartialView بنتايج البحث
        [HttpPost]
        public async Task<IActionResult> AddBySearch([FromForm] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return PartialView("_SearchResultsPartial", new List<BookCreateViewModel>());

            var client = _httpClientFactory.CreateClient();

            // نجهّز الـ URL بناءً على نوع البحث
            string url;
            bool isIsbn = query.All(char.IsDigit) && (query.Length == 10 || query.Length == 13);

            if (isIsbn)
                url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{Uri.EscapeDataString(query)}";
            else
                url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}";

            var json = await client.GetStringAsync(url);
            var parsed = JObject.Parse(json);
            var items = parsed["items"]?.ToArray() ?? Array.Empty<JToken>();

            // لو مفيش نتائج والبحث كان ISBN، نجرب كبحث عادي كـ fallback
            if (!items.Any() && isIsbn)
            {
                url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(query)}";
                json = await client.GetStringAsync(url);
                parsed = JObject.Parse(json);
                items = parsed["items"]?.ToArray() ?? Array.Empty<JToken>();
            }

            var results = items.Select(item =>
            {
                var vi = item["volumeInfo"];
                var industry = vi?["industryIdentifiers"]?.FirstOrDefault();
                string isbn = industry?["identifier"]?.ToString() ?? "";

                int? year = null;
                var pubDate = vi?["publishedDate"]?.ToString();
                if (!string.IsNullOrEmpty(pubDate) && pubDate.Length >= 4 && int.TryParse(pubDate.Substring(0, 4), out var y))
                    year = y;

                return new BookCreateViewModel
                {
                    Title = vi?["title"]?.ToString() ?? "",
                    Author = vi?["authors"]?.FirstOrDefault()?.ToString() ?? "",
                    Description = vi?["description"]?.ToString(),
                    Publisher = vi?["publisher"]?.ToString(),
                    PublishYear = year,
                    Pages = (int?)(vi?["pageCount"]?.ToObject<int>() ?? null),
                    ISBN = isbn,
                    CoverImageUrl = vi?["imageLinks"]?["thumbnail"]?.ToString(),
                    Category = vi?["categories"]?.FirstOrDefault()?.ToString()
                };
            }).ToList();

            return PartialView("_SearchResultsPartial", results);
        }

        // POST: Books/AddBookFromSearch  -> يضيف الكتاب فعليًا في DB (جاي من الـ modal مع selected collections)
        [HttpPost]
        public async Task<IActionResult> AddBookFromSearch([FromBody] BookCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (model == null || string.IsNullOrWhiteSpace(model.Title))
                return BadRequest("Invalid book data.");

            // لو الكتاب موجود فعلاً بنفس ISBN للمستخدم، ممكن تختار تمنع التكرار:
            if (!string.IsNullOrWhiteSpace(model.ISBN))
            {
                var exists = await _context.Books.AnyAsync(b => b.UserId == userId && b.ISBN == model.ISBN);
                if (exists)
                    return Conflict(new { message = "Book with same ISBN already exists." });
            }

            var book = new Book
            {
                Title = model.Title,
                Author = model.Author,
                Description = model.Description,
                Publisher = model.Publisher,
                PublishYear = model.PublishYear ?? 0,
                Pages = model.Pages ?? 0,
                ISBN = model.ISBN,
                CoverImageUrl = model.CoverImageUrl,
                Category = model.Category,
                UserId = userId,
                // --- (التعديلات هنا) ---
                TotalCopies = 1, // (بدلاً من StockCount)
                CheckedOutCopies = 0,
                DateAdded = DateTime.Now,
                Price = null // (Google API مش بيدينا سعر)
                // --- (نهاية التعديلات) ---
            };

            if (model.SelectedCollectionIds != null && model.SelectedCollectionIds.Any())
            {
                foreach (var collectionId in model.SelectedCollectionIds.Distinct())
                {
                    book.Collections.Add(new BookCollection
                    {
                        CollectionID = collectionId,
                        UserId = userId
                    });
                }
            }

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // إرجاع OK لليوزر-آي (الـ JS هيخفي الزر ويعرض Edit/Delete)
            return Ok(new { bookId = book.ID });
        }

        //(تم الحذف) GET: Books/Create

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                var book = new Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    Description = model.Description,
                    Publisher = model.Publisher,
                    PublishYear = model.PublishYear,
                    Pages = model.Pages,
                    ISBN = model.ISBN,
                    CoverImageUrl = model.CoverImageUrl,
                    Category = model.Category,
                    Rating = model.Rating,
                    Review = model.Review,
                    Status = model.Status,
                    UserId = userId,
                    // --- (التعديلات هنا) ---
                    TotalCopies = model.TotalCopies, // (بدلاً من StockCount)
                    Price = model.Price,
                    DateAdded = DateTime.Now,
                    CheckedOutCopies = 0
                    // --- (نهاية التعديلات) ---
                };

                foreach (var collectionId in model.SelectedCollectionIds)
                {
                    book.Collections.Add(new BookCollection
                    {
                        CollectionID = collectionId,
                        UserId = userId
                    });
                }

                _context.Add(book);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            model.Collections = _context.Collections
                 .Where(c => c.UserId == userId)
                 .Select(c => new SelectListItem
                 {
                     Value = c.ID.ToString(),
                     Text = c.Name
                 }).ToList();

            return View("AddBook",model);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var book = await _context.Books
                .Include(b => b.Collections)
                .ThenInclude(bc => bc.Collection)
                .FirstOrDefaultAsync(b => b.ID == id && b.UserId == userId);
            if (book == null)
            {
                return NotFound();
            }
            var viewModel = new BookEditViewModel
            {
                ID = book.ID,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Publisher = book.Publisher,
                PublishYear = book.PublishYear,
                Pages = book.Pages,
                ISBN = book.ISBN,
                CoverImageUrl = book.CoverImageUrl,
                Category = book.Category,
                Rating = book.Rating,
                Review = book.Review,
                Status = book.Status,
                // --- (التعديلات هنا) ---
                TotalCopies = book.TotalCopies, // (بدلاً من StockCount)
                Price = book.Price,
                // --- (نهاية التعديلات) ---
                SelectedCollectionIds = book.Collections.Select(bc => bc.CollectionID).ToList(),
                Collections = _context.Collections
                .Where(c => c.UserId == userId)
                .Select(c => new SelectListItem
                {
                    Value = c.ID.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookEditViewModel model)
        {
            if (id != model.ID)
                return NotFound();

            if (ModelState.IsValid)
            {
                var book = await _context.Books
                    .Include(b => b.Collections)
                    .FirstOrDefaultAsync(b => b.ID == id);

                if (book == null)
                    return NotFound();

                book.Title = model.Title;
                book.Author = model.Author;
                book.Description = model.Description;
                book.Publisher = model.Publisher;
                book.PublishYear = model.PublishYear;
                book.Pages = model.Pages;
                book.ISBN = model.ISBN;
                book.CoverImageUrl = model.CoverImageUrl;
                book.Category = model.Category;
                book.Rating = model.Rating;
                book.Review = model.Review;
                book.Status = model.Status;
                // --- (التعديلات هنا) ---
                book.TotalCopies = model.TotalCopies; // (بدلاً من StockCount)
                book.Price = model.Price;
                // (مش هنعدل DateAdded ولا CheckedOutCopies هنا)
                // --- (نهاية التعديلات) ---
                book.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var oldRelations = _context.BookCollections.Where(bc => bc.BookID == id);
                _context.BookCollections.RemoveRange(oldRelations);
                await _context.SaveChangesAsync();

                book.Collections.Clear(); // (تأكيد إضافي)
                foreach (var collectionId in model.SelectedCollectionIds)
                {
                    book.Collections.Add(new BookCollection { CollectionID = collectionId , UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) });
                }

                _context.Update(book); // تحديث الكتاب بالعلاقات الجديدة
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.Collections = _context.Collections
                .Where(c => c.UserId == userId)
                .Select(c => new SelectListItem
                {
                    Value = c.ID.ToString(),
                    Text = c.Name
                })
                .ToList();

            return View(model);
        }

        // GET: Books/Delete/5 (Replaced by a Modal)
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    var book = await _context.Books
        //        .Include(b => b.User)
        //        .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);
        //    if (book == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(book);
        //}

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var book = await _context.Books.Include(b => b.Borrowings).Include(b => b.Collections).FirstOrDefaultAsync(b => b.ID == id && b.UserId == userId);
            if (book != null)
            {
                _context.Borrowings.RemoveRange(book.Borrowings);
                _context.BookCollections.RemoveRange(book.Collections);
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.ID == id);
        }
    }
}