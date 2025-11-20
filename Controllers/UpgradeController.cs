using Biblio.Data;
using Biblio.Models;
using Biblio.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Biblio.Controllers
{
    [Authorize]
    public class UpgradeController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public UpgradeController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, AppDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        // Helper method to create and save a Notification
        private async Task CreateUserSystemNotification(AppUser user, NotificationType type, string message, string linkUrl)
        {
            var notification = new Notification
            {
                UserId = user.Id,
                Type = type,
                Message = message,
                LinkUrl = linkUrl,
                Status = NotificationStatus.Unread,
                Date = DateTime.Now
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
        // الخطوة 1: إنشاء جلسة الدفع وتوجيه المستخدم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(string payingPlanType)
        {
            if (string.IsNullOrEmpty(payingPlanType))
            {
                TempData["Error"] = "Paying Plan type is missing.";
                return RedirectToAction(nameof(Index));
            }
            var isYearly = payingPlanType == "Yearly";
            var price = isYearly ? 278 : 29;
            var amountInCents = 100 * price;
            string priceName = isYearly ? "Library Pro (Yearly)" : " Library Pro (Monthly)";

            var domain = "https://" + Request.Host; // الحصول على رابط الموقع الحالي

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = amountInCents,
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = priceName,
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment", // أو "subscription" لو عندك خطط جاهزة على Stripe
                // عند النجاح، أرسل المستخدم للـ Action الجديد مع نوع الخطة
                SuccessUrl = domain + $"/Upgrade/PaymentSuccess?payingPlanType={payingPlanType}",
                CancelUrl = domain + "/Upgrade/Index",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            // توجيه المستخدم لصفحة الدفع الخاصة بـ Stripe
            return Redirect(session.Url);
        }
        // الخطوة 2: استقبال المستخدم بعد الدفع وتنفيذ الترقية
        [HttpGet] // Stripe بيرجع المستخدم بـ GET Request
        public async Task<IActionResult> PaymentSuccess(string payingPlanType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Index", "Upgrade");
            }

            // 2. تحديث الخطة والأدوار
            user.PlanType = PlanType.Library;
            user.PayingPlanType = payingPlanType.Equals("Monthly", StringComparison.OrdinalIgnoreCase) ? PayingPlanType.Monthly : PayingPlanType.Yearly;
            user.LastPaymentDate = DateTime.Now;
            user.NextPaymentDate = DateTime.UtcNow.AddMonths(payingPlanType.Equals("Monthly", StringComparison.OrdinalIgnoreCase) ? 1 : 12);
            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                var newRole = "Librarian";
                var oldRoles = await _userManager.GetRolesAsync(user);
                var rolesToRemove = oldRoles.Where(r => r != "Admin").ToList();

                await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                await _userManager.AddToRoleAsync(user, newRole);
            }
            // 3. تحديث الجلسة فوراً
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: true);

            // 4. إرسال Notification
            await CreateUserSystemNotification(user, NotificationType.Information,
                "Your subscription has been upgraded to Library Pro! You now have full access to all features.",
                "/Account/Profile");

            return RedirectToAction("Profile", "Account");
        }

        // POST: /Upgrade/Downgrade (محدث)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Downgrade()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            user.PlanType = PlanType.Free;
            user.PayingPlanType = null;
            user.LastPaymentDate = null;
            user.NextPaymentDate = null;
            await _userManager.UpdateAsync(user);

            var newRole = "Reader";
            var oldRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = oldRoles.Where(r => r != "Admin").ToList();

            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            await _userManager.AddToRoleAsync(user, newRole);

            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: true);

            // إرسال Notification
            await CreateUserSystemNotification(user, NotificationType.Warning,
                "Your plan has been successfully downgraded to Free. You will lose access to Pro features.",
                "/Account/Profile");

            return RedirectToAction("Profile", "Account");
            //return Json(new { success = true, message = "Downgrade complete. Permissions updated." });
        }

        // GET: /Upgrade/GetUpgradeModalData (جديد)
        [HttpGet]
        public IActionResult GetUpgradeModalData(string payingPlanType)
        {
            // حساب الأسعار يدوياً (ممكن وضعها في خدمة أو دالة مساعدة)
            var isYearly = payingPlanType == "Yearly";
            var price = isYearly ? 278m : 29m;
            var period = isYearly ? " / Year" : " / Month";
            var billingText = isYearly ? $"Billed {price:C0} per year (Save 20%)" : $"Billed {price:C0} per month";

            return PartialView("_UpgradeConfirmationModal", new UpgradeModalViewModel
            {
                PayingPlanType = isYearly ? "Yearly" : "Monthly",
                Price = price,
                Period = period,
                BillingText = billingText,
                ActionUrl = Url.Action(nameof(CreateCheckoutSession), "Upgrade")
            });
        }

        // GET: /Upgrade/GetDowngradeModal (جديد)
        [HttpGet]
        public IActionResult GetDowngradeModal()
        {
            return PartialView("_DowngradeConfirmationModal", new DowngradeModalViewModel
            {
                ActionUrl = Url.Action(nameof(Downgrade), "Upgrade")
            });
        }
    }
}