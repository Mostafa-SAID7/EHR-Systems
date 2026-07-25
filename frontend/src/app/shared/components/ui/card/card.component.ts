import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export type CardPadding = 'none' | 'sm' | 'md' | 'lg';
export type CardVariant = 'default' | 'hover' | 'flat' | 'elevated';

/**
 * Card Component — uses centralised surface/shadow tokens
 */
@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [ngClass]="getClasses()">
      <!-- Optional header -->
      <div *ngIf="title"
        class="flex items-center justify-between pb-4 mb-4 border-b border-surface-200 dark:border-surface-700">
        <h3 class="text-base font-semibold text-gray-900 dark:text-white">{{ title }}</h3>
        <ng-content select="[card-actions]"></ng-content>
      </div>
      <ng-content></ng-content>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CardComponent {
  @Input() title?: string;
  @Input() padding: CardPadding = 'md';
  @Input() variant: CardVariant = 'default';

  getClasses(): Record<string, boolean> {
    const padMap: Record<CardPadding, string> = {
      none: 'p-0',
      sm:   'p-3',
      md:   'p-5',
      lg:   'p-7',
    };

    const variantMap: Record<CardVariant, string> = {
      default:  'bg-white dark:bg-surface-800 rounded-2xl shadow border border-surface-200 dark:border-surface-700',
      hover:    'bg-white dark:bg-surface-800 rounded-2xl shadow border border-surface-200 dark:border-surface-700 transition-all duration-250 hover:shadow-md hover:-translate-y-0.5',
      flat:     'bg-surface-50 dark:bg-surface-900 rounded-2xl border border-surface-200 dark:border-surface-700',
      elevated: 'bg-white dark:bg-surface-800 rounded-2xl shadow-lg border border-surface-200 dark:border-surface-700',
    };

    return {
      [variantMap[this.variant]]: true,
      [padMap[this.padding]]:     true,
    };
  }
}
