// User dropdown functionality
        const userInfo = document.getElementById('userInfo');
        const userDropdown = document.getElementById('userDropdown');

        userInfo.addEventListener('click', function(e) {
            e.stopPropagation();
            userDropdown.classList.toggle('show');
        });

        document.addEventListener('click', function(e) {
            if (!userInfo.contains(e.target)) {
                userDropdown.classList.remove('show');
            }
        });

        // Form validation
        class LendingFormValidator {
            constructor() {
                this.form = document.getElementById('lendingForm');
                this.init();
            }

            init() {
                this.form.addEventListener('submit', (e) => this.handleSubmit(e));
                
                // Real-time validation
                document.getElementById('barcodeTitle').addEventListener('input', () => this.validateBarcodeTitle());
                document.getElementById('checkedOut').addEventListener('change', () => this.validateCheckedOut());
                document.getElementById('dueDate').addEventListener('change', () => this.validateDueDate());
                document.getElementById('patronInfo').addEventListener('input', () => this.validatePatronInfo());

                // Set default dates
                this.setDefaultDates();
            }

            setDefaultDates() {
                const today = new Date();
                const checkedOutInput = document.getElementById('checkedOut');
                const dueDateInput = document.getElementById('dueDate');
                
                // Set checked out to today
                checkedOutInput.value = today.toISOString().split('T')[0];
                
                // Set due date to 14 days from today
                const dueDate = new Date(today);
                dueDate.setDate(dueDate.getDate() + 14);
                dueDateInput.value = dueDate.toISOString().split('T')[0];

                this.validateCheckedOut();
                this.validateDueDate();
            }

            validateBarcodeTitle() {
                const input = document.getElementById('barcodeTitle');
                const error = document.getElementById('barcodeTitleError');
                const success = document.getElementById('barcodeTitleSuccess');

                this.clearValidation(input, error, success);

                if (!input.value.trim()) {
                    this.setInvalid(input, error, 'Please enter a barcode or title.');
                    return false;
                } else if (input.value.trim().length < 3) {
                    this.setInvalid(input, error, 'Please enter at least 3 characters.');
                    return false;
                } else {
                    this.setValid(input, success, 'Item identifier looks good!');
                    return true;
                }
            }

            validateCheckedOut() {
                const input = document.getElementById('checkedOut');
                const error = document.getElementById('checkedOutError');
                const success = document.getElementById('checkedOutSuccess');

                this.clearValidation(input, error, success);

                if (!input.value) {
                    this.setInvalid(input, error, 'Please select the check-out date.');
                    return false;
                } else {
                    const selectedDate = new Date(input.value);
                    const today = new Date();
                    today.setHours(0, 0, 0, 0);
                    
                    if (selectedDate > today) {
                        this.setInvalid(input, error, 'Check-out date cannot be in the future.');
                        return false;
                    } else {
                        this.setValid(input, success, 'Check-out date is valid.');
                        this.validateDueDate(); // Revalidate due date when check-out changes
                        return true;
                    }
                }
            }

            validateDueDate() {
                const dueDateInput = document.getElementById('dueDate');
                const checkedOutInput = document.getElementById('checkedOut');
                const error = document.getElementById('dueDateError');
                const success = document.getElementById('dueDateSuccess');

                this.clearValidation(dueDateInput, error, success);

                if (!dueDateInput.value) {
                    this.setInvalid(dueDateInput, error, 'Please select the due date.');
                    return false;
                } else if (!checkedOutInput.value) {
                    this.setInvalid(dueDateInput, error, 'Please select check-out date first.');
                    return false;
                } else {
                    const dueDate = new Date(dueDateInput.value);
                    const checkedOutDate = new Date(checkedOutInput.value);
                    
                    if (dueDate <= checkedOutDate) {
                        this.setInvalid(dueDateInput, error, 'Due date must be after check-out date.');
                        return false;
                    } else {
                        this.setValid(dueDateInput, success, 'Due date is valid.');
                        return true;
                    }
                }
            }

            validatePatronInfo() {
                const input = document.getElementById('patronInfo');
                const error = document.getElementById('patronInfoError');
                const success = document.getElementById('patronInfoSuccess');

                this.clearValidation(input, error, success);

                if (!input.value.trim()) {
                    this.setInvalid(input, error, 'Please enter patron information.');
                    return false;
                } else if (input.value.trim().length < 2) {
                    this.setInvalid(input, error, 'Please enter at least 2 characters.');
                    return false;
                } else {
                    this.setValid(input, success, 'Patron information looks good!');
                    return true;
                }
            }

            validateAll() {
                const barcodeValid = this.validateBarcodeTitle();
                const checkedOutValid = this.validateCheckedOut();
                const dueDateValid = this.validateDueDate();
                const patronValid = this.validatePatronInfo();

                return barcodeValid && checkedOutValid && dueDateValid && patronValid;
            }

            handleSubmit(e) {
                e.preventDefault();
                
                if (this.validateAll()) {
                    const formData = new FormData(this.form);
                    const data = {
                        barcodeTitle: document.getElementById('barcodeTitle').value,
                        checkedOut: document.getElementById('checkedOut').value,
                        dueDate: document.getElementById('dueDate').value,
                        patronInfo: document.getElementById('patronInfo').value
                    };

                    this.showSuccess(data);
                } else {
                    this.showError();
                }
            }

            showSuccess(data) {
                const submitBtn = document.getElementById('submitBtn');
                submitBtn.innerHTML = '<i class="fas fa-check"></i> Lending Completed!';
                submitBtn.style.backgroundColor = '#28a745';
                
                setTimeout(() => {
                    alert(`Lending completed successfully!\n\nItem: ${data.barcodeTitle}\nPatron: ${data.patronInfo}\nDue: ${data.dueDate}`);
                    
                    // Reset form
                    this.form.reset();
                    this.clearAllValidation();
                    this.setDefaultDates();
                    
                    submitBtn.innerHTML = 'Complete Lending';
                    submitBtn.style.backgroundColor = 'var(--button-bg-color)';
                }, 1000);
            }

            showError() {
                const submitBtn = document.getElementById('submitBtn');
                submitBtn.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Please Fix Errors';
                submitBtn.style.backgroundColor = '#dc3545';
                
                setTimeout(() => {
                    submitBtn.innerHTML = 'Complete Lending';
                    submitBtn.style.backgroundColor = 'var(--button-bg-color)';
                }, 2000);
            }

            // Helper methods
            setValid(element, success, message) {
                element.classList.remove('is-invalid');
                element.classList.add('is-valid');
                success.textContent = message;
            }

            setInvalid(element, error, message) {
                element.classList.remove('is-valid');
                element.classList.add('is-invalid');
                error.textContent = message;
            }
        }