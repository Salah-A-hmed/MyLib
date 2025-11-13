$(document).ready(function () {

    // (1) هيقرأ التوكن من السطر اللي ضفناه في Index.cshtml
    const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();

    const activeTabPane = $('#nav-active');
    const returnedTabPane = $('#nav-returned');
    const addTabPane = $('#nav-add');

    let selectedBookId = 0;
    let selectedVisitorId = 0;

    // --- لوجيك تحميل الـ Tabs ---

    function loadTabContent(url, targetPane, forceReload = false) {
        if (targetPane.data('loaded') === true && !forceReload) {
            return;
        }

        targetPane.html('<div class="text-center p-5"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');

        fetch(url)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.text();
            })
            .then(html => {
                targetPane.html(html);
                targetPane.data('loaded', true);

                if (targetPane.attr('id') === 'nav-active') {
                    bindReturnButtons();
                }
                if (targetPane.attr('id') === 'nav-add') {
                    bindAddTabLogic();
                }
            })
            .catch(err => {
                console.error("Error loading tab:", err);
                targetPane.html('<div class="alert alert-danger">Failed to load content. Please refresh.</div>');
            });
    }

    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        const targetPaneId = $(e.target).data('bs-target');
        const targetPane = $(targetPaneId);
        const url = $(e.target).data('url');

        loadTabContent(url, targetPane);
    });

    loadTabContent($('#nav-active-tab').data('url'), activeTabPane);


    // --- لوجيك التاب الثالث "Add" ---
    function bindAddTabLogic() {
        selectedBookId = 0;
        selectedVisitorId = 0;
        const lendButton = $('#lend-button');
        lendButton.prop('disabled', true);

        // (ده هيصلح مشكلة التاريخ الفاضي لو الكنترولر مرجعش موديل)
        const borrowDateInput = $('#BorrowDate');
        if (borrowDateInput.val() === "" || borrowDateInput.val() === "0001-01-01") {
            const today = new Date().toISOString().split('T')[0];
            borrowDateInput.val(today);
        }

        $('#book-search-query').on('keyup', function () {
            const query = $(this).val();
            fetch(`/Borrowings/SearchAvailableBooks?query=${query}`)
                .then(res => res.text())
                .then(html => { $('#book-search-results').html(html); });
        });

        $('#visitor-search-query').on('keyup', function () {
            const query = $(this).val();
            fetch(`/Borrowings/SearchVisitors?query=${query}`)
                .then(res => res.text())
                .then(html => { $('#visitor-search-results').html(html); });
        });

        $('#book-search-results').on('click', '.js-book-select-card', function () {
            $('.js-book-select-card').removeClass('selected');
            $(this).addClass('selected');
            selectedBookId = $(this).data('book-id');
            $('#selectedBookId').val(selectedBookId);
            checkLendButtonState();
        });

        $('#visitor-search-results').on('click', '.js-visitor-select-card', function (e) {
            e.preventDefault();
            $('.js-visitor-select-card').removeClass('selected');
            $(this).addClass('selected');
            selectedVisitorId = $(this).data('visitor-id');
            $('#selectedVisitorId').val(selectedVisitorId);
            checkLendButtonState();
        });

        function checkLendButtonState() {
            if (selectedBookId > 0 && selectedVisitorId > 0) {
                lendButton.prop('disabled', false);
            } else {
                lendButton.prop('disabled', true);
            }
        }

        // --- (ده اللوجيك اللي هيصلح "Lend") ---
        $('#lend-form').on('submit', function (e) {
            e.preventDefault();

            const form = $(this);
            // use prop('action') to get the fully qualified action URL (safer when partials are injected)
            const url = form.prop('action') || form.attr('action');
            const formData = new FormData(this);

            // (2) إضافة التوكن يدوياً (ده اللي هيصلح 400 Bad Request)
            if (antiForgeryToken) {
                // append to form body (works for most setups)
                formData.append('__RequestVerificationToken', antiForgeryToken);
            } else {
                console.error("Anti Forgery Token not found!");
                alert("Error: Security token missing. Please refresh the page.");
                return;
            }

            // Also send token in header — some antiforgery configurations expect the token in a header for non-form posts.
            const headers = {
                'RequestVerificationToken': antiForgeryToken,
                'Accept': 'text/html' // we expect HTML partial in failure case
            };

            fetch(url, {
                method: 'POST',
                body: formData,
                headers: headers
            })
                .then(response => {
                    if (response.redirected) {
                        showReturnAlert(false, 'Book lent successfully!');
                        $('#nav-active-tab').tab('show');
                        // (هنعمل ريلود للتابات)
                        loadTabContent($('#nav-active-tab').data('url'), activeTabPane, true); // (forceReload)
                        addTabPane.data('loaded', false);
                        return;
                    }
                    // (لو فشل الـ Validation)
                    return response.text();
                })
                .then(html => {
                    if (html) {
                        addTabPane.html(html);
                        bindAddTabLogic(); // (شغل الـ JS تاني على الفورم الجديدة)
                    }
                })
                .catch(error => {
                    console.error('Error submitting form:', error);
                });
        });
    }


    // --- لوجيك الـ Return ---

    function bindReturnButtons() {
        $('.return-btn').on('click', function () {
            // (الكود ده سليم زي ما هو)
            const button = $(this);
            const id = button.data('id');
            const status = button.data('status');
            const fine = button.data('fine');
            const daysLate = button.data('dayslate');
            const bookTitle = button.data('booktitle');

            if (status === "Overdue") {
                $('#returnModalLabel').html('<i class="fas fa-exclamation-triangle"></i> Overdue: ' + bookTitle);
                $('#returnDaysLate').text(daysLate);
                $('#returnFineAmount').text(new Intl.NumberFormat('en-EG', { style: 'currency', currency: 'EGP' }).format(fine));
                $('#confirmReturnBtn').data('id', id);
                var returnModal = new bootstrap.Modal(document.getElementById('returnModal'));
                returnModal.show();
            } else if (status === "Borrowed") {
                ajaxReturnBook(id);
            }
        });
    }

    $('#confirmReturnBtn').on('click', function () {
        const id = $(this).data('id');
        ajaxReturnBook(id);
        var returnModal = bootstrap.Modal.getInstance(document.getElementById('returnModal'));
        returnModal.hide();
    });

    function ajaxReturnBook(id) {
        // (هنتأكد إننا بنبعت التوكن هنا برضه)
        const headers = {};
        if (antiForgeryToken) {
            headers['RequestVerificationToken'] = antiForgeryToken;
        }

        fetch('/Borrowings/ReturnBook/' + id, {
            method: 'POST',
            headers: headers
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Server responded with ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    const row = $('#borrow-row-' + id);
                    row.addClass('table-success');
                    showReturnAlert(data.wasOverdue,
                        data.wasOverdue ? 'Book returned (Overdue) and fine collected.' : 'Book returned on time!');

                    row.fadeOut(1500, function () {
                        $(this).remove();
                    });

                    returnedTabPane.data('loaded', false);
                } else {
                    alert("Error: " + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert("An error occurred while returning the book. Please check console.");
            });
    }

    function showReturnAlert(wasOverdue, message) {
        const alert = $('#return-alert');
        $('#return-alert-message').text(message);

        if (wasOverdue) {
            alert.removeClass('alert-success').addClass('alert-warning');
        } else {
            alert.removeClass('alert-warning').addClass('alert-success');
        }

        alert.show().addClass('show');
        setTimeout(function () {
            alert.hide().removeClass('show');
        }, 4000);
    }
});