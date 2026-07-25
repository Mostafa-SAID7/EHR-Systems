import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';

@Component({
  selector: 'app-login-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="stagger">
      <div class="mb-7">
        <h2 class="heading-lg mb-1">Welcome back</h2>
        <p class="body-text">Sign in to your EHR account</p>
      </div>

      <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="space-y-4">

        <!-- Email -->
        <div class="form-field">
          <label for="email" class="input-label">Email address</label>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 flex items-center pl-3.5 pointer-events-none">
              <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207"/>
              </svg>
            </div>
            <input
              id="email"
              type="email"
              formControlName="email"
              placeholder="your@email.com"
              [class]="hasError('email') ? 'input-icon-error' : 'input-icon'"
            />
          </div>
          <p *ngIf="hasError('email')" class="input-error-msg">
            Please enter a valid email address
          </p>
        </div>

        <!-- Password -->
        <div class="form-field">
          <div class="flex items-center justify-between">
            <label for="password" class="input-label mb-0">Password</label>
            <a routerLink="/auth/forgot-password"
              class="text-xs font-medium text-primary-600 hover:text-primary-700
                     dark:text-primary-400 dark:hover:text-primary-300 transition-colors">
              Forgot password?
            </a>
          </div>
          <div class="relative">
            <div class="absolute inset-y-0 left-0 flex items-center pl-3.5 pointer-events-none">
              <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"/>
              </svg>
            </div>
            <input
              id="password"
              [type]="showPassword ? 'text' : 'password'"
              formControlName="password"
              placeholder="••••••••"
              [class]="hasError('password') ? 'input-icon-error' : 'input-icon'"
            />
            <button
              type="button"
              (click)="showPassword = !showPassword"
              class="absolute inset-y-0 right-0 flex items-center pr-3.5
                     text-gray-400 hover:text-gray-600 dark:hover:text-gray-200
                     transition-colors"
              [attr.aria-label]="showPassword ? 'Hide password' : 'Show password'"
            >
              <svg *ngIf="!showPassword" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7
                     -1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"/>
              </svg>
              <svg *ngIf="showPassword" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7
                     a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242
                     M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0
                     A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7
                     a10.025 10.025 0 01-4.132 5.411m0 0L21 21"/>
              </svg>
            </button>
          </div>
          <p *ngIf="hasError('password')" class="input-error-msg">
            Password must be at least 6 characters
          </p>
        </div>

        <!-- Remember me -->
        <div class="flex items-center gap-2.5">
          <input
            id="remember"
            type="checkbox"
            formControlName="rememberMe"
            class="w-4 h-4 rounded accent-primary-600 cursor-pointer"
          />
          <label for="remember" class="text-sm text-gray-600 dark:text-gray-400 cursor-pointer select-none">
            Keep me signed in for 30 days
          </label>
        </div>

        <!-- Server error -->
        <div *ngIf="serverError" class="alert-error animate-scale-in">
          <svg class="w-4 h-4 text-red-500 shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <span class="text-sm">{{ serverError }}</span>
        </div>

        <!-- Submit -->
        <button
          type="submit"
          [disabled]="loginForm.invalid || isLoading"
          class="btn-primary w-full py-3"
        >
          <svg *ngIf="isLoading" class="w-4 h-4 animate-spin-slow" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3"/>
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4a4 4 0 00-4 4H4z"/>
          </svg>
          {{ isLoading ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>

      <p class="mt-6 text-center text-sm text-gray-500 dark:text-gray-400">
        Don't have an account?
        <a routerLink="/auth/register"
          class="font-semibold text-primary-600 hover:text-primary-700
                 dark:text-primary-400 dark:hover:text-primary-300 transition-colors ml-1">
          Create one
        </a>
      </p>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPageComponent implements OnInit, OnDestroy {
  loginForm: FormGroup;
  isLoading = false;
  showPassword = false;
  serverError = '';

  private destroy$ = new Subject<void>();

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {
    this.loginForm = this.fb.group({
      email:      ['', [Validators.required, Validators.email]],
      password:   ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false],
    });
  }

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  hasError(field: string): boolean {
    const c = this.loginForm.get(field);
    return !!(c && c.invalid && c.touched);
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    this.isLoading = true;
    this.serverError = '';
    const { email, password, rememberMe } = this.loginForm.value;

    this.authService
      .login({ email, password, rememberMe })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.notificationService.success('Success', 'Logged in successfully');
        },
        error: (err: any) => {
          this.isLoading = false;
          this.serverError = err?.error?.message || 'Login failed. Please check your credentials.';
          this.cdr.markForCheck();
        },
      });
  }
}
