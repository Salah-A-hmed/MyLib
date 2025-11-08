// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {

    // -----------------------------------------------------------------
    // (1) كود الـ User Dropdown (كما هو)
    // -----------------------------------------------------------------
    const userInfo = document.getElementById('userInfo');
    const userDropdown = document.getElementById('userDropdown');

    if (userInfo && userDropdown) {
        userInfo.addEventListener('click', function (e) {
            e.stopPropagation();
            var myDropdown = new bootstrap.Dropdown(userInfo);
            myDropdown.toggle();
        });

        document.addEventListener('click', function (e) {
            if (userDropdown.classList.contains('show') && !userInfo.contains(e.target)) {
                var myDropdown = bootstrap.Dropdown.getInstance(userInfo);
                if (myDropdown) myDropdown.hide();
            }
        });
    }

    // -----------------------------------------------------------------
    // (2) كود تحديد الصفحة النشطة (النسخة المعدلة)
    // -----------------------------------------------------------------
    try {
        const path = window.location.pathname.toLowerCase();

        const navLinks = document.querySelectorAll(".nav-link[data-page]");
        navLinks.forEach((link) => link.classList.remove("active"));

        let activePageData = null; // (القيمة الافتراضية هي "لا شيء")

        // (أ) فحص الحالات الخاصة أولاً (حسب الـ Action)
        if (path.includes("/books/addbysearch" || path.includes("/books/create"))) {
            activePageData = "add-items";
        }

        // (ب) إذا لم تكن حالة خاصة، افحص الـ Controller
        else if (path.startsWith("/collections")) {
            activePageData = "add-collection";
        }
        else if (path.startsWith("/borrowings")) {
            activePageData = "borrowings";
        }

        // (ج) حالة الـ Library (الكتب)
        else if (path.startsWith("/books")) {
            activePageData = "library";
        }

        // (د) حالة الصفحة الرئيسية (Home) أو الجذر (/)
        // (هنا تم التعديل: لا تقم بتعيين أي قيمة، اتركها null)
        else if (path.startsWith("/home") || path === "/") {
            // لا تفعل شيئاً
        }

        // (هـ) تفعيل اللينك الصحيح (فقط إذا وجدنا تطابق)
        if (activePageData) {
            const activeLink = document.querySelector(`.nav-link[data-page="${activePageData}"]`);
            if (activeLink) {
                activeLink.classList.add("active");
            }
        }

        // (و) تم حذف "else"
        // إذا لم نجد تطابقاً (بما في ذلك Home)، لن يتم تفعيل أي لينك

    } catch (e) {
        console.error("Error setting active nav link:", e);
    }

});