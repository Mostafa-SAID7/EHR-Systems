import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'success' | 'warning' | 'ghost' | 'outline';
export type ButtonSize = 'sm' | 'md' | 'lg';

/**
 * Button Component — uses centralised .btn-* classes from styles.scss
 */
@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './button.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ButtonComponent {
  @Input() variant: ButtonVariant = 'primary';
  @Input() size: ButtonSize = 'md';
  @Input() disabled = false;
  @Input() loading = false;
  @Output() clicked = new EventEmitter<void>();

  onClick(): void {
    if (!this.disabled && !this.loading) this.clicked.emit();
  }

  getClasses(): Record<string, boolean> {
    const sizeMap: Record<ButtonSize, string> = {
      sm: 'px-3 py-1.5 text-sm rounded-lg',
      md: 'px-4 py-2 text-sm rounded-xl',
      lg: 'px-6 py-3 text-base rounded-xl',
    };

    const variantMap: Record<ButtonVariant, string> = {
      primary:   'bg-primary-600 text-white hover:bg-primary-700 active:bg-primary-800 shadow-sm hover:shadow-md focus-visible:ring-primary-500',
      secondary: 'bg-surface-100 dark:bg-surface-700 text-gray-800 dark:text-gray-200 hover:bg-surface-200 dark:hover:bg-surface-600 border border-surface-200 dark:border-surface-600 focus-visible:ring-gray-400',
      danger:    'bg-red-600 text-white hover:bg-red-700 active:bg-red-800 shadow-sm hover:shadow-md focus-visible:ring-red-500',
      success:   'bg-primary-600 text-white hover:bg-primary-700 shadow-sm hover:shadow-md focus-visible:ring-primary-500',
      warning:   'bg-yellow-500 text-white hover:bg-yellow-600 shadow-sm hover:shadow-md focus-visible:ring-yellow-400',
      ghost:     'text-gray-700 dark:text-gray-300 hover:bg-surface-100 dark:hover:bg-surface-800 focus-visible:ring-gray-400',
      outline:   'border border-primary-600 text-primary-700 dark:text-primary-400 hover:bg-primary-50 dark:hover:bg-primary-900/20 focus-visible:ring-primary-500',
    };

    const base = 'inline-flex items-center justify-center gap-2 font-semibold transition-all duration-200 focus:outline-none focus-visible:ring-2 focus-visible:ring-offset-2 select-none';

    return {
      [base]: true,
      [sizeMap[this.size]]: true,
      [variantMap[this.variant]]: true,
      'opacity-40 pointer-events-none': this.disabled || this.loading,
    };
  }
}
