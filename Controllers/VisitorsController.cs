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
using Biblio.Models.ViewModels;

namespace Biblio.Controllers
{
    public class VisitorsController : Controller
    {
        private readonly AppDbContext _context;

        public VisitorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Visitors
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. هنجيب كل الزوار، ومعاهم الاستعارات بتاعتهم
            var visitorsQuery = _context.Visitors
                .Include(v => v.Borrowings) // (مهم عشان نحسب الغرامات)
                .Where(v => v.UserId == userId);

            // 2. (الخطوة الأهم) هنحول كل Visitor لـ ViewModel
            // وهنحسب الغرامة لكل واحد
            var viewModelList = await visitorsQuery
                .Select(v => new VisitorIndexViewModel
                {
                    Visitor = v,
                    // (اللوجيك بتاعك) بنجمع غرامات الاستعارات اللي حالتها "Overdue" بس
                    TotalFines = v.Borrowings
                                .Where(b => b.Status == BorrowingStatus.Overdue)
                                .Sum(b => b.FineAmount)
                })
                .AsNoTracking()
                .ToListAsync();

            return View(viewModelList);
        }

        // (تم تعديل الدالة دي بالكامل)
        // GET: Visitors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. هنجيب الزائر + كل استعاراته + الكتب المرتبطة بالاستعارات دي
            var visitor = await _context.Visitors
                .Include(v => v.Borrowings)
                    .ThenInclude(b => b.Book) // (مهم جداً عشان اسم وصورة الكتاب)
                .Where(v => v.UserId == userId && v.ID == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (visitor == null)
            {
                return NotFound();
            }

            // 2. هنجهز الـ ViewModel
            var viewModel = new VisitorDetailsViewModel
            {
                Visitor = visitor,
                Borrowings = visitor.Borrowings.OrderByDescending(b => b.BorrowDate).ToList(),
                TotalBorrowings = visitor.Borrowings.Count,
                TotalFines = visitor.Borrowings
                                .Where(b => b.Status == BorrowingStatus.Overdue)
                                .Sum(b => b.FineAmount)
            };

            // 3. حساب بيانات الـ Pie Chart
            var statusGroups = visitor.Borrowings
                .GroupBy(b => b.Status)
                .Select(g => new {
                    Label = g.Key.ToString(),
                    Value = (decimal)g.Count()
                })
                .OrderByDescending(x => x.Value);

            viewModel.StatusPieChart.Labels.AddRange(statusGroups.Select(x => x.Label));
            viewModel.StatusPieChart.Data.AddRange(statusGroups.Select(x => x.Value));

            return View(viewModel);
        }        // GET: Visitors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Visitors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Visitor visitor)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            visitor.UserId = userId;
            if (ModelState.IsValid)
            {
                _context.Add(visitor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(visitor);
        }

        // GET: Visitors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // ... (الدالة دي زي ما هي) ...
            if (id == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.ID == id && v.UserId == userId);
            if (visitor == null)
            {
                return NotFound();
            }
            return View(visitor);
        }

        // POST: Visitors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Visitor visitor)
        {
            // ... (الدالة دي زي ما هي) ...
            if (id != visitor.ID)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            visitor.UserId = userId;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(visitor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VisitorExists(visitor.ID))
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
            return View(visitor);
        }

        // GET: Visitors/Delete/5 (Replaced by a Modal)
        // POST: Visitors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // ... (الدالة دي زي ما هي) ...
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var visitor = await _context.Visitors.Include(v => v.Borrowings).FirstOrDefaultAsync(v => v.ID == id && v.UserId == userId);
            if (visitor != null)
            {
                _context.Borrowings.RemoveRange(visitor.Borrowings);
                _context.Visitors.Remove(visitor);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VisitorExists(int id)
        {
            return _context.Visitors.Any(e => e.ID == id);
        }
    }
}