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
  templateUrl: './form-field.component.html',
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
