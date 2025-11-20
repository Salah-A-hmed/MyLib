using Biblio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Biblio.Controllers
{
    [Authorize]
    public class UpgradeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public UpgradeController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(string payingPlanType)
        {
            // هذا مجرد نموذج. في التطبيق الحقيقي، هنا تتم معالجة الدفع
            if (string.IsNullOrEmpty(payingPlanType))
            {
                TempData["Error"] = "paying Plan type is missing.";
                return RedirectToAction(nameof(Index));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // 1. تحديث الخطة في الداتابيز
            user.PlanType = PlanType.Library; // الترقية إلى خطة المكتبة
            user.PayingPlanType = payingPlanType.Equals("Monthly", StringComparison.OrdinalIgnoreCase) ? PayingPlanType.Monthly : PayingPlanType.Yearly;
            user.LastPaymentDate = DateTime.Now;
            user.NextPaymentDate = DateTime.UtcNow.AddMonths(payingPlanType.Equals("Monthly", StringComparison.OrdinalIgnoreCase) ? 1 : 12);
            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                // 2. حذف الأدوار القديمة ثم إضافة الدور الجديد (Librarian)
                var newRole = "Librarian";

                // البحث عن الأدوار القديمة وحذفها (اللوجيك المطلوب)
                var oldRoles = await _userManager.GetRolesAsync(user);
                if (oldRoles != null && oldRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, oldRoles.ToList());
                }

                // إضافة الدور الجديد
                await _userManager.AddToRoleAsync(user, newRole);

                // 3. تحديث الجلسة (Claims Refresh)
                // تسجيل الخروج ثم تسجيل الدخول مرة أخرى لضمان تحديث الأدوار فوراً
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: true); // استخدم isPersistent: true للحفاظ على الجلسة

                TempData["Success"] = $"Congratulations! You are now a {newRole} member. Your permissions have been updated.";
            }
            else
            {
                TempData["Error"] = "Failed to update user profile.";
            }

            // التوجيه إلى صفحة الإشعارات أو الرئيسية
            return RedirectToAction("Profile", "Account");
        }
        // POST: /Upgrade/Downgrade (Action العودة للخطة المجانية)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Downgrade()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction("Index", "Home");
            }

            // 1. تحديث الخطة إلى Free
            user.PlanType = PlanType.Free;
            user.PayingPlanType = null;
            user.LastPaymentDate = null;
            user.NextPaymentDate = null;
            var updateResult = await _userManager.UpdateAsync(user);

            // 2. حذف الأدوار القديمة ثم إضافة دور Reader
            var newRole = "Reader";

            var oldRoles = await _userManager.GetRolesAsync(user);
            if (oldRoles != null && oldRoles.Any())
            {
                // نحذف كل الأدوار ما عدا "Admin"
                var rolesToRemove = oldRoles.Where(r => r != "Admin").ToList();
                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }
            }

            // إضافة دور Reader
            if (!await _userManager.IsInRoleAsync(user, newRole))
            {
                await _userManager.AddToRoleAsync(user, newRole);
            }

            // 3. تحديث الجلسة فوراً
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: true);

            TempData["Success"] = "You have successfully downgraded to the Free plan. Your permissions have been updated.";
            return RedirectToAction("Profile", "Account");
        }
    }
}