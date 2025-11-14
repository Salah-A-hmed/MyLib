// Enhanced Auth UI behavior: safe selectors, per-form handling, unobtrusive-friendly.
// Runs after DOM is ready and won't throw if elements are missing.

document.addEventListener('DOMContentLoaded', function () {
  // Email regex used for lightweight client-side feedback (server still authoritative)
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  // Find all auth forms we care about (Login and Register)
  const forms = document.querySelectorAll('#loginForm, #registerForm');
  forms.forEach(setupForm);

  // Also run bootstrap-like validation for any form with .needs-validation
  Array.from(document.getElementsByClassName('needs-validation')).forEach(form => {
    form.addEventListener('submit', (e) => {
      if (!form.checkValidity()) {
        e.preventDefault();
        e.stopPropagation();
      }
      form.classList.add('was-validated');
    }, false);
  });

  function setupForm(form) {
    if (!form) return;

    // Useful input selectors — Razor Identity uses names like "Input.Email" and ids like "Input_Email"
    const email = form.querySelector('input[name$="Email"], input[id$="_Email"]');
    const password = form.querySelector('input[name$="Password"], input[id$="_Password"]');
    const fullName = form.querySelector('input[name$="FullName"], input[id$="_FullName"]');
    const confirm = form.querySelector('input[name$="ConfirmPassword"], input[id$="_ConfirmPassword"]');

    // real-time feedback
    [email, password, fullName, confirm].forEach(input => {
      if (!input) return;
      input.addEventListener('input', () => validateField(input));
    });

    // Submit handler: enhance UX but allow valid forms to submit normally so server-side validation runs
    form.addEventListener('submit', function (e) {
      // Clear previous top-level validation summary styling (leave messages intact)
      let isValid = true;

      // HTML5 validity is a good baseline
      if (!form.checkValidity()) {
        isValid = false;
      }

      // Extra checks: email format and password min length for nicer UX
      if (email) {
        if (!email.value || !emailRegex.test(email.value)) {
          markInvalid(email);
          isValid = false;
        } else {
          markValid(email);
        }
      }

      if (password) {
        // for register there will also be confirm; for login simple minimum length
        if (!password.value || password.value.length < 6) {
          markInvalid(password);
          isValid = false;
        } else {
          markValid(password);
        }
      }

      if (confirm && password) {
        if (confirm.value !== password.value) {
          markInvalid(confirm);
          markInvalid(password);
          isValid = false;
        } else {
          // only mark valid if both have content and match
          if (confirm.value && password.value && confirm.value === password.value) {
            markValid(confirm);
            markValid(password);
          }
        }
      }

      if (fullName) {
        if (!fullName.value || fullName.value.trim().length < 2) {
          markInvalid(fullName);
          isValid = false;
        } else {
          markValid(fullName);
        }
      }

      if (!isValid) {
        // Prevent submission so server won't be called unnecessarily.
        e.preventDefault();
        e.stopPropagation();

        // Focus first invalid field
        const firstInvalid = form.querySelector('.is-invalid, :invalid');
        if (firstInvalid) firstInvalid.focus();

        // Add Bootstrap visual state for the form so client-side messages (if any) appear consistently
        form.classList.add('was-validated');
      } else {
        // Allow natural form submission (server-side validation and Identity pipeline will run).
        // Remove previous client-only success classes to avoid persisting state if server returns errors.
        Array.from(form.querySelectorAll('.is-valid')).forEach(el => el.classList.remove('is-valid'));
      }
    });
  }

  function validateField(input) {
    if (!input) return;
    const name = input.getAttribute('name') || input.id || '';

    if (name.toLowerCase().includes('email')) {
      if (input.value && emailRegex.test(input.value)) markValid(input);
      else if (input.value) markInvalid(input);
      else clearState(input);
      return;
    }

    if (name.toLowerCase().includes('password')) {
      if (input.value && input.value.length >= 6) markValid(input);
      else if (input.value) markInvalid(input);
      else clearState(input);
      return;
    }

    if (name.toLowerCase().includes('confirmpassword')) {
      const form = input.form;
      const pwd = form ? form.querySelector('input[name$="Password"], input[id$="_Password"]') : null;
      if (pwd && input.value && input.value === pwd.value) {
        markValid(input);
        markValid(pwd);
      } else if (input.value) {
        markInvalid(input);
      } else {
        clearState(input);
      }
      return;
    }

    // Generic non-empty check for full name or other inputs
    if (input.value) markValid(input);
    else clearState(input);
  }

  function markInvalid(el) {
    if (!el) return;
    el.classList.remove('is-valid');
    el.classList.add('is-invalid');
  }

  function markValid(el) {
    if (!el) return;
    el.classList.remove('is-invalid');
    el.classList.add('is-valid');
  }

  function clearState(el) {
    if (!el) return;
    el.classList.remove('is-invalid', 'is-valid');
  }
});