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
using System.Globalization; // (إضافة جديدة)
namespace Biblio.Controllers
{
    public class CollectionsController : Controller
    {
        private readonly AppDbContext _context;

        public CollectionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Collections
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var viewModel = new CollectionDashboardViewModel();

            // === 1. جلب البيانات الأساسية ===
            // هنجيب كل الكتب وكل الكولكشنز الخاصة بالمستخدم مرة واحدة
            var userBooks = await _context.Books
                                    .Where(b => b.UserId == userId)
                                    .AsNoTracking()
                                    .ToListAsync();

            var userCollections = await _context.Collections
                                        .Where(c => c.UserId == userId)
                                        .Include(c => c.Books) // ده بيعمل Join مع BookCollection
                                        .ThenInclude(bc => bc.Book) // وده بيعمل Join من BookCollection لـ Book
                                        .AsNoTracking()
                                        .ToListAsync();

            // === 2. حساب الـ KPIs (المستطيلات) ===
            viewModel.TotalUniqueBooks = userBooks.Count;
            viewModel.TotalCopies = userBooks.Sum(b => b.TotalCopies);

            // === 3. تجهيز بيانات الـ Charts ===

            // (البيانات دي هتكون فاضية لو مفيش كولكشنز)
            if (userCollections.Any())
            {
                // Pie Chart 1: (Collection vs Book Count)
                var pieChartData = userCollections
                    .Select(c => new { Id = c.ID, Label = c.Name, Value = (decimal)c.Books.Count() })
                    .OrderByDescending(x => x.Value);
                
                // convert int IDs to string because ChartDataViewModel.Ids is now List<string>
                viewModel.CollectionBookCountChart.Ids.AddRange(pieChartData.Select(x => x.Id.ToString()));
                viewModel.CollectionBookCountChart.Labels.AddRange(pieChartData.Select(x => x.Label));
                viewModel.CollectionBookCountChart.Data.AddRange(pieChartData.Select(x => x.Value));

                // Bar Chart 4: (Collection vs Total Copies)
                var copiesChartData = userCollections
                    .Select(c => new {
                        Id = c.ID,
                        Label = c.Name,
                        Value = (decimal)c.Books.Sum(bc => bc.Book.TotalCopies)
                    })
                    .OrderByDescending(x => x.Value);

                viewModel.CollectionCopiesChart.Ids.AddRange(copiesChartData.Select(x => x.Id.ToString())); 
                viewModel.CollectionCopiesChart.Labels.AddRange(copiesChartData.Select(x => x.Label));
                viewModel.CollectionCopiesChart.Data.AddRange(copiesChartData.Select(x => x.Value));

                // Bar Chart 5: (Collection vs Total Value)
                var valueChartData = userCollections
                    .Select(c => new {
                        Id = c.ID,
                        Label = c.Name,
                        Value = c.Books.Sum(bc => bc.Book.TotalCopies * (bc.Book.Price ?? 0))
                    })
                    .OrderByDescending(x => x.Value);
                
                viewModel.CollectionValueChart.Ids.AddRange(valueChartData.Select(x => x.Id.ToString()));
                viewModel.CollectionValueChart.Labels.AddRange(valueChartData.Select(x => x.Label));
                viewModel.CollectionValueChart.Data.AddRange(valueChartData.Select(x => x.Value));

                // --- (أعقد جزء) Column Charts الزمنية ---
                var allBookCollections = userCollections.SelectMany(c => c.Books);
                var currentYear = DateTime.Now.Year;

                // Column Chart 3a: (Monthly)
                viewModel.MonthlyAddedChart.Labels.AddRange(CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12)); // ["Jan", "Feb", ...]
                var monthlyGroups = allBookCollections
                    .Where(bc => bc.Book.DateAdded.Year == currentYear)
                    .GroupBy(bc => bc.Collection.Name)
                    .Select(g => new ChartSeries
                    {
                        Name = g.Key,
                        Data = Enumerable.Range(1, 12)
                                         .Select(month => g.Count(b => b.Book.DateAdded.Month == month))
                                         .ToList()
                    });
                viewModel.MonthlyAddedChart.Series.AddRange(monthlyGroups);

                // Column Chart 3b: (Yearly)
                var distinctYears = allBookCollections
                                    .Select(bc => bc.Book.DateAdded.Year)
                                    .Distinct()
                                    .OrderBy(y => y)
                                    .ToList();
                viewModel.YearlyAddedChart.Labels.AddRange(distinctYears.Select(y => y.ToString()));
                var yearlyGroups = allBookCollections
                    .GroupBy(bc => bc.Collection.Name)
                    .Select(g => new ChartSeries
                    {
                        Name = g.Key,
                        Data = distinctYears
                                 .Select(year => g.Count(b => b.Book.DateAdded.Year == year))
                                 .ToList()
                    });
                viewModel.YearlyAddedChart.Series.AddRange(yearlyGroups);
            }

            return View(viewModel);
        }
        // (التعديل) الدالة دي بقت Async وبترجع ViewModel متخصص
        // GET: Collections/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. هنجيب الكولكشن ده بكل الكتب اللي جواه
            var collection = await _context.Collections
                .Include(c => c.Books)
                    .ThenInclude(bc => bc.Book) // (Join -> Join)
                .Where(c => c.UserId == userId && c.ID == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (collection == null)
            {
                return NotFound();
            }

            // 2. هنجهز الـ ViewModel
            var viewModel = new CollectionDetailsViewModel
            {
                CollectionId = collection.ID,
                CollectionName = collection.Name,
                CollectionDescription = collection.Description
            };

            var booksInCollection = collection.Books.Select(bc => bc.Book).ToList();

            if (booksInCollection.Any())
            {
                // 3. حساب الـ KPIs
                viewModel.TotalUniqueBooks = booksInCollection.Count;
                viewModel.TotalCopies = booksInCollection.Sum(b => b.TotalCopies);

                // 4. حساب Pie Chart (Status)
                var statusGroups = booksInCollection
                    .GroupBy(b => b.Status ?? "No Status")
                    .Select(g => new { Label = g.Key, Value = (decimal)g.Count() })
                    .OrderByDescending(x => x.Value);

                viewModel.StatusPieChart.Labels.AddRange(statusGroups.Select(x => x.Label));
                viewModel.StatusPieChart.Data.AddRange(statusGroups.Select(x => x.Value));

                // 5. حساب Bar Chart (Top 10 Copies)
                var copiesGroups = booksInCollection
                    .OrderByDescending(b => b.TotalCopies)
                    .Take(10)
                    .Select(b => new { Label = b.Title, Value = (decimal)b.TotalCopies })
                    .OrderBy(x => x.Value); // (بنرتب تصاعدي عشان الرسمة تطلع من الأقصر للأطول)

                viewModel.CopiesBarChart.Labels.AddRange(copiesGroups.Select(x => x.Label));
                viewModel.CopiesBarChart.Data.AddRange(copiesGroups.Select(x => x.Value));

                // 6. حساب Bar Chart (Top 10 Price)
                var priceGroups = booksInCollection
                    .Where(b => b.Price.HasValue && b.Price > 0)
                    .OrderByDescending(b => b.Price.Value)
                    .Take(10)
                    .Select(b => new { Label = b.Title, Value = b.Price.Value })
                    .OrderBy(x => x.Value);

                viewModel.PriceBarChart.Labels.AddRange(priceGroups.Select(x => x.Label));
                viewModel.PriceBarChart.Data.AddRange(priceGroups.Select(x => x.Value));

                // 7. حساب Histogram (Rating)
                var ratingLabels = new List<string> { "5★", "4★", "3★", "2★", "1★", "0★", "N/A" };
                var ratingCounts = new List<decimal>
                {
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 4.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 3.5m && b.Rating.Value < 4.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 2.5m && b.Rating.Value < 3.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 1.5m && b.Rating.Value < 2.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 0.5m && b.Rating.Value < 1.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value < 0.5m), // (الـ 0 لوحده)
                    booksInCollection.Count(b => !b.Rating.HasValue) // (الـ N/A لوحده)
                };
                viewModel.RatingHistogram.Labels.AddRange(ratingLabels);
                viewModel.RatingHistogram.Data.AddRange(ratingCounts);

                // 8. حساب Timeline (Date Added)
                var dateGroups = booksInCollection
                    .GroupBy(b => b.DateAdded.ToString("yyyy-MM")) // (تجميع بالشهور)
                    .Select(g => new { Label = g.Key, Value = (decimal)g.Count() })
                    .OrderBy(x => x.Label); // (نرتب بالزمن)

                viewModel.DateAddedTimeline.Labels.AddRange(dateGroups.Select(x => x.Label));
                viewModel.DateAddedTimeline.Data.AddRange(dateGroups.Select(x => x.Value));
            }
            return View(viewModel);
        }
        // GET: Collections/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Collections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Collection collection)
        {
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"Current logged-in userId: {userId}");
            collection.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(collection);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(collection);
        }

        // GET: Collections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var collection = await _context.Collections.FirstOrDefaultAsync(c => c.ID == id && c.UserId == userId);
            if (collection == null)
            {
                return NotFound();
            }
            return View(collection);
        }

        // POST: Collections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Collection collection)
        {
            if (id != collection.ID)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            collection.UserId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(collection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CollectionExists(collection.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(collection);
        }

        // GET: Collections/Delete/5 (Replaced by a Modal)
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    var collection = await _context.Collections
        //        .Include(c => c.User)
        //        .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);
        //    if (collection == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(collection);
        //}

        // POST: Collections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var collection = await _context.Collections.Include(c => c.Books).FirstOrDefaultAsync(c => c.ID == id && c.UserId == userId);
            if (collection != null)
            {
                _context.BookCollections.RemoveRange(collection.Books);
                _context.Collections.Remove(collection);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CollectionExists(int id)
        {
            return _context.Collections.Any(e => e.ID == id);
        }

        // يرجّع Collections الخاصة باليوزر كـ JSON
        [HttpGet]
        public async Task<IActionResult> GetUserCollections()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cols = await _context.Collections
                .Where(c => c.UserId == userId)
                .Select(c => new { id = c.ID, name = c.Name })
                .ToListAsync();

            return Json(cols);
        }

    }
}
