using Biblio.Data;
using Biblio.Models;
using Biblio.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace Biblio.Controllers
{
    // (1) تأمين الكنترولر بالكامل للأدمن فقط
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, UserManager<AppUser> userManager, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }


        // --- (أ) صفحة إدارة المستخدمين ---
        // GET: /Admin/ManageUsers
        public async Task<IActionResult> ManageUsers()
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            string adminRoleId = adminRole?.Id ?? "";

            // (هنجيب كل اليوزرز اللي مش أدمن)
            var users = await _context.Users
                .Where(u => !_context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId))
                .ToListAsync();

            var viewModels = new List<UserManageViewModel>();
            foreach (var user in users)
            {
                viewModels.Add(new UserManageViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    PlanType = user.PlanType,
                    Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "N/A",
                    AvailableRoles = new SelectList(new[] { "Librarian", "Reader" })
                });
            }
            return View(viewModels); // (لـ Views/Admin/ManageUsers.cshtml)
        }

        // GET: /Admin/EditUser/{id}
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("EditUser GET rejected for id {UserId}. user==null: {IsNull}, isAdmin: {IsAdmin}", id, user == null, user != null && _userManager.IsInRoleAsync(user, "Admin").Result);
                return NotFound(); // (ممنوع تعدل الأدمن)
            }

            var viewModel = new UserManageViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PlanType = user.PlanType,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "N/A",
                // (هنجيب قايمة الرتب المتاحة)
                AvailableRoles = new SelectList(new[] { "Librarian", "Reader" })
            };
            return View(viewModel); // (لـ Views/Admin/EditUser.cshtml)
        }

        // POST: /Admin/EditUser/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(UserManageViewModel model)
        {
            // Basic null checks
            if (model == null || string.IsNullOrEmpty(model.Id))
            {
                _logger.LogWarning("EditUser POST received invalid model or missing Id. Model null: {IsNull}, Id: {Id}", model == null, model?.Id);
                ModelState.AddModelError(string.Empty, "Invalid request data.");
                model ??= new UserManageViewModel();
                model.AvailableRoles = new SelectList(new[] { "Librarian", "Reader" }, model?.Role);
                return View(model);
            }

            // Robust PlanType parsing (accept numeric or name)
            if (Request.HasFormContentType && Request.Form.TryGetValue("PlanType", out var planVals))
            {
                var raw = planVals.FirstOrDefault();
                if (!string.IsNullOrEmpty(raw))
                {
                    if (int.TryParse(raw, out var intVal))
                    {
                        if (Enum.IsDefined(typeof(PlanType), intVal))
                        {
                            model.PlanType = (PlanType)intVal;
                        }
                    }
                    else if (Enum.TryParse<PlanType>(raw, true, out var enumVal))
                    {
                        model.PlanType = enumVal;
                    }

                    ModelState.Remove(nameof(model.PlanType));
                }
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null || await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogWarning("EditUser POST: target user not found or is Admin. Id: {Id}, userNull: {UserNull}", model.Id, user == null);
                return NotFound(); // Prevent editing Admins or non-existing users
            }

            // If model binding failed, re-show form with dropdown populated
            if (!ModelState.IsValid)
            {
                // Log modelstate errors to help debugging
                foreach (var kvp in ModelState)
                {
                    var key = kvp.Key;
                    var errors = kvp.Value.Errors;
                    if (errors != null && errors.Count > 0)
                    {
                        foreach (var err in errors)
                        {
                            _logger.LogWarning("ModelState error on key '{Key}': {ErrorMessage}", key, err.ErrorMessage);
                        }
                    }
                }

                model.AvailableRoles = new SelectList(new[] { "Librarian", "Reader" }, model.Role);
                return View(model);
            }

            // Update PlanType on user
            user.PlanType = model.PlanType;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                    _logger.LogWarning("UserManager.UpdateAsync failed for user {UserId}: {Error}", model.Id, err.Description);
                }

                model.AvailableRoles = new SelectList(new[] { "Librarian", "Reader" }, model.Role);
                return View(model);
            }

            // Update Roles: remove existing then add new if valid
            var oldRoles = await _userManager.GetRolesAsync(user);
            if (oldRoles != null && oldRoles.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, oldRoles);
                if (!removeResult.Succeeded)
                {
                    foreach (var err in removeResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, err.Description);
                        _logger.LogWarning("RemoveFromRolesAsync failed for user {UserId}: {Error}", model.Id, err.Description);
                    }

                    model.AvailableRoles = new SelectList(new[] { "Librarian", "Reader" }, model.Role);
                    return View(model);
                }
            }

            if (!string.IsNullOrEmpty(model.Role) && (model.Role == "Librarian" || model.Role == "Reader"))
            {
                var addResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!addResult.Succeeded)
                {
                    foreach (var err in addResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, err.Description);
                        _logger.LogWarning("AddToRoleAsync failed for user {UserId}, role {Role}: {Error}", model.Id, model.Role, err.Description);
                    }

                    model.AvailableRoles = new SelectList(new[] { "Librarian", "Reader" }, model.Role);
                    return View(model);
                }
            }

            _logger.LogInformation("EditUser POST succeeded for user {UserId}. PlanType: {PlanType}, Role: {Role}", model.Id, model.PlanType, model.Role);
            return RedirectToAction(nameof(ManageUsers));
        }

        // --- (ب) صفحة إرسال الإشعارات ---
        // GET: /Admin/SendNotification
        public async Task<IActionResult> SendNotification()
        {
            // (هنجيب قايمة اليوزرز عشان الـ Dropdown)
            ViewBag.UserList = new SelectList(await _userManager.Users.ToListAsync(), "Id", "UserName");
            return View(new Notification()); // (هيبعت موديل فاضي عشان الفورم)
        }

        // POST: /Admin/SendNotification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(
            string message, string linkUrl,
            NotificationType type, string target, string specificUserId)
        {
            if (string.IsNullOrEmpty(message))
            {
                TempData["Error"] = "Message cannot be empty.";
                return RedirectToAction(nameof(SendNotification));
            }

            var notifications = new List<Notification>();

            if (target == "Specific")
            {
                // 1. (إشعار محدد)
                notifications.Add(new Notification
                {
                    Message = message,
                    LinkUrl = linkUrl,
                    Type = type,
                    UserId = specificUserId,
                    Date = DateTime.Now
                });
            }
            else
            {
                // 2. (إعلان)
                List<AppUser> targetUsers;
                if (target == "Pro")
                {
                    // (برو = Librarian + Plan Library)
                    var proRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Librarian");
                    targetUsers = await _context.Users
                        .Where(u => u.PlanType == PlanType.Library &&
                                    _context.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == proRole.Id))
                        .ToListAsync();
                }
                else if (target == "Reader")
                {
                    targetUsers = (await _userManager.GetUsersInRoleAsync("Reader")).ToList();
                }
                else // (All)
                {
                    targetUsers = await _userManager.Users.ToListAsync();
                }

                foreach (var user in targetUsers)
                {
                    notifications.Add(new Notification
                    {
                        Message = message,
                        LinkUrl = linkUrl,
                        Type = type,
                        UserId = user.Id,
                        Date = DateTime.Now
                    });
                }
            }

            await _context.Notifications.AddRangeAsync(notifications);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Notification sent to {notifications.Count} user(s).";
            return RedirectToAction(nameof(SendNotification));
        }

        // --- (ج) الداشبورد العام (الرئيسي) ---
        // GET: /Admin/Index
        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel();
            var minDate = new DateTime(2000, 1, 1);

            // 1. KPIs
            viewModel.TotalUsers = await _context.Users.CountAsync();
            viewModel.TotalBooks = await _context.Books.CountAsync();
            viewModel.TotalCollections = await _context.Collections.CountAsync();

            // 2. User Charts
            var usersWithData = await _context.Users
                .Include(u => u.Collections)
                .Include(u => u.Books)
                .AsNoTracking()
                .ToListAsync();

            var pieData = usersWithData
                .Select(u => new { Id = u.Id, Label = u.UserName, Value = (decimal)u.Collections.Count })
                .Where(x => x.Value > 0)
                .OrderByDescending(x => x.Value);

            // Ids are strings now — preserve Identity user Id as-is
            viewModel.UserCollectionChart.Ids.AddRange(pieData.Select(x => x.Id));
            viewModel.UserCollectionChart.Labels.AddRange(pieData.Select(x => x.Label));
            viewModel.UserCollectionChart.Data.AddRange(pieData.Select(x => x.Value));

            var barData = usersWithData
                .Select(u => new { Label = u.UserName, Value = (decimal)u.Books.Count })
                .OrderByDescending(x => x.Value);

            viewModel.UserBookCountChart.Labels.AddRange(barData.Select(x => x.Label));
            viewModel.UserBookCountChart.Data.AddRange(barData.Select(x => x.Value));

            // 3. (جديد) Platform Timeline Charts
            // Books timeline: group by Year & Month (EF Core translatable)
            var booksTimelineRaw = await _context.Books
                .Where(b => b.DateAdded > minDate)
                .GroupBy(b => new { b.DateAdded.Year, b.DateAdded.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var booksTimeline = booksTimelineRaw
                .Select(x => new { Label = $"{x.Year}-{x.Month:D2}", Value = (decimal)x.Count })
                .ToList();

            viewModel.PlatformBooksTimeline.Labels.AddRange(booksTimeline.Select(x => x.Label));
            viewModel.PlatformBooksTimeline.Data.AddRange(booksTimeline.Select(x => x.Value));

            // Collections timeline: compute earliest DateAdded per collection, then group by Year & Month
            var collectionsDates = await _context.Collections
                .Where(c => c.Books.Any(b => b.Book.DateAdded > minDate))
                .Select(c => new
                {
                    Year = c.Books.Min(b => b.Book.DateAdded).Year,
                    Month = c.Books.Min(b => b.Book.DateAdded).Month
                })
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var collectionsTimeline = collectionsDates
                .Select(x => new { Label = $"{x.Year}-{x.Month:D2}", Value = (decimal)x.Count })
                .ToList();

            viewModel.PlatformCollectionsTimeline.Labels.AddRange(collectionsTimeline.Select(x => x.Label));
            viewModel.PlatformCollectionsTimeline.Data.AddRange(collectionsTimeline.Select(x => x.Value));

            return View(viewModel); // (لـ Views/Admin/Index.cshtml)
        }

        // --- (د) صفحات الـ Drill-Down ---
        // GET: /Admin/ViewUserDashboard/{id}
        public async Task<IActionResult> ViewUserDashboard(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var viewModel = new CollectionDashboardViewModel();

            var userBooks = await _context.Books.Where(b => b.UserId == id).AsNoTracking().ToListAsync();
            var userCollections = await _context.Collections
                .Where(c => c.UserId == id)
                .Include(c => c.Books).ThenInclude(bc => bc.Book)
                .AsNoTracking()
                .ToListAsync();

            viewModel.TotalUniqueBooks = userBooks.Count;
            viewModel.TotalCopies = userBooks.Sum(b => b.TotalCopies);

            if (userCollections.Any())
            {
                var pieChartData = userCollections
                    .Select(c => new { Id = c.ID, Label = c.Name, Value = (decimal)c.Books.Count() })
                    .OrderByDescending(x => x.Value);
                // collection IDs are ints -> convert to string
                viewModel.CollectionBookCountChart.Ids.AddRange(pieChartData.Select(x => x.Id.ToString()));
                viewModel.CollectionBookCountChart.Labels.AddRange(pieChartData.Select(x => x.Label));
                viewModel.CollectionBookCountChart.Data.AddRange(pieChartData.Select(x => x.Value));

                var copiesChartData = userCollections
                    .Select(c => new { Id = c.ID, Label = c.Name, Value = (decimal)c.Books.Sum(bc => bc.Book.TotalCopies) })
                    .OrderByDescending(x => x.Value);
                viewModel.CollectionCopiesChart.Ids.AddRange(copiesChartData.Select(x => x.Id.ToString()));
                viewModel.CollectionCopiesChart.Labels.AddRange(copiesChartData.Select(x => x.Label));
                viewModel.CollectionCopiesChart.Data.AddRange(copiesChartData.Select(x => x.Value));

                var valueChartData = userCollections
                    .Select(c => new { Id = c.ID, Label = c.Name, Value = c.Books.Sum(bc => bc.Book.TotalCopies * (bc.Book.Price ?? 0)) })
                    .OrderByDescending(x => x.Value);
                viewModel.CollectionValueChart.Ids.AddRange(valueChartData.Select(x => x.Id.ToString()));
                viewModel.CollectionValueChart.Labels.AddRange(valueChartData.Select(x => x.Label));
                viewModel.CollectionValueChart.Data.AddRange(valueChartData.Select(x => x.Value));

                var allBookCollections = userCollections.SelectMany(c => c.Books);
                var currentYear = DateTime.Now.Year;

                viewModel.MonthlyAddedChart.Labels.AddRange(CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames.Take(12));
                var monthlyGroups = allBookCollections
                    .Where(bc => bc.Book.DateAdded.Year == currentYear)
                    .GroupBy(bc => bc.Collection.Name)
                    .Select(g => new ChartSeries
                    {
                        Name = g.Key,
                        Data = Enumerable.Range(1, 12).Select(month => g.Count(b => b.Book.DateAdded.Month == month)).ToList()
                    });
                viewModel.MonthlyAddedChart.Series.AddRange(monthlyGroups);

                var distinctYears = allBookCollections.Select(bc => bc.Book.DateAdded.Year).Distinct().OrderBy(y => y).ToList();
                viewModel.YearlyAddedChart.Labels.AddRange(distinctYears.Select(y => y.ToString()));
                var yearlyGroups = allBookCollections
                    .GroupBy(bc => bc.Collection.Name)
                    .Select(g => new ChartSeries
                    {
                        Name = g.Key,
                        Data = distinctYears.Select(year => g.Count(b => b.Book.DateAdded.Year == year)).ToList()
                    });
                viewModel.YearlyAddedChart.Series.AddRange(yearlyGroups);
            }

            ViewData["TargetUserName"] = user.UserName;
            return View(viewModel); // (لـ Views/Admin/ViewUserDashboard.cshtml)
        }

        // GET: /Admin/ViewCollectionDetails/{id}
        public async Task<IActionResult> ViewCollectionDetails(int id)
        {
            var collection = await _context.Collections
                .Include(c => c.Books).ThenInclude(bc => bc.Book)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ID == id);

            if (collection == null) return NotFound();

            var viewModel = new CollectionDetailsViewModel
            {
                CollectionId = collection.ID,
                CollectionName = collection.Name,
                CollectionDescription = collection.Description
            };

            var booksInCollection = collection.Books.Select(bc => bc.Book).ToList();

            if (booksInCollection.Any())
            {
                viewModel.TotalUniqueBooks = booksInCollection.Count;
                viewModel.TotalCopies = booksInCollection.Sum(b => b.TotalCopies);

                var statusGroups = booksInCollection.GroupBy(b => b.Status ?? "No Status").Select(g => new { Label = g.Key, Value = (decimal)g.Count() }).OrderByDescending(x => x.Value);
                viewModel.StatusPieChart.Labels.AddRange(statusGroups.Select(x => x.Label));
                viewModel.StatusPieChart.Data.AddRange(statusGroups.Select(x => x.Value));

                var copiesGroups = booksInCollection.OrderByDescending(b => b.TotalCopies).Take(10).Select(b => new { Label = b.Title, Value = (decimal)b.TotalCopies }).OrderBy(x => x.Value);
                viewModel.CopiesBarChart.Labels.AddRange(copiesGroups.Select(x => x.Label));
                viewModel.CopiesBarChart.Data.AddRange(copiesGroups.Select(x => x.Value));

                var priceGroups = booksInCollection.Where(b => b.Price.HasValue && b.Price > 0).OrderByDescending(b => b.Price.Value).Take(10).Select(b => new { Label = b.Title, Value = b.Price.Value }).OrderBy(x => x.Value);
                viewModel.PriceBarChart.Labels.AddRange(priceGroups.Select(x => x.Label));
                viewModel.PriceBarChart.Data.AddRange(priceGroups.Select(x => x.Value));

                var ratingLabels = new List<string> { "5★", "4★", "3★", "2★", "1★", "0★", "N/A" };
                var ratingCounts = new List<decimal>
                {
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 4.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 3.5m && b.Rating.Value < 4.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 2.5m && b.Rating.Value < 3.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 1.5m && b.Rating.Value < 2.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value >= 0.5m && b.Rating.Value < 1.5m),
                    booksInCollection.Count(b => b.Rating.HasValue && b.Rating.Value < 0.5m),
                    booksInCollection.Count(b => !b.Rating.HasValue)
                };
                viewModel.RatingHistogram.Labels.AddRange(ratingLabels);
                viewModel.RatingHistogram.Data.AddRange(ratingCounts);

                var minValidDate = new DateTime(2000, 1, 1);
                var validDateBooks = booksInCollection.Where(b => b.DateAdded > minValidDate).ToList();
                if (!validDateBooks.Any())
                {
                    var oldDateGroups = booksInCollection.GroupBy(b => b.DateAdded.ToString("yyyy-MM")).Select(g => new { Label = g.Key, Value = (decimal)g.Count() }).OrderBy(x => x.Label);
                    viewModel.DateAddedTimeline.Labels.AddRange(oldDateGroups.Select(x => x.Label));
                    viewModel.DateAddedTimeline.Data.AddRange(oldDateGroups.Select(x => x.Value));
                }
                else
                {
                    var dateGroups = validDateBooks.GroupBy(b => b.DateAdded.ToString("yyyy-MM")).Select(g => new { Label = g.Key, Value = (decimal)g.Count() }).OrderBy(x => x.Label);
                    viewModel.DateAddedTimeline.Labels.AddRange(dateGroups.Select(x => x.Label));
                    viewModel.DateAddedTimeline.Data.AddRange(dateGroups.Select(x => x.Value));
                }
            }

            return View(viewModel); // (لـ Views/Admin/ViewCollectionDetails.cshtml)
        }
    }
}