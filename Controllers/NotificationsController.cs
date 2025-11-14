// EDIT: Enforce ownership, removed Bind attributes and auto-assign UserId in Create/Edit POST (by dev tool)
using System;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
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
    public class NotificationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public NotificationsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // (جديد) دالة للـ AJAX عشان تجيب عدد الإشعارات غير المقروءة
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var count = await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
                .CountAsync();

            return Json(new { count = count });
        }
        // (تعديل 1) دالة Index مبقتش بتمسح الإشعارات
        // GET: Notifications
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.Visitor)
                .Include(n => n.Book)
                .Include(n => n.Borrowing) // (بنحتاجه عشان اللينك)
                .OrderByDescending(n => n.Date)
                .AsNoTracking()
                .ToListAsync();

            // (هنحسب العدد ونبعته عشان زرار "Mark all")
            ViewData["UnreadCount"] = notifications.Count(n => n.Status == NotificationStatus.Unread);

            return View(notifications);
        }

        // (جديد 2) الدالة دي بتقرا الإشعار وتوديك للينك بتاعه
        [HttpGet]
        public async Task<IActionResult> GoTo(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.ID == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound();
            }

            // (1) علّم إنه اتقرا
            notification.Status = NotificationStatus.Read;
            await _context.SaveChangesAsync();

            // (2) وديه للينك (لو مفيش لينك، رجعه لصفحة الإشعارات)
            if (string.IsNullOrEmpty(notification.LinkUrl))
            {
                return RedirectToAction(nameof(Index));
            }

            return Redirect(notification.LinkUrl);
        }

        // (جديد 3) الدالة دي بتمسح كل الإشعارات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.Status == NotificationStatus.Unread)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.Status = NotificationStatus.Read;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: Notifications/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notification = await _context.Notifications
                .Include(n => n.User)
                .Include(n => n.Visitor)
                .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // GET: Notifications/Create
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            var VisitorSelectListItems = _context.Visitors
                .Where(v => v.UserId == userId)
                .Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name }).ToList();
            ViewBag.VisitorSelectList = VisitorSelectListItems;
            return View();
        }

        // POST: Notifications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Notification notification)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            notification.UserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(notification);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["VisitorID"] = new SelectList(_context.Visitors, "ID", "ID", notification.VisitorID);
            return View(notification);
        }

        // GET: Notifications/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.ID == id && n.UserId == userId);
            if (notification == null)
            {
                return NotFound();
            }
            var VisitorSelectListItems = _context.Visitors
                .Where(v => v.UserId == userId)
                .Select(v => new SelectListItem { Value = v.ID.ToString(), Text = v.Name, Selected = (v.ID == notification.VisitorID) }).ToList();
            ViewBag.VisitorSelectList = VisitorSelectListItems;
            return View(notification);
        }

        // POST: Notifications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Notification notification)
        {
            if (id != notification.ID)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            notification.UserId = userId;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NotificationExists(notification.ID))
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
            ViewData["VisitorID"] = new SelectList(_context.Visitors, "ID", "ID", notification.VisitorID);
            return View(notification);
        }

        // GET: Notifications/Delete/5 (Replaced by a Modal)
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    var notification = await _context.Notifications
        //        .Include(n => n.User)
        //        .Include(n => n.Visitor)
        //        .FirstOrDefaultAsync(m => m.ID == id && m.UserId == userId);
        //    if (notification == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(notification);
        //}

        // POST: Notifications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.ID == id && n.UserId == userId);
            if (notification != null)
            {
                _context.Notifications.Remove(notification);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NotificationExists(int id)
        {
            return _context.Notifications.Any(e => e.ID == id);
        }
    }
}
