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
        // قراءة حالة الـ Pro من الـ DOM (وجود كلاس pro-bg-gradient)
        const isPro = document.querySelector('#proPlanCard').classList.contains('pro-bg-gradient');
        if (isPro) return;

        // تغيير تسمية الـ Toggle Labels
        const monthlyLabel = document.getElementById('monthlyLabel');
        const yearlyLabel = document.getElementById('yearlyLabel');

        if (toggle.checked) {
            // الحالة السنوية
            animateValue(priceDisplay, priceMonthly, priceYearly, 300);
            periodDisplay.textContent = "/yr";
            billingText.textContent = `Billed $${priceYearly} Yearly (Save $${priceSave})`;
            hiddenInput.value = "yearly";

            yearlyLabel.classList.remove('text-muted');
            monthlyLabel.classList.add('text-muted');
        } else {
            // الحالة الشهرية
            animateValue(priceDisplay, priceYearly, priceMonthly, 300);
            periodDisplay.textContent = "/mo";
            billingText.textContent = "Billed Monthly";
            hiddenInput.value = "monthly";

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

    // دالة لمعالجة إرسال فورم الترقية
    window.handleUpgradeSubmit = function (e) {
        const btn = document.getElementById('upgradeBtn');
        btn.disabled = true;
        btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Processing Payment...';
        // لا نمنع الـ submit هنا (e.preventDefault())، نترك الفورم يرسل بشكل طبيعي للـ Controller
    };

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

    // معالجة زر الداونغريد (إظهار التحميل)
    const downgradeBtn = document.getElementById('downgradeBtn');
    if (downgradeForm) {
        downgradeForm.addEventListener('submit', function (e) {
            // التأكد من أن المستخدم قد أكد العملية قبل أي شيء
            if (!confirm("Are you sure you want to downgrade to the Free plan? You will lose access to all Pro features.")) {
                e.preventDefault();
                return;
            }

            // إذا تم التأكيد، نغير حالة الزر إلى التحميل
            const btn = downgradeForm.querySelector('#downgradeBtn');
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Downgrading...';

            // لا نحتاج لـ e.preventDefault() هنا لأننا نريد للـ Form أن يرسل بشكل طبيعي بعد تغيير حالة الزر
            // الـ Form سيقوم بالإرسال إلى /Upgrade/Downgrade
        });

        // نحذف الـ Event Listener القديم الموجود على الزر نفسه إن وجد
        // ونقوم بتعطيل الـ event listener المباشر على الزر في حالة ما إذا كان مربوطاً بالـ HTML مباشرة
        // (الآن الزر مربوط بالـ Form، وهذا أفضل)
    }
});