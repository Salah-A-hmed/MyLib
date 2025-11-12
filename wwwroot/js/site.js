document.addEventListener('DOMContentLoaded', function () {

    // -----------------------------------------------------------------
    //  (1) كود تحديد الصفحة النشطة (الخاص بك)
    // -----------------------------------------------------------------
    try {
        const path = window.location.pathname.toLowerCase();
        const navLinks = document.querySelectorAll(".nav-link[data-page]");
        navLinks.forEach((link) => link.classList.remove("active"));
        let activePageData = null; 

        if (path.includes("/books/addbook")) {
            activePageData = "add-items";
        }
        else if (path.startsWith("/collections")) {
            activePageData = "add-collection";
        }
        else if (path.startsWith("/borrowings")) {
            activePageData = "borrowings";
        }
        else if (path.startsWith("/books")) {
            activePageData = "library";
        }
        else if (path.startsWith("/home") || path === "/") {
            // No action
        }

        if (activePageData) {
            const activeLink = document.querySelector(`.nav-link[data-page="${activePageData}"]`);
            if (activeLink) {
                activeLink.classList.add("active");
            }
        }
    } catch (e) {
        console.error("Error setting active nav link:", e);
    }

    
    // -----------------------------------------------------------------
    //  (2) كود الـ Modal الموحد للحذف (الخاص بك)
    // -----------------------------------------------------------------
    var confirmationModalEl = document.getElementById('DeleteConfirmationModal'); // (بنستخدم الـ ID الصحيح)
    if (confirmationModalEl) {
        confirmationModalEl.addEventListener('show.bs.modal', function (event) {
            var button = event.relatedTarget;
            var deleteUrl = button.getAttribute('data-delete-url');
            var entityName = button.getAttribute('data-entity-name');
            
            var modalMessage = confirmationModalEl.querySelector('#DeleteConfirmationModalMessage');
            if (entityName) {
                modalMessage.textContent = 'Are you sure you want to delete "' + entityName + '"? This action cannot be undone.';
            } else {
                modalMessage.textContent = 'Are you sure you want to delete this item? This action cannot be undone.';
            }

            var form = confirmationModalEl.querySelector('#DeleteConfirmationModalForm');
            form.setAttribute('action', deleteUrl);
        });
    }

    // -----------------------------------------------------------------
    //  (3) كود "محاكاة الصفحة" الجديد
    // -----------------------------------------------------------------
    const libraryContent = document.getElementById('library-main-content');
    const detailsView = document.getElementById('book-details-view');
    const backBtn = document.getElementById('backToLibraryBtn');
    const bookCards = document.querySelectorAll('.js-book-details-trigger');

    // (أ) وظيفة إظهار التفاصيل
    // (أ) وظيفة إظهار التفاصيل (النسخة المحدثة)
    function showDetailsView(card) {
        if (!libraryContent || !detailsView || !card) return;

        // 1. قراءة البيانات من الكارت
        var bookId = card.getAttribute('data-book-id');
        var title = card.getAttribute('data-title');
        var author = card.getAttribute('data-author');
        var description = card.getAttribute('data-description');
        var cover = card.getAttribute('data-cover');
        var isbn = card.getAttribute('data-isbn');
        var publisher = card.getAttribute('data-publisher');
        var year = card.getAttribute('data-year');
        var pages = card.getAttribute('data-pages');
        // --- (التعديلات هنا) ---
        var totalCopies = card.getAttribute('data-total-copies');
        var availableCopies = card.getAttribute('data-available-copies');
        var price = card.getAttribute('data-price');
        // --- (نهاية التعديلات) ---
        var status = card.getAttribute('data-status');
        var rating = card.getAttribute('data-rating'); // ده رقم (e.g., "4.5")
        var review = card.getAttribute('data-review');
        var editUrl = card.getAttribute('data-edit-url');
        var deleteUrl = card.getAttribute('data-delete-url');
        var missingImgUrl = card.getAttribute('data-missing-img-url');

        // 2. ملء البيانات في حاوية التفاصيل
        var view = detailsView;
        view.querySelector('#modalBookTitle').textContent = title || 'N/A';
        view.querySelector('#modalBookAuthor').textContent = author || 'N/A';
        view.querySelector('#modalBookDescription').textContent = description || 'No description provided.';
        view.querySelector('#modalBookISBN').textContent = isbn || 'N/A';
        view.querySelector('#modalBookPublisher').textContent = publisher || 'N/A';
        view.querySelector('#modalBookYear').textContent = year || 'N/A';
        view.querySelector('#modalBookPages').textContent = pages || 'N/A';

        var stockEl = view.querySelector('#modalBookStock'); // (اتأكد إن الـ ID ده موجود في _BookDetailsContent.cshtml)
        if (stockEl) {
            stockEl.textContent = availableCopies + " Available / " + totalCopies + " Total";
        }
        var priceEl = view.querySelector('#modalBookPrice'); // (هنحتاج نضيف ID ده)
        if (priceEl) {
            priceEl.textContent = (price && parseFloat(price) > 0) ? ("$" + parseFloat(price).toFixed(2)) : "N/A";
        }
        var reviewP = view.querySelector('#modalBookReview');
        if (review && review.trim() !== "") {
            reviewP.textContent = review;
        } else {
            reviewP.textContent = 'No review yet.';
        }

        view.querySelector('#modalBookImage').src = cover ? cover : missingImgUrl;
        view.querySelector('#modalBookImage').alt = title;

        // 3. تحديث أزرار الحذف والتعديل
        view.querySelector('#modalEditButton').href = editUrl;
        var deleteBtn = view.querySelector('#modalDeleteButton');
        deleteBtn.setAttribute('data-delete-url', deleteUrl);
        deleteBtn.setAttribute('data-entity-name', title);

        // 4. تحديث الـ Status Badge
        var statusBadge = view.querySelector('#modalBookStatusBadge');
        if (statusBadge) {
            statusBadge.textContent = status;
            statusBadge.className = "badge rounded-pill fs-6 mb-2";
            if (status === "Completed") statusBadge.classList.add("bg-success");
            else if (status === "In Progress") statusBadge.classList.add("bg-info", "text-dark");
            else if (status === "Not Begun") statusBadge.classList.add("bg-secondary");
            else statusBadge.classList.add("bg-light", "text-dark");
        }

        // 5. (جديد) لوجيك إنشاء النجوم
        var ratingValue = parseFloat(rating) || 0; // تحويل الرقم
        var starsContainer = view.querySelector('#modalBookRatingStars');
        var ratingText = view.querySelector('#modalBookRating');

        starsContainer.innerHTML = ""; // فضي الحاوية

        if (ratingValue > 0) {
            for (let i = 1; i <= 5; i++) {
                if (i <= ratingValue) {
                    // نجمة مليانة
                    starsContainer.innerHTML += '<i class="fas fa-star"></i>';
                } else if (i - 0.5 <= ratingValue) {
                    // نص نجمة
                    starsContainer.innerHTML += '<i class="fas fa-star-half-alt"></i>';
                } else {
                    // نجمة فاضية
                    starsContainer.innerHTML += '<i class="far fa-star"></i>';
                }
            }
            ratingText.textContent = `(${ratingValue.toFixed(1)})`; // (4.5)
        } else {
            // لو مفيش تقييم
            for (let i = 1; i <= 5; i++) {
                starsContainer.innerHTML += '<i class="far fa-star"></i>'; // 5 نجوم فاضية
            }
            ratingText.textContent = "(No rating)";
        }

        // 6. تبديل العرض
        libraryContent.style.display = 'none';
        detailsView.style.display = 'block';
        // 7. (جديد) تحديث الـ URL Hash
        // ده بيغير الـ URL في المتصفح من غير ما يعمل Reload
        // عشان لو عمل "Back" يرجع للـ URL ده
        window.history.pushState(null, "", "#book-" + bookId);
    }
    // (ب) وظيفة إظهار المكتبة (زرار الرجوع)
    function showLibraryView() {
        if (!libraryContent || !detailsView) return;
        libraryContent.style.display = 'block';
        detailsView.style.display = 'none';
        // (جديد) إزالة الـ Hash من الـ URL
        // (بنستخدم pathname عشان نرجع للـ URL الأصلي: /Books)
        window.history.pushState(null, "", window.location.pathname);
    }

    // (ج) ربط كل الكروت بوظيفة إظهار التفاصيل
    bookCards.forEach(card => {
        card.addEventListener('click', function (e) {
            // نتأكد إن اللي داس عليه مش زرار (لو لسه في أزرار قديمة)
            if (e.target.closest('A, BUTTON')) {
                return;
            }
            showDetailsView(this);
        });
    });

    // (د) ربط زرار الرجوع بوظيفة إظهار المكتبة
    if (backBtn) {
        backBtn.addEventListener('click', showLibraryView);
    }
    
    // -----------------------------------------------------------------
    //  (4) (جديد) كود التشغيل عند تحميل الصفحة (لقراءة الـ Hash)
    // -----------------------------------------------------------------
    // الكود ده بيشوف لو الـ URL فيه "هاش" (زي #book-5) أول ما الصفحة تفتح
    // لو لقى "هاش"، بيفتح التفاصيل بتاعة الكتاب ده علطول.

    function checkHashAndShowDetails() {
        if (window.location.hash && window.location.hash.startsWith("#book-")) {
            const bookId = window.location.hash.substring(6); // بيشيل #book-
            const cardToOpen = document.querySelector(`.js-book-details-trigger[data-book-id="${bookId}"]`);

            if (cardToOpen) {
                showDetailsView(cardToOpen);
            }
        }
    }

    // شغل الكود ده أول ما الصفحة تفتح
    checkHashAndShowDetails();

}); // نهاية الـ DOMContentLoaded