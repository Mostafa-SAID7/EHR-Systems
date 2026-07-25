import {
  Component, Input, Output, EventEmitter,
  ChangeDetectionStrategy, HostListener
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate, group } from '@angular/animations';

/**
 * Modal Component — cinematic backdrop + scale-in dialog
 */
@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      *ngIf="open"
      @backdrop
      class="fixed inset-0 z-50 flex items-end sm:items-center justify-center p-4 sm:p-0"
      (click)="onBackdropClick()"
    >
      <!-- Blurred backdrop -->
      <div class="absolute inset-0 bg-black/50 backdrop-blur-sm"></div>

      <!-- Dialog -->
      <div
        @dialog
        class="relative bg-white dark:bg-surface-800
               rounded-2xl sm:rounded-3xl
               shadow-2xl
               border border-surface-200 dark:border-surface-700
               w-full max-w-md
               mx-auto
               overflow-hidden"
        [style.max-width]="maxWidth"
        (click)="$event.stopPropagation()"
      >
        <!-- Header -->
        <div class="flex items-center justify-between px-6 py-5 border-b border-surface-200 dark:border-surface-700">
          <h2 class="text-lg font-semibold text-gray-900 dark:text-white">{{ title }}</h2>
          <button
            (click)="onClose()"
            class="flex items-center justify-center w-8 h-8 rounded-xl text-gray-400
                   hover:text-gray-600 hover:bg-surface-100
                   dark:hover:text-gray-200 dark:hover:bg-surface-700
                   transition-all duration-150"
            aria-label="Close"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <!-- Content -->
        <div class="px-6 py-5">
          <ng-content></ng-content>
        </div>

        <!-- Footer -->
        <div class="flex justify-end gap-3 px-6 py-4 bg-surface-50 dark:bg-surface-900/50 border-t border-surface-200 dark:border-surface-700">
          <button
            (click)="onClose()"
            class="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300
                   bg-white dark:bg-surface-800
                   border border-surface-200 dark:border-surface-600
                   rounded-xl hover:bg-surface-50 dark:hover:bg-surface-700
                   transition-all duration-200"
          >
            {{ cancelLabel }}
          </button>
          <button
            (click)="onConfirm()"
            class="px-4 py-2 text-sm font-semibold text-white
                   bg-primary-600 hover:bg-primary-700 active:bg-primary-800
                   rounded-xl shadow-sm hover:shadow-md
                   transition-all duration-200"
          >
            {{ confirmLabel }}
          </button>
        </div>
      </div>
    </div>
  `,
  animations: [
    trigger('backdrop', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms ease-out', style({ opacity: 1 })),
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ opacity: 0 })),
      ]),
    ]),
    trigger('dialog', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.94) translateY(8px)' }),
        animate('280ms cubic-bezier(0.34, 1.56, 0.64, 1)',
          style({ opacity: 1, transform: 'scale(1) translateY(0)' })),
      ]),
      transition(':leave', [
        animate('180ms cubic-bezier(0.4, 0, 1, 1)',
          style({ opacity: 0, transform: 'scale(0.96) translateY(4px)' })),
      ]),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModalComponent {
  @Input() open = false;
  @Input() title = 'Modal';
  @Input() confirmLabel = 'Confirm';
  @Input() cancelLabel = 'Cancel';
  @Input() closeOnBackdrop = true;
  @Input() maxWidth = '28rem';

  @Output() openChange = new EventEmitter<boolean>();
  @Output() confirm    = new EventEmitter<void>();
  @Output() close      = new EventEmitter<void>();

  onConfirm(): void { this.confirm.emit(); this.setOpen(false); }
  onClose():   void { this.close.emit();   this.setOpen(false); }

  onBackdropClick(): void {
    if (this.closeOnBackdrop) this.onClose();
  }

  private setOpen(v: boolean): void {
    this.open = v;
    this.openChange.emit(v);
  }

  @HostListener('keydown.escape')
  onEscape(): void { if (this.closeOnBackdrop) this.onClose(); }
}
