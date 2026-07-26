import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="stagger">
      <div class="mb-7">
        <h2 class="heading-lg mb-1">Create an account</h2>
        <p class="body-text">Get started with EHR Platform</p>
      </div>

      <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="space-y-4">
        <!-- Name row -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div class="form-field">
            <label for="firstName" class="input-label">First name</label>
            <input
              id="firstName"
              type="text"
              formControlName="firstName"
              placeholder="John"
              [class]="hasError('firstName') ? 'input-error' : 'input'"
            />
            <p *ngIf="hasError('firstName')" class="input-error-msg">First name is required</p>
          </div>

          <div class="form-field">
            <label for="lastName" class="input-label">Last name</label>
            <input
              id="lastName"
              type="text"
              formControlName="lastName"
              placeholder="Smith"
              [class]="hasError('lastName') ? 'input-error' : 'input'"
            />
            <p *ngIf="hasError('lastName')" class="input-error-msg">Last name is required</p>
          </div>
        </div>

        <!-- Email -->
        <div class="form-field">
          <label for="email" class="input-label">Email address</label>
          <input
            id="email"
            type="email"
            formControlName="email"
            placeholder="your@email.com"
            [class]="hasError('email') ? 'input-error' : 'input'"
          />
          <p *ngIf="hasError('email')" class="input-error-msg">Please enter a valid email</p>
        </div>

        <!-- Password -->
        <div class="form-field">
          <label for="password" class="input-label">Password</label>
          <input
            id="password"
            type="password"
            formControlName="password"
            placeholder="••••••••"
            [class]="hasError('password') ? 'input-error' : 'input'"
          />
          <p *ngIf="hasError('password')" class="input-error-msg">Password must be at least 8 characters</p>
        </div>

        <!-- Server error -->
        <div *ngIf="serverError" class="alert-error animate-scale-in">
          <span class="text-sm">{{ serverError }}</span>
        </div>

        <button
          type="submit"
          [disabled]="registerForm.invalid || isLoading"
          class="btn-primary w-full py-3 mt-2"
        >
          {{ isLoading ? 'Creating account…' : 'Register Account' }}
        </button>
      </form>

      <p class="mt-6 text-center text-sm text-gray-500 dark:text-gray-400">
        Already have an account?
        <a routerLink="/auth/login" class="font-semibold text-primary-600 hover:text-primary-700 ml-1">Sign in</a>
      </p>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterPageComponent {
  registerForm: FormGroup;
  isLoading = false;
  serverError = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
    this.registerForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName:  ['', Validators.required],
      email:     ['', [Validators.required, Validators.email]],
      password:  ['', [Validators.required, Validators.minLength(8)]],
    });
  }

  hasError(field: string): boolean {
    const c = this.registerForm.get(field);
    return !!(c && c.invalid && c.touched);
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.serverError = '';

    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.notificationService.success('Success', 'Account created! Please sign in.');
        this.router.navigate(['/auth/login']);
      },
      error: (err: any) => {
        this.isLoading = false;
        this.serverError = err?.error?.message || 'Registration failed.';
        this.cdr.markForCheck();
      }
    });
  }
}
