import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface Vital {
  name: string;
  value: number | string;
  unit: string;
  normal: { min: number; max: number };
  status: 'normal' | 'warning' | 'critical';
  timestamp: Date;
  trend?: 'up' | 'down' | 'stable';
  icon?: string;
}

/**
 * Vitals Card — cinematic status cards, no generic border patterns
 */
@Component({
  selector: 'app-vitals-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 stagger">
      <div
        *ngFor="let vital of vitals; let i = index"
        [ngClass]="getCardClasses(vital.status)"
        class="relative overflow-hidden rounded-2xl p-4 transition-all duration-300 hover:shadow-md hover:-translate-y-0.5"
      >
        <!-- Decorative circle -->
        <div [ngClass]="getGlowClasses(vital.status)"
          class="absolute -top-4 -right-4 w-20 h-20 rounded-full opacity-10 blur-xl pointer-events-none">
        </div>

        <!-- Header -->
        <div class="flex items-start justify-between mb-3">
          <div>
            <p class="text-xs font-semibold uppercase tracking-wider opacity-70">{{ vital.name }}</p>
            <p class="text-2xs opacity-50 mt-0.5">{{ vital.unit }}</p>
          </div>
          <div class="flex items-center gap-1.5">
            <!-- Status dot -->
            <span [ngClass]="getDotClasses(vital.status)"
              class="w-2 h-2 rounded-full animate-pulse-soft"></span>
            <!-- Trend -->
            <span *ngIf="vital.trend" [ngClass]="getTrendClasses(vital.trend)"
              class="text-xs font-bold">
              {{ getTrendIcon(vital.trend) }}
            </span>
          </div>
        </div>

        <!-- Value -->
        <div class="mb-3">
          <span [ngClass]="getValueClasses(vital.status)"
            class="text-3xl font-bold tabular-nums tracking-tight">
            {{ vital.value }}
          </span>
        </div>

        <!-- Status badge + range -->
        <div class="flex items-center justify-between">
          <span [ngClass]="getBadgeClasses(vital.status)"
            class="px-2 py-0.5 rounded-full text-2xs font-semibold capitalize">
            {{ vital.status }}
          </span>
          <span class="text-2xs opacity-50">
            {{ vital.normal.min }}–{{ vital.normal.max }}
          </span>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VitalsCardComponent {
  @Input() vitals: Vital[] = [];

  getCardClasses(status: string) {
    return {
      'bg-primary-50  dark:bg-primary-900/20  border border-primary-200 dark:border-primary-800/50': status === 'normal',
      'bg-yellow-50   dark:bg-yellow-900/20   border border-yellow-200 dark:border-yellow-800/50':   status === 'warning',
      'bg-red-50      dark:bg-red-900/20      border border-red-200   dark:border-red-800/50':        status === 'critical',
    };
  }

  getGlowClasses(status: string) {
    return {
      'bg-primary-500': status === 'normal',
      'bg-yellow-500':  status === 'warning',
      'bg-red-500':     status === 'critical',
    };
  }

  getDotClasses(status: string) {
    return {
      'bg-primary-500': status === 'normal',
      'bg-yellow-500':  status === 'warning',
      'bg-red-500':     status === 'critical',
    };
  }

  getValueClasses(status: string) {
    return {
      'text-primary-700 dark:text-primary-300': status === 'normal',
      'text-yellow-700  dark:text-yellow-300':  status === 'warning',
      'text-red-700     dark:text-red-300':     status === 'critical',
    };
  }

  getBadgeClasses(status: string) {
    return {
      'bg-primary-200/70 dark:bg-primary-800/50 text-primary-800 dark:text-primary-200': status === 'normal',
      'bg-yellow-200/70  dark:bg-yellow-800/50  text-yellow-800  dark:text-yellow-200':  status === 'warning',
      'bg-red-200/70     dark:bg-red-800/50     text-red-800     dark:text-red-200':     status === 'critical',
    };
  }

  getTrendClasses(trend: string) {
    return {
      'text-red-500':     trend === 'up',
      'text-primary-500': trend === 'down',
      'text-gray-400':    trend === 'stable',
    };
  }

  getTrendIcon(trend: string): string {
    return trend === 'up' ? '↑' : trend === 'down' ? '↓' : '→';
  }
}
