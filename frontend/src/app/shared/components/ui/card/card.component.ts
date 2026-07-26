import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export type CardPadding = 'none' | 'sm' | 'md' | 'lg';
export type CardVariant = 'default' | 'hover' | 'flat' | 'elevated' | 'green';

@Component({
  selector: 'app-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './card.component.html',
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
      default:  'bg-white dark:bg-surface-800 rounded-2xl shadow-card border border-surface-100 dark:border-surface-700/50',
      hover:    'bg-white dark:bg-surface-800 rounded-2xl shadow-card border border-surface-100 dark:border-surface-700/50 cursor-pointer transition-all duration-250 hover:shadow-card-hover hover:-translate-y-0.5 hover:border-primary-200/60 dark:hover:border-primary-700/40',
      flat:     'bg-surface-50 dark:bg-surface-900 rounded-2xl border border-surface-200/80 dark:border-surface-700/60',
      elevated: 'bg-white dark:bg-surface-800 rounded-2xl shadow-lg border border-surface-100 dark:border-surface-700/50',
      green:    'bg-gradient-to-br from-primary-50 to-primary-100/40 dark:from-primary-950/50 dark:to-primary-900/20 rounded-2xl border border-primary-200/50 dark:border-primary-800/30',
    };

    return {
      [variantMap[this.variant]]: true,
      [padMap[this.padding]]:     true,
    };
  }
}
