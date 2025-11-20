using Biblio.Data;
using Biblio.Models; 
using Biblio.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Biblio.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var userId = user.Id;
            var paymentprice = 0;
            var PaymentPlan = user.PayingPlanType;
            if (PaymentPlan==PayingPlanType.Monthly) {paymentprice = 29;}
            else if(PaymentPlan == PayingPlanType.Yearly) {paymentprice = 278;}
            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                ProfilePicture = null,
                PlanType = user.PlanType.ToString(),
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PaymentPlan = user.PayingPlanType.ToString(),
                NextPayment = user.NextPaymentDate?.ToString("yyyy-MM-dd"),
                PaymentPrice = paymentprice
            };
            if(user.PlanType == PlanType.Free)
            {
                vm.PaymentPlan = "N/A";
                vm.NextPayment = "N/A";
                vm.PaymentPrice = 0;
            }
            var userBooks = await _context.Books
                        .Where(b => b.UserId == userId)
                        .AsNoTracking()
                        .ToListAsync();

            var userCollections = await _context.Collections
                                        .Where(c => c.UserId == userId)
                                        .AsNoTracking()
                                        .ToListAsync();
            var allUserBorrowings = await _context.Borrowings
                .Include(b => b.Book)
                .Include(b => b.Visitor)
                .Where(b => b.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var activeBorrowings = allUserBorrowings
                .Where(b => b.Status == BorrowingStatus.Borrowed || b.Status == BorrowingStatus.Overdue)
                .OrderBy(b => b.DueDate)
                .ToList();

            // === 2. حساب الـ KPIs (المستطيلات) ===

            ViewBag.UserRole = roles;
            ViewBag.TotalBooks = userBooks.Count;
            ViewBag.TotalCollections = userCollections.Count;
            ViewBag.ActiveBorrowings = activeBorrowings.Count;
            return View(vm);
        }
    }
}
