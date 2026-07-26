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
  template: `
    <div class="min-h-[350px] flex items-center justify-center p-6 text-center">
      <div
        class="max-w-md w-full bg-white dark:bg-surface-800 rounded-3xl p-8 shadow-xl border border-red-100 dark:border-red-900/30 animate-scale-in"
      >
        <!-- Icon -->
        <div
          class="w-16 h-16 rounded-2xl bg-red-50 dark:bg-red-900/30 text-red-600 dark:text-red-400 flex items-center justify-center mx-auto mb-5 shadow-sm"
        >
          <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
        </div>

        <!-- Title & Message -->
        <h3 class="text-lg font-bold text-gray-900 dark:text-white">
          {{ title || 'Something went wrong' }}
        </h3>
        <p class="text-xs text-gray-600 dark:text-gray-300 mt-2 leading-relaxed">
          {{ message || 'An unexpected error occurred while rendering this component. Our engineers have been alerted.' }}
        </p>

        <!-- Technical Details Accordion (Toggle) -->
        <div *ngIf="errorDetails" class="mt-4 text-left">
          <button
            (click)="showStack.set(!showStack())"
            class="text-[11px] font-semibold text-gray-500 hover:text-gray-700 dark:hover:text-gray-300 flex items-center gap-1 mx-auto"
          >
            <span>{{ showStack() ? 'Hide Technical Details' : 'Show Technical Details' }}</span>
            <svg
              [class.rotate-180]="showStack()"
              class="w-3.5 h-3.5 transition-transform"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
            </svg>
          </button>

          <div
            *ngIf="showStack()"
            class="mt-2 p-3 bg-surface-100 dark:bg-surface-900 rounded-xl text-[10px] font-mono text-red-600 dark:text-red-400 overflow-x-auto max-h-40 border border-surface-200 dark:border-surface-700"
          >
            <p class="font-bold">{{ errorDetails.name }}: {{ errorDetails.message }}</p>
            <pre *ngIf="errorDetails.stack" class="mt-1 whitespace-pre-wrap leading-tight text-gray-500">{{ errorDetails.stack }}</pre>
          </div>
        </div>

        <!-- Actions -->
        <div class="mt-6 flex flex-col sm:flex-row items-center justify-center gap-3">
          <button (click)="retry.emit()" class="btn-primary btn-sm w-full sm:w-auto px-5 py-2.5">
            <svg class="w-4 h-4 mr-1.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            Try Again
          </button>
          <a routerLink="/dashboard" class="btn-secondary btn-sm w-full sm:w-auto px-5 py-2.5">
            Return to Dashboard
          </a>
        </div>
      </div>
    </div>
  `,
})
export class ErrorBoundaryComponent {
  @Input() title = '';
  @Input() message = '';
  @Input() errorDetails: any = null;

  @Output() retry = new EventEmitter<void>();

  readonly showStack = signal(false);
}
