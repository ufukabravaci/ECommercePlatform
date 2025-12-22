/**
 * Auth Pages JavaScript
 * E-Commerce Platform
 */

// Toastr Config
toastr.options = {
    "closeButton": true,
    "progressBar": true,
    "positionClass": "toast-top-right",
    "timeOut": "5000"
};

// Password Toggle
document.querySelectorAll('.password-toggle').forEach(btn => {
    btn.addEventListener('click', function () {
        const input = this.parentElement.querySelector('input');
        const icon = this.querySelector('i');

        if (input.type === 'password') {
            input.type = 'text';
            icon.classList.replace('fa-eye', 'fa-eye-slash');
        } else {
            input.type = 'password';
            icon.classList.replace('fa-eye-slash', 'fa-eye');
        }
    });
});

// Password Strength Checker
const passwordInput = document.getElementById('Password');
if (passwordInput) {
    passwordInput.addEventListener('input', function () {
        const password = this.value;
        const strengthContainer = document.getElementById('passwordStrength');

        if (!strengthContainer) return;

        const bars = strengthContainer.querySelectorAll('.strength-bar');
        const text = strengthContainer.querySelector('.strength-text');

        if (password.length === 0) {
            strengthContainer.style.display = 'none';
            return;
        }

        strengthContainer.style.display = 'block';

        let strength = 0;
        if (password.length >= 8) strength++;
        if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
        if (/\d/.test(password)) strength++;
        if (/[^a-zA-Z0-9]/.test(password)) strength++;

        const colors = ['#dc2626', '#f59e0b', '#eab308', '#10b981'];
        const texts = ['Zayıf', 'Orta', 'İyi', 'Güçlü'];

        bars.forEach((bar, i) => {
            bar.style.background = i < strength ? colors[strength - 1] : '#e2e8f0';
        });

        if (text) {
            text.textContent = texts[strength - 1] || '';
            text.style.color = colors[strength - 1];
        }
    });
}

// Password Match Validation
const confirmPasswordInput = document.getElementById('ConfirmPassword');
if (confirmPasswordInput && passwordInput) {
    confirmPasswordInput.addEventListener('input', function () {
        if (this.value.length > 0 && passwordInput.value !== this.value) {
            this.classList.add('input-validation-error');
        } else {
            this.classList.remove('input-validation-error');
        }
    });
}

// OTP Input Handler
class OTPHandler {
    constructor(containerSelector, hiddenInputId, formId, autoSubmit = true) {
        this.container = document.querySelector(containerSelector);
        this.hiddenInput = document.getElementById(hiddenInputId);
        this.form = document.getElementById(formId);
        this.autoSubmit = autoSubmit;

        if (this.container) {
            this.inputs = this.container.querySelectorAll('.otp-input');
            this.init();
        }
    }

    init() {
        this.inputs.forEach((input, index) => {
            // Input event
            input.addEventListener('input', (e) => this.handleInput(e, index));

            // Keydown event
            input.addEventListener('keydown', (e) => this.handleKeydown(e, index));

            // Paste event (only first input)
            if (index === 0) {
                input.addEventListener('paste', (e) => this.handlePaste(e));
            }
        });
    }

    handleInput(e, index) {
        const value = e.target.value;

        // Only allow digits
        if (!/^\d*$/.test(value)) {
            e.target.value = '';
            return;
        }

        // Move to next input
        if (value && index < this.inputs.length - 1) {
            this.inputs[index + 1].focus();
        }

        this.updateCode();
    }

    handleKeydown(e, index) {
        if (e.key === 'Backspace' && !e.target.value && index > 0) {
            this.inputs[index - 1].focus();
        }
    }

    handlePaste(e) {
        e.preventDefault();
        const pasteData = e.clipboardData.getData('text').trim();

        if (/^\d{6}$/.test(pasteData)) {
            pasteData.split('').forEach((char, i) => {
                if (this.inputs[i]) {
                    this.inputs[i].value = char;
                }
            });
            this.inputs[this.inputs.length - 1].focus();
            this.updateCode();
        }
    }

    updateCode() {
        let code = '';
        this.inputs.forEach(input => {
            code += input.value;
        });

        if (this.hiddenInput) {
            this.hiddenInput.value = code;
        }

        // Auto submit when complete
        if (this.autoSubmit && code.length === 6 && this.form) {
            this.form.submit();
        }
    }
}

// Resend Timer
class ResendTimer {
    constructor(linkId, textId, countdownId, seconds = 60) {
        this.link = document.getElementById(linkId);
        this.text = document.getElementById(textId);
        this.countdownEl = document.getElementById(countdownId);
        this.seconds = seconds;
        this.countdown = seconds;

        if (this.link) {
            this.init();
        }
    }

    init() {
        this.timer = setInterval(() => {
            this.countdown--;

            if (this.countdownEl) {
                this.countdownEl.textContent = this.countdown;
            }

            if (this.countdown <= 0) {
                clearInterval(this.timer);
                this.link.classList.remove('disabled');
                if (this.text) {
                    this.text.innerHTML = '<i class="fas fa-redo me-1"></i>Tekrar Gönder';
                }
            }
        }, 1000);

        this.link.addEventListener('click', (e) => this.handleResend(e));
    }

    handleResend(e) {
        e.preventDefault();
        if (this.link.classList.contains('disabled')) return;

        // API call would go here
        toastr.info('Yeni kod gönderildi!');

        // Reset timer
        this.countdown = this.seconds;
        this.link.classList.add('disabled');
        if (this.text) {
            this.text.innerHTML = `Tekrar Gönder (<span id="countdown">${this.seconds}</span>s)`;
        }
    }
}

// Form Submit Loading Handler
function initFormLoading(formId, btnId) {
    const form = document.getElementById(formId);
    const btn = document.getElementById(btnId);

    if (form && btn) {
        form.addEventListener('submit', function () {
            const btnText = btn.querySelector('.btn-text');
            const btnLoading = btn.querySelector('.btn-loading');

            if (btnText) btnText.classList.add('d-none');
            if (btnLoading) btnLoading.classList.remove('d-none');
            btn.disabled = true;
        });
    }
}