document.addEventListener('DOMContentLoaded', function () {

    // -----------------------------------------------------------------
    // (1) كود الـ User Dropdown (الذي كان موجوداً لديك)
    // -----------------------------------------------------------------
    const userInfo = document.getElementById('userInfo');
    const userDropdown = document.getElementById('userDropdown');

    if (userInfo && userDropdown) {
        userInfo.addEventListener('click', function (e) {
            e.stopPropagation();
            // استخدام Bootstrap JS API لفتح الـ Dropdown
            var myDropdown = new bootstrap.Dropdown(userInfo);
            myDropdown.toggle();
        });

        // (اختياري) إغلاق الـ dropdown عند الضغط خارجها
        document.addEventListener('click', function (e) {
            if (userDropdown.classList.contains('show') && !userInfo.contains(e.target)) {
                var myDropdown = bootstrap.Dropdown.getInstance(userInfo);
                if (myDropdown) myDropdown.hide();
            }
        });
    }

    // -----------------------------------------------------------------
    // (2) كود تحديد الصفحة النشطة (الذي تم نقله)
    // -----------------------------------------------------------------
    try {
        const path = window.location.pathname.toLowerCase();

        const navLinks = document.querySelectorAll(".nav-link[data-page]");
        navLinks.forEach((link) => link.classList.remove("active"));

        let activePageData = null;

        // (أ) فحص الحالات الخاصة أولاً (حسب الـ Action)
        if (path.includes("/books/addbysearch")) {
            activePageData = "add-items";
        }

        // (ب) إذا لم تكن حالة خاصة، افحص الـ Controller
        else if (path.startsWith("/collections")) {
            activePageData = "add-collection";
        }
        else if (path.startsWith("/borrowings")) {
            activePageData = "dashboards";
        }

        // (ج) الحالة الافتراضية (صفحة الكتب، الرئيسية)
        else if (path.startsWith("/books") || path.startsWith("/home") || path === "/") {
            activePageData = "library";
        }

        // (د) تفعيل اللينك الصحيح
        if (activePageData) {
            const activeLink = document.querySelector(`.nav-link[data-page="${activePageData}"]`);
            if (activeLink) {
                activeLink.classList.add("active");
            }
        } else {
            // (هـ) إذا فشل كل شيء، اجعل "Library" هي النشطة
            const defaultLink = document.querySelector('.nav-link[data-page="library"]');
            if (defaultLink) {
                defaultLink.classList.add("active");
            }
        }
    } catch (e) {
        console.error("Error setting active nav link:", e);
    }

});