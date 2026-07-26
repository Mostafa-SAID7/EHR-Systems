import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

/**
 * Error Boundary Component
 * Fallback UI component rendered when runtime errors occur.
 */
@Component({
  selector: 'app-error-boundary',
  standalone: true,
  imports: [CommonModule, RouterModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './error-boundary.component.html',
})
export class ErrorBoundaryComponent {
  @Input() title = '';
  @Input() message = '';
  @Input() errorDetails: any = null;

  @Output() retry = new EventEmitter<void>();

  readonly showStack = signal(false);
}
