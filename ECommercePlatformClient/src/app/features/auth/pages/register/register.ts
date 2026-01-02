import { ChangeDetectionStrategy, Component, inject, signal, computed } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services';
import { ReactiveFormsModule, FormBuilder, Validators, AbstractControl, ValidationErrors } from '@angular/forms';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './register.html',
  styleUrl: './register.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    },
    {
      validators: this.passwordMatchValidator
    }
  );

  private passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
    const p = control.get('password')?.value;
    const cp = control.get('confirmPassword')?.value;
    return p === cp ? null : { passwordMismatch: true };
  }

  passwordStrength = computed(() => {
  const pass = this.form.controls.password.value;
  if (!pass) return null;

  let level = 0;
  if (pass.length >= 6) level++;
  if (pass.length >= 8) level++;
  if (/[A-Z]/.test(pass)) level++;
  if (/[0-9]/.test(pass)) level++;
  if (/[^A-Za-z0-9]/.test(pass)) level++;

  if (level <= 1) return { level: 1, text: 'Zayıf', class: 'bg-danger' };
  if (level <= 2) return { level: 2, text: 'Orta', class: 'bg-warning' };
  if (level <= 3) return { level: 3, text: 'İyi', class: 'bg-info' };
  return { level: 4, text: 'Güçlü', class: 'bg-success' };
});

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.authService.register(this.form.getRawValue()).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.success.set(res.isSuccessful);
        if (!res.isSuccessful) {
          this.error.set(res.errorMessages?.join(', ') ?? 'Kayıt başarısız');
        }
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err.message ?? 'Bir hata oluştu');
      }
    });
  }

  togglePasswordVisibility(type: 'password' | 'confirm') {
  if (type === 'password') {
    this.showPassword.update(v => !v);
  } else {
    this.showConfirmPassword.update(v => !v);
  }
}
}