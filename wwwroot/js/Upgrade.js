document.addEventListener('DOMContentLoaded', function () {
    const toggle = document.getElementById('billingToggle');
    const priceDisplay = document.getElementById('priceDisplay');
    const periodDisplay = document.getElementById('periodDisplay');
    const billingText = document.getElementById('billingText');
    const hiddenInput = document.getElementById('selectedPlanType');

    // الأسعار
    const priceMonthly = 29;
    const priceYearly = 278; // $29 * 12 = $348. $278 is approx 20% off.
    const priceSave = 70; // 348 - 278 = 70

    // دالة تحديث الواجهة (تعمل فقط إذا لم يكن المستخدم مشتركاً بالفعل)
    function updatePricing() {
        const isPro = document.getElementById('currentPlanBadge').classList.contains('pro-bg-gradient');
        if (isPro) return;

        // تغيير تسمية الـ Toggle Labels
        const monthlyLabel = document.getElementById('monthlyLabel');
        const yearlyLabel = document.getElementById('yearlyLabel');

        if (toggle.checked) {
            // الحالة السنوية
            animateValue(priceDisplay, priceMonthly, priceYearly, 300);
            periodDisplay.textContent = "/yr";
            billingText.textContent = `Billed $${priceYearly} Yearly (Save $${priceSave})`;
            hiddenInput.value = "Yearly";

            yearlyLabel.classList.remove('text-muted');
            monthlyLabel.classList.add('text-muted');
        } else {
            // الحالة الشهرية
            animateValue(priceDisplay, priceYearly, priceMonthly, 300);
            periodDisplay.textContent = "/mo";
            billingText.textContent = `Billed $${priceMonthly} Monthly`;
            hiddenInput.value = "Monthly";

            yearlyLabel.classList.add('text-muted');
            monthlyLabel.classList.remove('text-muted');
        }
    }

    // استماع لزر التبديل
    if (toggle) {
        toggle.addEventListener('change', updatePricing);
        // تشغيل التحديث الأولي عند التحميل
        updatePricing();
    }
    // دالة بسيطة لعمل تأثير عداد الأرقام (Animation)
    function animateValue(obj, start, end, duration) {
        let startTimestamp = null;
        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;
            const progress = Math.min((timestamp - startTimestamp) / duration, 1);
            const val = Math.floor(progress * (end - start) + start);
            obj.innerHTML = "$" + val;
            if (progress < 1) {
                window.requestAnimationFrame(step);
            } else if (progress === 1) {
                obj.innerHTML = `$${end}`; // ضمان القيمة النهائية الدقيقة
            }
        };
        window.requestAnimationFrame(step);
    }

    // --------------------------------------------------------
    // 1. منطق الـ Modal للترقية (Upgrade)
    // --------------------------------------------------------
    $('#openUpgradeModalBtn').on('click', function () {
        // قراءة الرابط من الـ data-url attribute
        const baseUrl = $(this).data('url');
        const payingPlanType = hiddenInput ? hiddenInput.value : "Monthly";

        // دمج الرابط مع الـ Query String
        const finalUrl = `${baseUrl}?payingPlanType=${payingPlanType}`;

        $('#upgradeModalContainer').load(finalUrl, function (response, status, xhr) {
            if (status == "error") {
                alert("Error loading modal: " + xhr.status + " " + xhr.statusText);
                return;
            }

            const modalElement = document.getElementById('upgradeConfirmationModal');
            const modal = new bootstrap.Modal(modalElement);
            modal.show();

            // ربط زر التأكيد داخل المودال
            $('#modalConfirmUpgradeBtn').on('click', function () {
                const btn = $(this);
                btn.prop('disabled', true);
                btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Processing...');
                $('#confirmUpgradeForm').submit();
            });
        });
    });
    // --------------------------------------------------------
    // 2. منطق الـ Modal للعودة للخطة المجانية (Downgrade)
    // --------------------------------------------------------
    $('#openDowngradeModalBtn').on('click', function () {
        // قراءة الرابط من الـ data-url attribute
        const url = $(this).data('url');

        $('#downgradeModalContainer').load(url, function (response, status, xhr) {
            if (status == "error") {
                alert("Error loading modal: " + xhr.status + " " + xhr.statusText);
                return;
            }

            const modalElement = document.getElementById('downgradeConfirmationModal');
            const modal = new bootstrap.Modal(modalElement);
            modal.show();

            $('#modalConfirmDowngradeBtn').on('click', function () {
                const btn = $(this);
                btn.prop('disabled', true);
                btn.html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Downgrading...');
                $('#confirmDowngradeForm').submit();
            });
        });
    });
});