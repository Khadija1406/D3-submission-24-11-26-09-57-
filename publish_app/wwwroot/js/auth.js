(function () {
    const landing = document.querySelector('main');
    const login = document.getElementById('pulse-login');
    const signup = document.getElementById('pulse-signup');

    function route() {
        const hash = location.hash.toLowerCase();
        const isLogin = hash === '#login';
        const isSignup = hash === '#signup';
        if (login) login.classList.toggle('is-active', isLogin);
        if (signup) signup.classList.toggle('is-active', isSignup);
        if (landing) landing.style.display = (isLogin || isSignup) ? 'none' : '';
        document.body.classList.toggle('auth-open', isLogin || isSignup);
        if (isLogin || isSignup) { window.scrollTo(0, 0); }
    }

    window.addEventListener('hashchange', route);
    route();

    // Password visibility toggle with accessible feedback
    document.querySelectorAll('[data-toggle-password]').forEach(btn => {
        btn.addEventListener('click', () => {
            const field = document.getElementById(btn.dataset.togglePassword);
            if (!field) return;
            const isPassword = field.type === 'password';
            field.type = isPassword ? 'text' : 'password';
            btn.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
            btn.classList.toggle('is-showing', isPassword);
        });
    });

    // Real-time password feedback (Norman's principles: Visibility & Feedback)
    function attachPasswordLiveFeedback(inputSelector, feedbackSelector) {
        const input = document.querySelector(inputSelector);
        const feedback = document.querySelector(feedbackSelector);
        if (!input || !feedback) return;

        input.addEventListener('input', () => {
            const len = input.value.length;
            if (len === 0) {
                feedback.innerHTML = '<small class="text-muted"><i class="bi bi-shield-check me-1"></i> Minimum 8 characters required</small>';
                feedback.className = 'auth-pwd-feedback';
            } else if (len < 8) {
                const remaining = 8 - len;
                feedback.innerHTML = `<small class="text-danger"><i class="bi bi-exclamation-circle me-1"></i> ${len} / 8 characters (${remaining} more needed)</small>`;
                feedback.className = 'auth-pwd-feedback is-invalid';
            } else {
                feedback.innerHTML = `<small class="text-success"><i class="bi bi-check-circle-fill me-1"></i> ${len} characters (Password meets requirement)</small>`;
                feedback.className = 'auth-pwd-feedback is-valid';
            }
        });
    }

    attachPasswordLiveFeedback('#login-password', '#login-pwd-feedback');
    attachPasswordLiveFeedback('#signup-password', '#signup-pwd-feedback');

    // AJAX Auth Forms
    document.querySelectorAll('[data-auth-form]').forEach(form => {
        form.addEventListener('submit', async e => {
            e.preventDefault();
            const toast = form.querySelector('.auth-toast');
            if (!form.reportValidity()) return;

            const isSignup = form.dataset.authForm === 'signup';

            if (isSignup) {
                const name = form.querySelector('#signup-name').value.trim();
                const email = form.querySelector('#signup-email').value.trim();
                const password = form.querySelector('#signup-password').value;
                const confirm = form.querySelector('#signup-confirm').value;

                if (!name) {
                    toast.textContent = 'Please enter your name.';
                    toast.classList.add('show', 'is-error');
                    form.querySelector('#signup-name').focus();
                    return;
                }

                // Password length validation: Reject if less than 8 characters!
                if (!password || password.length < 8) {
                    toast.textContent = 'Password must be at least 8 characters long.';
                    toast.classList.add('show', 'is-error');
                    form.querySelector('#signup-password').focus();
                    return;
                }

                if (password !== confirm) {
                    toast.textContent = 'Passwords do not match. Please verify and try again.';
                    toast.classList.add('show', 'is-error');
                    form.querySelector('#signup-confirm').focus();
                    return;
                }

                toast.textContent = 'Creating your account...';
                toast.classList.remove('is-error');
                toast.classList.add('show');

                try {
                    const res = await fetch('/Account/RegisterAjax', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ name, email, password, confirmPassword: confirm })
                    });
                    const data = await res.json();

                    if (data.success) {
                        toast.textContent = data.message || 'Account created successfully!';
                        toast.classList.remove('is-error');
                        toast.classList.add('show');

                        // Exact name set by user during registration:
                        let cleanName = (data.user && data.user.name) ? data.user.name : name;
                        if (cleanName && !cleanName.includes('@')) {
                            localStorage.setItem('pulse.user', cleanName);
                        }

                        if (window.PulseApp && window.PulseApp.sync) {
                            window.PulseApp.sync();
                        }
                        setTimeout(() => {
                            location.hash = '#dashboard';
                        }, 800);
                    } else {
                        toast.textContent = data.message || 'Account creation could not be completed.';
                        toast.classList.add('show', 'is-error');
                    }
                } catch (err) {
                    console.error('Signup error:', err);
                    toast.textContent = 'Network error while creating account. Please try again.';
                    toast.classList.add('show', 'is-error');
                }
            } else {
                // Login
                const emailInput = form.querySelector('#login-email');
                const passwordInput = form.querySelector('#login-password');
                const email = emailInput.value.trim();
                const password = passwordInput.value;
                const remember = form.querySelector('input[name="remember"]')?.checked || false;

                if (!email) {
                    toast.textContent = 'Please enter your username or email address.';
                    toast.classList.add('show', 'is-error');
                    emailInput.focus();
                    return;
                }

                // Password character count validation: Don't let enter if less than 8 characters!
                if (!password || password.length < 8) {
                    toast.textContent = 'Password must be at least 8 characters long.';
                    toast.classList.add('show', 'is-error');
                    passwordInput.focus();
                    return;
                }

                toast.textContent = 'Verifying credentials...';
                toast.classList.remove('is-error');
                toast.classList.add('show');

                try {
                    const res = await fetch('/Account/LoginAjax', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ email, password, rememberMe: remember })
                    });
                    const data = await res.json();

                    if (data.success) {
                        toast.textContent = data.message || 'Sign in successful!';
                        toast.classList.remove('is-error');
                        toast.classList.add('show');

                        // Exact name set by user: NEVER fallback to email prefix which contains numbers
                        if (data.user && data.user.name && !data.user.name.includes('@')) {
                            localStorage.setItem('pulse.user', data.user.name);
                        }

                        if (window.PulseApp && window.PulseApp.sync) {
                            window.PulseApp.sync();
                        }
                        setTimeout(() => {
                            location.hash = '#dashboard';
                        }, 800);
                    } else {
                        // User does not exist or incorrect password: clear feedback
                        toast.textContent = data.message || 'Sign in failed. Only registered accounts may enter.';
                        toast.classList.add('show', 'is-error');
                    }
                } catch (err) {
                    console.error('Login error:', err);
                    toast.textContent = 'Network error while logging in. Please check your connection.';
                    toast.classList.add('show', 'is-error');
                }
            }
        });
    });

    // Reset password helper link
    document.querySelectorAll('[data-demo-link="reset"]').forEach(a => {
        a.addEventListener('click', e => {
            e.preventDefault();
            const t = a.closest('form').querySelector('.auth-toast');
            t.textContent = 'Password reset instructions have been dispatched to your email address.';
            t.classList.remove('is-error');
            t.classList.add('show');
        });
    });
})();
