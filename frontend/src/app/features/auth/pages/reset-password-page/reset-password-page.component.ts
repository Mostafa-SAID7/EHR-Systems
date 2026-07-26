import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-reset-password-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="stagger">
      <div class="mb-7">
        <h2 class="heading-lg mb-1">Set new password</h2>
        <p class="body-text">Choose a strong password for your account.</p>
      </div>

      <div *ngIf="success" class="alert-success mb-6">
        <p class="text-sm font-medium">Password reset successful! Redirecting to login…</p>
      </div>

      <form *ngIf="!success" [formGroup]="resetForm" (ngSubmit)="onSubmit()" class="space-y-4">
        <div class="form-field">
          <label for="newPassword" class="input-label">New password</label>
          <input
            id="newPassword"
            type="password"
            formControlName="newPassword"
            placeholder="At least 8 characters"
            [class]="hasError('newPassword') ? 'input-error' : 'input'"
          />
          <p *ngIf="hasError('newPassword')" class="input-error-msg">
            Password must be at least 8 characters
          </p>
        </div>

        <div class="form-field">
          <label for="confirmPassword" class="input-label">Confirm new password</label>
          <input
            id="confirmPassword"
            type="password"
            formControlName="confirmPassword"
            placeholder="Repeat your new password"
            [class]="hasError('confirmPassword') || mismatch ? 'input-error' : 'input'"
          />
          <p *ngIf="mismatch" class="input-error-msg">Passwords do not match</p>
        </div>

        <div *ngIf="serverError" class="alert-error animate-scale-in">
          <span class="text-sm">{{ serverError }}</span>
        </div>

        <button
          type="submit"
          [disabled]="resetForm.invalid || isLoading"
          class="btn-primary w-full py-3"
        >
          {{ isLoading ? 'Resetting password…' : 'Reset Password' }}
        </button>
      </form>

      <p class="mt-6 text-center text-sm text-gray-500 dark:text-gray-400">
        <a routerLink="/auth/login" class="font-semibold text-primary-600 hover:text-primary-700">
          Return to login
        </a>
      </p>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPasswordPageComponent {
  resetForm: FormGroup;
  isLoading = false;
  success = false;
  serverError = '';
  get mismatch(): boolean {
    const f = this.resetForm;
    return f.touched && f.get('newPassword')?.value !== f.get('confirmPassword')?.value;
  }

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef
  ) {
    this.resetForm = this.fb.group({
      newPassword:     ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required],
    });
  }

  hasError(field: string): boolean {
    const c = this.resetForm.get(field);
    return !!(c && c.invalid && c.touched);
  }

  onSubmit(): void {
    if (this.resetForm.invalid || this.mismatch) {
      this.resetForm.markAllAsTouched();
      return;
    }

    const token = this.route.snapshot.paramMap.get('token') ?? '';
    this.isLoading = true;
    this.serverError = '';

    // Call backend reset-password endpoint (POST /api/v1/auth/reset-password)
    this.authService.resetPassword(token, this.resetForm.value.newPassword).subscribe({
      next: () => {
        this.success = true;
        this.notificationService.success('Password Reset', 'Your password has been reset successfully.');
        setTimeout(() => this.router.navigate(['/auth/login']), 2000);
        this.cdr.markForCheck();
      },
      error: (err: any) => {
        this.isLoading = false;
        this.serverError = err?.error?.message || 'Reset failed. Link may have expired.';
        this.cdr.markForCheck();
      }
    });
  }
}
