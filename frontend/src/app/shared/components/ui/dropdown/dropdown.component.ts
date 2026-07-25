import {
  Component, Input, Output, EventEmitter,
  ChangeDetectionStrategy, HostListener
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';

export interface DropdownOption {
  id: string | number;
  label: string;
  icon?: string;
  divider?: boolean;
  danger?: boolean;
}

/**
 * Dropdown Component — polished menu, no generic border outlines
 */
@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="relative inline-block" #container>
      <button
        (click)="toggleOpen()"
        class="inline-flex items-center gap-2 px-4 py-2
               bg-white dark:bg-surface-800
               border border-surface-200 dark:border-surface-600
               rounded-xl text-sm font-medium text-gray-700 dark:text-gray-200
               hover:bg-surface-50 dark:hover:bg-surface-700
               transition-all duration-200
               shadow-xs"
      >
        <ng-content></ng-content>
        <svg
          class="w-4 h-4 text-gray-400 transition-transform duration-200"
          [class.rotate-180]="isOpen"
          fill="none" stroke="currentColor" viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
        </svg>
      </button>

      <div
        *ngIf="isOpen"
        @dropdownAnim
        class="absolute top-full left-0 mt-2 min-w-[11rem] w-max
               bg-white dark:bg-surface-800
               rounded-2xl shadow-lg
               border border-surface-200 dark:border-surface-700
               z-50 overflow-hidden py-1.5"
      >
        <ng-container *ngFor="let option of options">
          <hr
            *ngIf="option.divider"
            class="my-1.5 border-surface-200 dark:border-surface-700"
          />
          <button
            *ngIf="!option.divider"
            (click)="selectOption(option)"
            [class.text-red-600]="option.danger"
            [class.dark:text-red-400]="option.danger"
            [class.hover:bg-red-50]="option.danger"
            [class.dark:hover:bg-red-900/20]="option.danger"
            [class.text-gray-700]="!option.danger"
            [class.dark:text-gray-200]="!option.danger"
            [class.hover:bg-surface-50]="!option.danger"
            [class.dark:hover:bg-surface-700]="!option.danger"
            class="w-full flex items-center gap-2.5 px-4 py-2
                   text-sm transition-colors duration-150"
          >
            <span *ngIf="option.icon" class="text-base leading-none w-4 text-center">{{ option.icon }}</span>
            {{ option.label }}
          </button>
        </ng-container>
      </div>
    </div>
  `,
  animations: [
    trigger('dropdownAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.96) translateY(-6px)' }),
        animate('200ms cubic-bezier(0.16, 1, 0.3, 1)',
          style({ opacity: 1, transform: 'scale(1) translateY(0)' })),
      ]),
      transition(':leave', [
        animate('130ms ease-in',
          style({ opacity: 0, transform: 'scale(0.96) translateY(-4px)' })),
      ]),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DropdownComponent {
  @Input() options: DropdownOption[] = [];
  @Output() select = new EventEmitter<DropdownOption>();

  isOpen = false;

  toggleOpen(): void { this.isOpen = !this.isOpen; }

  selectOption(option: DropdownOption): void {
    this.select.emit(option);
    this.isOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(e: MouseEvent): void {
    if (!(e.target as HTMLElement).closest('app-dropdown')) this.isOpen = false;
  }
}
