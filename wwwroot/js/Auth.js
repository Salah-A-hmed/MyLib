// Bootstrap form validation
        (function() {
            'use strict';
            window.addEventListener('load', function() {
                var forms = document.getElementsByClassName('needs-validation');
                var validation = Array.prototype.filter.call(forms, function(form) {
                    form.addEventListener('submit', function(event) {
                        if (form.checkValidity() === false) {
                            event.preventDefault();
                            event.stopPropagation();
                        }
                        form.classList.add('was-validated');
                    }, false);
                });
            }, false);
        })();

        // Custom validation for better UX
        document.getElementById('loginForm').addEventListener('submit', function(e) {
            e.preventDefault();
            
            const email = document.getElementById('email');
            const password = document.getElementById('password');
            let isValid = true;

            // Email validation
            if (!email.value) {
                email.classList.add('is-invalid');
                email.classList.remove('is-valid');
                isValid = false;
            } else if (!isValidEmail(email.value)) {
                email.classList.add('is-invalid');
                email.classList.remove('is-valid');
                isValid = false;
            } else {
                email.classList.remove('is-invalid');
                email.classList.add('is-valid');
            }

            // Password validation
            if (!password.value) {
                password.classList.add('is-invalid');
                password.classList.remove('is-valid');
                isValid = false;
            } else if (password.value.length < 6) {
                password.classList.add('is-invalid');
                password.classList.remove('is-valid');
                isValid = false;
            } else {
                password.classList.remove('is-invalid');
                password.classList.add('is-valid');
            }

            if (isValid) {
                // Form is valid, you can submit it here
                alert('Login form is valid! Ready to submit.');
                // You would typically send the data to your server here
            }
        });

        // Real-time validation
        document.getElementById('email').addEventListener('input', function() {
            if (this.value && isValidEmail(this.value)) {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
            } else if (this.value) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else {
                this.classList.remove('is-invalid', 'is-valid');
            }
        });

        document.getElementById('password').addEventListener('input', function() {
            if (this.value && this.value.length >= 6) {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
            } else if (this.value) {
                this.classList.add('is-invalid');
                this.classList.remove('is-valid');
            } else {
                this.classList.remove('is-invalid', 'is-valid');
            }
        });

        function isValidEmail(email) {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return emailRegex.test(email);
        }