document.addEventListener("DOMContentLoaded", function () {
    // هذه الدالة ستقوم بتشغيل كل عناصر التحكم الخاصة بالصفحة
    initializePageControls();
});

// Helper to close all non-bootstrap dropdowns
function closeAllDropdowns(except = null) {
    document
        .querySelectorAll(
            ".select-dropdown.active, .control-dropdown.active"
        )
        .forEach((dropdown) => {
            if (dropdown !== except && !dropdown.contains(except)) {
                dropdown.classList.remove("active");
            }
        });
}

// دالة لتشغيل كل عناصر التحكم في الصفحة
function initializePageControls() {

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

    // View control (Cover, List, Summary)
    const viewButton = document.getElementById("viewButton");
    if (viewButton) {
        const viewDropdown = viewButton.closest('.control-dropdown');
        const viewOptions = viewDropdown.querySelectorAll(".select-option");
        const bookContainer = document.querySelector(".book");

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

                document.getElementById("viewText").textContent = text;
                viewButton.querySelector("i:first-child").className = icon;
                viewDropdown.classList.remove("active");

                if (bookContainer) {
                    bookContainer.classList.remove("view-cover", "view-list", "view-summary");
                    bookContainer.classList.remove("grid", "grid-cols-2", "sm:grid-cols-3", "md:grid-cols-4", "gap-6");

                    if (value === "cover") {
                        bookContainer.classList.add("view-cover");
                        bookContainer.classList.add("grid", "grid-cols-2", "sm:grid-cols-3", "md:grid-cols-4", "gap-6");
                    } else if (value === "list") {
                        bookContainer.classList.add("view-list");
                    } else {
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
}