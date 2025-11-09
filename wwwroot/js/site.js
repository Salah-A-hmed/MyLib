// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', function () {

    // -----------------------------------------------------------------
    //  كود تحديد الصفحة النشطة (النسخة المعدلة)
    // -----------------------------------------------------------------
    try {
        const path = window.location.pathname.toLowerCase();

        const navLinks = document.querySelectorAll(".nav-link[data-page]");
        navLinks.forEach((link) => link.classList.remove("active"));

        let activePageData = null; // (القيمة الافتراضية هي "لا شيء")

        // (أ) فحص الحالات الخاصة أولاً (حسب الـ Action)
        if (path.includes("/books/addbook")) {
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

document.addEventListener('DOMContentLoaded', function () {

    // --- كود الـ Modal الموحد للحذف ---
    var DeleteConfirmationModal = document.getElementById('DeleteConfirmationModal');
    if (DeleteConfirmationModal) {
        DeleteConfirmationModal.addEventListener('show.bs.modal', function (event) {
            // الزرار اللي داس عليه المستخدم (اللي فتح الـ modal)
            var button = event.relatedTarget;

            // استخراج المعلومات من الزرار
            var deleteUrl = button.getAttribute('data-delete-url');
            var entityName = button.getAttribute('data-entity-name');

            // تحديث رسالة التأكيد
            var modalMessage = DeleteConfirmationModal.querySelector('#DeleteConfirmationModalMessage');
            if (entityName) {
                modalMessage.textContent = 'Are you sure you want to delete "' + entityName + '" ? This cannot be reversed.';
            } else {
                modalMessage.textContent = 'Are you sure you want to delete This ? It cannot be reversed.';
            }

            // تحديث الـ action بتاع الفورم
            var form = DeleteConfirmationModal.querySelector('#DeleteConfirmationModalForm');
            form.setAttribute('action', deleteUrl);
        });
    }

});