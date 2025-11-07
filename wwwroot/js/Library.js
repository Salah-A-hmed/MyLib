// Auto-detect current page and set active state
document.addEventListener("DOMContentLoaded", function () {
    const currentPath = window.location.pathname;
    let currentPage = currentPath.split("/").pop().replace(".html", "");

    if (currentPage === "") {
        currentPage = "index";
    }

    const navLinks = document.querySelectorAll(".nav-link[data-page]");
    navLinks.forEach((link) => link.classList.remove("active"));

    let activeLink = null;
    activeLink = document.querySelector(`[data-page="${currentPage}"]`);

    if (!activeLink) {
        const pageMap = {
            index: "library",
            home: "library",
            dashboard: "library",
            books: "library",
            addbysearch: "add-items",
            collections: "add-collection",
            borrowings: "dashboards"
        };

        const controllerName = window.location.pathname.split('/')[1];
        if (controllerName && pageMap[controllerName.toLowerCase()]) {
            activeLink = document.querySelector(
                `[data-page="${pageMap[controllerName.toLowerCase()]}"]`
            );
        }

        if (!activeLink && pageMap[currentPage]) {
            activeLink = document.querySelector(
                `[data-page="${pageMap[currentPage]}"]`
            );
        }
    }

    if (!activeLink) {
        activeLink = document.querySelector('[data-page="library"]');
    }

    if (activeLink) {
        activeLink.classList.add("active");
    }
});

// Helper to close all dropdowns
function closeAllDropdowns(except = null) {
    document
        .querySelectorAll(
            ".select-dropdown.active, .control-dropdown.active, #userDropdown.show"
        )
        .forEach((dropdown) => {
            if (dropdown !== except) {
                dropdown.classList.remove("active", "show");
            }
        });
}

// User dropdown (هذا الكود موجود أيضاً في site.js، لكن لا ضرر من التأكيد عليه)
document.addEventListener('DOMContentLoaded', function () {
    const userInfo = document.getElementById('userInfo');
    const userDropdown = document.getElementById('userDropdown');

    if (userInfo && userDropdown) {
        userInfo.addEventListener('click', function (e) {
            e.stopPropagation();
            // استخدام Bootstrap JS API لفتح الـ Dropdown
            var myDropdown = new bootstrap.Dropdown(userInfo);
            myDropdown.toggle();
        });

        document.addEventListener('click', function (e) {
            if (userDropdown.classList.contains('show') && !userInfo.contains(e.target)) {
                var myDropdown = new bootstrap.Dropdown(userInfo);
                myDropdown.hide();
            }
        });
    }
});


// Select dropdown (للقائمة الرئيسية في library.html لو وجدت)
const selectDropdown = document.getElementById("selectDropdown");
if (selectDropdown) {
    const selectTrigger = selectDropdown.querySelector(".select-trigger");
    const selectOptions = selectDropdown.querySelectorAll(".select-option");

    selectTrigger.addEventListener("click", function (e) {
        e.stopPropagation();
        closeAllDropdowns(selectDropdown);
        selectDropdown.classList.toggle("active");
    });

    selectOptions.forEach((option) => {
        option.addEventListener("click", function () {
            selectTrigger.querySelector("span").textContent = this.textContent;
            selectDropdown.classList.remove("active");
        });
    });
}


// -----------------------------------------------------------------
// ( 💡 هذا هو الجزء الذي تم إصلاحه 💡 )
// View control (Cover, List, Summary)
// -----------------------------------------------------------------
const viewButton = document.getElementById("viewButton");
if (viewButton) {
    const viewDropdown = viewButton.closest('.control-dropdown'); // ابحث عن الأب
    const viewOptions = viewDropdown.querySelectorAll(".select-option");
    const bookContainer = document.querySelector(".book"); // حاوية الكتب

    viewButton.addEventListener("click", function (e) {
        e.stopPropagation();
        closeAllDropdowns(viewDropdown);
        viewDropdown.classList.toggle("active");
    });

    viewOptions.forEach((option) => {
        option.addEventListener("click", function () {
            const value = this.dataset.value; // "cover", "list", or "summary"
            const text = this.textContent;
            const icon = this.dataset.icon;

            // تحديث شكل الزر
            document.getElementById("viewText").textContent = text;
            viewButton.querySelector("i:first-child").className = icon;
            viewDropdown.classList.remove("active");

            // *** هذا هو الكود المضاف لتغيير الستايل ***
            if (bookContainer) {
                // 1. إزالة كل الكلاسات الخاصة بالعرض القديم
                bookContainer.classList.remove("view-cover", "view-list", "view-summary");
                bookContainer.classList.remove("grid", "grid-cols-2", "sm:grid-cols-3", "md:grid-cols-4", "gap-6");

                // 2. إضافة الكلاس المناسب
                if (value === "cover") {
                    bookContainer.classList.add("view-cover");
                    // إضافة كلاسات Tailwind الخاصة بالـ Grid
                    bookContainer.classList.add("grid", "grid-cols-2", "sm:grid-cols-3", "md:grid-cols-4", "gap-6");
                } else if (value === "list") {
                    bookContainer.classList.add("view-list");
                } else {
                    // الافتراضي هو Summary
                    bookContainer.classList.add("view-summary");
                }
            }
        });
    });
}


// Sort control (Bootstrap dropdown تعمل تلقائياً)
const sortButton = document.getElementById("sortDropdown");
if (sortButton) {
    // لا نحتاج كود JS إضافي هنا
}

// Filter sidebar functionality
const filterButton = document.getElementById("filterButton");
const filterSidebar = document.getElementById("filterSidebar");
const filterOverlay = document.getElementById("filterOverlay");
const filterClose = document.getElementById("filterClose");

if (filterButton && filterSidebar && filterOverlay && filterClose) {
    filterButton.addEventListener("click", function () {
        filterSidebar.classList.add("show");
        filterOverlay.classList.add("show");
        document.body.style.overflow = "hidden";
    });

    function closeFilter() {
        filterSidebar.classList.remove("show");
        filterOverlay.classList.remove("show");
        document.body.style.overflow = "auto";
    }

    filterClose.addEventListener("click", closeFilter);
    filterOverlay.addEventListener("click", closeFilter);
}

// Close dropdowns when clicking outside
document.addEventListener("click", function (e) {
    // لا نغلق dropdowns الخاصة بـ Bootstrap، هي تتعامل مع نفسها
    document.querySelectorAll(".control-dropdown.active, .select-dropdown.active").forEach(d => {
        if (!d.contains(e.target)) {
            d.classList.remove("active");
        }
    });
});