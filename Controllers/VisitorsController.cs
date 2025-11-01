// EDIT: Enforce ownership, removed Bind attributes and auto-assign UserId in Create/Edit POST (by dev tool)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Biblio.Data;
using Biblio.Models;

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

            var visitors = await _context.Visitors
                .Where(v => v.UserId == userId)
                .ToListAsync();

            return View(visitors);
        }

        // GET: Visitors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var visitor = await _context.Visitors
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);
            if (visitor == null)
            {
                return NotFound();
            }

            return View(visitor);
        }

        // GET: Visitors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Visitors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Visitor visitor)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                visitor.UserId = userId;
                _context.Add(visitor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Debugging section
            var errors = ModelState.Values.SelectMany(v => v.Errors);
            Console.WriteLine(string.Join(" | ", errors.Select(e => e.ErrorMessage)));

            return View(visitor);
        }


        // GET: Visitors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
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

        // GET: Visitors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var visitor = await _context.Visitors
                .Include(v => v.User)
                .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);
            if (visitor == null)
            {
                return NotFound();
            }

            return View(visitor);
        }

        // POST: Visitors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var visitor = await _context.Visitors.Include(v => v.Notifications).Include(v => v.Borrowings).FirstOrDefaultAsync(v => v.ID == id && v.UserId == userId);
            if (visitor != null)
            {
                _context.Notifications.RemoveRange(visitor.Notifications);
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
