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
  templateUrl: './reset-password-page.component.html',
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
