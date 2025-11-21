using Biblio.Data;
using Biblio.Models; 
using Biblio.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace Biblio.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailSender _emailSender; // خدمة الإيميل
        public AccountController(AppDbContext context, UserManager<AppUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
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
                PaymentPrice = paymentprice,
                StatusMessage = TempData["StatusMessage"] as string
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
        // أكشن إرسال رابط التفعيل
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendVerificationEmail()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = Url.Action(
                action: "ConfirmEmail",
                controller: "Account",
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            TempData["StatusMessage"] = "Verification email sent. Please check your inbox.";
            return RedirectToAction("Profile");
        }

        // صفحة استقبال التفعيل (مسموحة للجميع)
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null) return RedirectToAction("Index", "Home");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound($"Unable to load user with ID '{userId}'.");

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                return View("ConfirmEmail");
            }
            else
            {
                return View("Error");
            }
        }
    }
}
