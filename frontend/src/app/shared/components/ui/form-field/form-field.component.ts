import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

export type InputType = 'text' | 'email' | 'password' | 'number' | 'date' | 'tel' | 'url' | 'search';

/**
 * Form Field Component — centralised input styling, no duplication
 */
@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="mb-4">
      <label *ngIf="label" [for]="fieldId"
        class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1.5">
        {{ label }}
        <span *ngIf="required" class="text-red-500 ml-0.5">*</span>
      </label>

      <div class="relative">
        <!-- Prefix icon slot -->
        <div *ngIf="prefixIcon"
          class="absolute inset-y-0 left-0 flex items-center pl-3.5 pointer-events-none text-gray-400">
          {{ prefixIcon }}
        </div>

        <input
          [id]="fieldId"
          [type]="type"
          [formControl]="control"
          [placeholder]="placeholder"
          [disabled]="disabled"
          [required]="required"
          [class.pl-10]="prefixIcon"
          [class.pr-10]="suffixIcon"
          [ngClass]="hasError()
            ? 'w-full px-4 py-2.5 bg-white dark:bg-surface-800 border border-red-500 dark:border-red-500 rounded-xl text-sm text-gray-900 dark:text-gray-100 placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-red-400 focus:border-red-500 transition-all duration-200'
            : 'w-full px-4 py-2.5 bg-white dark:bg-surface-800 border border-surface-200 dark:border-surface-600 rounded-xl text-sm text-gray-900 dark:text-gray-100 placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-all duration-200'"
        />

        <!-- Suffix icon slot -->
        <div *ngIf="suffixIcon"
          class="absolute inset-y-0 right-0 flex items-center pr-3.5 pointer-events-none text-gray-400">
          {{ suffixIcon }}
        </div>

        <!-- Valid checkmark -->
        <div *ngIf="!hasError() && control.valid && control.touched && !suffixIcon"
          class="absolute inset-y-0 right-0 flex items-center pr-3.5 text-primary-500 pointer-events-none text-sm">
          ✓
        </div>
      </div>

      <!-- Errors -->
      <div *ngIf="hasError()" class="mt-1.5 space-y-0.5 animate-fade-in">
        <p *ngIf="control.errors?.['required']"   class="text-xs text-red-500">{{ label || 'This field' }} is required</p>
        <p *ngIf="control.errors?.['email']"       class="text-xs text-red-500">Please enter a valid email address</p>
        <p *ngIf="control.errors?.['minlength']"   class="text-xs text-red-500">Minimum {{ control.errors?.['minlength'].requiredLength }} characters required</p>
        <p *ngIf="control.errors?.['maxlength']"   class="text-xs text-red-500">Maximum {{ control.errors?.['maxlength'].requiredLength }} characters allowed</p>
        <p *ngIf="control.errors?.['pattern']"     class="text-xs text-red-500">Invalid format</p>
        <p *ngIf="control.errors?.['min']"         class="text-xs text-red-500">Minimum value is {{ control.errors?.['min'].min }}</p>
        <p *ngIf="control.errors?.['max']"         class="text-xs text-red-500">Maximum value is {{ control.errors?.['max'].max }}</p>
        <p *ngIf="error"                           class="text-xs text-red-500">{{ error }}</p>
      </div>

      <!-- Hint -->
      <p *ngIf="hint && !hasError()" class="mt-1.5 text-xs text-gray-500 dark:text-gray-400">{{ hint }}</p>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormFieldComponent {
  @Input() control!: FormControl;
  @Input() label = '';
  @Input() placeholder = '';
  @Input() type: InputType = 'text';
  @Input() required = false;
  @Input() disabled = false;
  @Input() error = '';
  @Input() hint = '';
  @Input() prefixIcon = '';
  @Input() suffixIcon = '';
  @Input() fieldId = `field-${Math.random().toString(36).substr(2, 9)}`;

  hasError(): boolean {
    return (this.control.invalid && this.control.touched) || !!this.error;
  }
}
