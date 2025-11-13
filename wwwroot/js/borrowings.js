$(document).ready(function () {

    const antiForgeryToken = $('input[name="__RequestVerificationToken"]').val();
    const activeTabPane = $('#nav-active');
    const returnedTabPane = $('#nav-returned');
    const addTabPane = $('#nav-add');

    let selectedBookId = 0;
    let selectedVisitorId = 0;

    // --- لوجيك تحميل الـ Tabs ---

    function loadTabContent(url, targetPane, forceReload = false) {
        // (تعديل: ضفنا forceReload)
        if (targetPane.data('loaded') === true && !forceReload) {
            return;
        }

        targetPane.html('<div class="text-center p-5"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div></div>');

        fetch(url)
            .then(response => response.text())
            .then(html => {
                targetPane.html(html);
                targetPane.data('loaded', true);

                if (targetPane.attr('id') === 'nav-active') {
                    bindReturnButtons();
                }
                if (targetPane.attr('id') === 'nav-add') {
                    bindAddTabLogic(); // (هنستدعي الدالة الجديدة)
                }
            })
            .catch(err => {
                targetPane.html('<div class="alert alert-danger">Failed to load content.</div>');
            });
    }

    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        const targetPaneId = $(e.target).data('bs-target');
        const targetPane = $(targetPaneId);
        const url = $(e.target).data('url');

        loadTabContent(url, targetPane);
    });

    loadTabContent($('#nav-active-tab').data('url'), activeTabPane);


    // --- (جديد) لوجيك التاب الثالث "Add" (معدل) ---
    function bindAddTabLogic() {
        selectedBookId = 0;
        selectedVisitorId = 0;
        const lendButton = $('#lend-button');
        lendButton.prop('disabled', true);

        $('#book-search-query').on('keyup', function () {
            const query = $(this).val();
            fetch(`/Borrowings/SearchAvailableBooks?query=${query}`)
                .then(res => res.text())
                .then(html => {
                    $('#book-search-results').html(html);
                });
        });

        $('#visitor-search-query').on('keyup', function () {
            const query = $(this).val();
            fetch(`/Borrowings/SearchVisitors?query=${query}`)
                .then(res => res.text())
                .then(html => {
                    $('#visitor-search-results').html(html);
                });
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

        // --- (تعديل) الـ Selector بقى #lend-form ---
        $('#lend-form').on('submit', function (e) {
            e.preventDefault(); // (1. امنع الإرسال العادي)

            const form = $(this);
            const url = form.attr('action');

            // (تأكد إننا بنبعت الـ Token صح)
            const formData = new FormData(this);
            formData.append('__RequestVerificationToken', antiForgeryToken);

            fetch(url, {
                method: 'POST',
                body: formData
            })
                .then(response => {
                    // (3. لو الرد "Redirect" ده معناه نجاح)
                    if (response.redirected) {
                        // (تعديل) بدل ما نعمل ريفريش، هنروح للتاب الأولاني
                        showReturnAlert(false, 'Book lent successfully!'); // (إظهار رسالة نجاح)
                        $('#nav-active-tab').tab('show'); // (افتح التاب الأول)
                        activeTabPane.data('loaded', false); // (اجبره يعمل ريلود)
                        addTabPane.data('loaded', false); // (اجبر تاب الإضافة يعمل ريلود المرة الجاية)
                        return;
                    }

                    // (4. لو الرد "HTML" ده معناه فشل)
                    return response.text();
                })
                .then(html => {
                    if (html) {
                        // (5. اعرض الأخطاء في نفس التاب)
                        addTabPane.html(html);
                    }
                })
                .catch(error => {
                    console.error('Error submitting form:', error);
                });
        });
    }

    // --- (لوجيك الـ Return (زي ما هو)) ---

    function bindReturnButtons() {
        // (الكود ده زي ما هو من المرة اللي فاتت)
        $('.return-btn').on('click', function () {
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
        fetch('/Borrowings/ReturnBook/' + id, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': antiForgeryToken // (استخدام المتغير العام)
            }
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    const row = $('#borrow-row-' + id);
                    row.addClass('table-success');

                    // (تعديل) استخدام الدالة المحدثة
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
                alert("An error occurred while returning the book.");
            });
    }

    // (تعديل) الدالة دي بقت بتقبل "رسالة"
    function showReturnAlert(wasOverdue, message) {
        const alert = $('#return-alert');
        $('#return-alert-message').text(message); // (استخدام الرسالة)

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