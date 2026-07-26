import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-forgot-password-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="stagger">
      <div class="mb-7">
        <h2 class="heading-lg mb-1">Reset your password</h2>
        <p class="body-text">Enter your email address and we'll send you a password reset link.</p>
      </div>

      <div *ngIf="submitted" class="alert-success mb-6">
        <p class="text-sm">Password reset link sent! Check your inbox for instructions.</p>
      </div>

      <form *ngIf="!submitted" [formGroup]="forgotForm" (ngSubmit)="onSubmit()" class="space-y-4">
        <div class="form-field">
          <label for="email" class="input-label">Email address</label>
          <input
            id="email"
            type="email"
            formControlName="email"
            placeholder="your@email.com"
            [class]="hasError('email') ? 'input-error' : 'input'"
          />
          <p *ngIf="hasError('email')" class="input-error-msg">Please enter a valid email address</p>
        </div>

        <button
          type="submit"
          [disabled]="forgotForm.invalid || isLoading"
          class="btn-primary w-full py-3"
        >
          {{ isLoading ? 'Sending link…' : 'Send Reset Link' }}
        </button>
      </form>

      <p class="mt-6 text-center text-sm text-gray-500 dark:text-gray-400">
        Remembered your password?
        <a routerLink="/auth/login" class="font-semibold text-primary-600 hover:text-primary-700 ml-1">Return to login</a>
      </p>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordPageComponent {
  forgotForm: FormGroup;
  isLoading = false;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private cdr: ChangeDetectorRef
  ) {
    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  hasError(field: string): boolean {
    const c = this.forgotForm.get(field);
    return !!(c && c.invalid && c.touched);
  }

  onSubmit(): void {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.authService.forgotPassword(this.forgotForm.value.email).subscribe({
      next: () => {
        this.isLoading = false;
        this.submitted = true;
        this.notificationService.success('Email Sent', 'Instructions sent to your email address.');
        this.cdr.markForCheck();
      },
      error: () => {
        // AuthService already handles errors silently — show success regardless
        this.isLoading = false;
        this.submitted = true;
        this.cdr.markForCheck();
      }
    });
  }
}
