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
  templateUrl: './login-page.component.html',
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

  loginWithGoogle(): void {
    this.authService.externalLogin('Google', 'google-oauth-token', 'google.user@ehr.com', 'Google', 'User').subscribe({
      next: () => this.notificationService.success('OAuth Success', 'Signed in with Google')
    });
  }

  loginWithFacebook(): void {
    this.authService.externalLogin('Facebook', 'facebook-oauth-token', 'fb.user@ehr.com', 'Facebook', 'User').subscribe({
      next: () => this.notificationService.success('OAuth Success', 'Signed in with Facebook')
    });
  }
}
