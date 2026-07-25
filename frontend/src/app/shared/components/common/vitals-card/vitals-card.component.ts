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

// Trend SVG paths
const TREND_PATHS: Record<string, string> = {
  up:     'M5 15l7-7 7 7',
  down:   'M19 9l-7 7-7-7',
  stable: 'M5 12h14',
};

@Component({
  selector: 'app-vitals-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 stagger">
      <div
        *ngFor="let vital of vitals"
        [ngClass]="getCardClasses(vital.status)"
        class="relative overflow-hidden rounded-2xl p-4
               transition-all duration-300 hover:shadow-md hover:-translate-y-0.5 cursor-default"
      >
        <!-- Ambient glow blob — no hard edges -->
        <div [ngClass]="getGlowClasses(vital.status)"
          class="absolute -top-6 -right-6 w-24 h-24 rounded-full
                 opacity-20 blur-2xl pointer-events-none"></div>

        <!-- Header -->
        <div class="flex items-start justify-between mb-3">
          <div>
            <p class="text-xs font-bold uppercase tracking-widest opacity-70">{{ vital.name }}</p>
            <p class="text-2xs opacity-50 mt-0.5 font-medium">{{ vital.unit }}</p>
          </div>
          <div class="flex items-center gap-2">
            <!-- Status dot -->
            <span [ngClass]="getDotClasses(vital.status)"
              class="w-2 h-2 rounded-full animate-pulse-soft shrink-0"></span>
            <!-- Trend SVG icon -->
            <svg *ngIf="vital.trend"
              [ngClass]="getTrendClasses(vital.trend)"
              class="w-3.5 h-3.5"
              fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5"
                [attr.d]="getTrendPath(vital.trend)"/>
            </svg>
          </div>
        </div>

        <!-- Value -->
        <div class="mb-4">
          <span [ngClass]="getValueClasses(vital.status)"
            class="text-3xl font-bold tabular-nums tracking-tight">
            {{ vital.value }}
          </span>
        </div>

        <!-- Progress bar -->
        <div class="mb-3">
          <div class="w-full h-1.5 bg-black/10 dark:bg-white/10 rounded-full overflow-hidden">
            <div [ngClass]="getBarClasses(vital.status)"
              [style.width.%]="getProgress(vital)"
              class="h-full rounded-full transition-all duration-700 ease-smooth"></div>
          </div>
        </div>

        <!-- Status + range -->
        <div class="flex items-center justify-between">
          <span [ngClass]="getBadgeClasses(vital.status)"
            class="px-2.5 py-0.5 rounded-full text-2xs font-bold capitalize">
            {{ vital.status }}
          </span>
          <span class="text-2xs opacity-50 font-medium">
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
      'bg-primary-50  dark:bg-primary-900/20  border border-primary-200/60 dark:border-primary-800/40': status === 'normal',
      'bg-amber-50    dark:bg-amber-900/20    border border-amber-200/60  dark:border-amber-800/40':    status === 'warning',
      'bg-red-50      dark:bg-red-900/20      border border-red-200/60    dark:border-red-800/40':      status === 'critical',
    };
  }

  getGlowClasses(status: string) {
    return {
      'bg-primary-400': status === 'normal',
      'bg-amber-400':   status === 'warning',
      'bg-red-400':     status === 'critical',
    };
  }

  getDotClasses(status: string) {
    return {
      'bg-primary-500': status === 'normal',
      'bg-amber-500':   status === 'warning',
      'bg-red-500':     status === 'critical',
    };
  }

  getValueClasses(status: string) {
    return {
      'text-primary-700 dark:text-primary-300': status === 'normal',
      'text-amber-700   dark:text-amber-300':   status === 'warning',
      'text-red-700     dark:text-red-300':     status === 'critical',
    };
  }

  getBarClasses(status: string) {
    return {
      'bg-primary-500': status === 'normal',
      'bg-amber-500':   status === 'warning',
      'bg-red-500':     status === 'critical',
    };
  }

  getBadgeClasses(status: string) {
    return {
      'bg-primary-200/60 dark:bg-primary-800/50 text-primary-800 dark:text-primary-200': status === 'normal',
      'bg-amber-200/60   dark:bg-amber-800/50   text-amber-800   dark:text-amber-200':   status === 'warning',
      'bg-red-200/60     dark:bg-red-800/50     text-red-800     dark:text-red-200':     status === 'critical',
    };
  }

  getTrendClasses(trend: string) {
    return {
      'text-red-500':     trend === 'up',
      'text-primary-500': trend === 'down',
      'text-gray-400':    trend === 'stable',
    };
  }

  getTrendPath(trend: string): string {
    return TREND_PATHS[trend] || TREND_PATHS['stable'];
  }

  getProgress(vital: Vital): number {
    const val = typeof vital.value === 'string'
      ? parseFloat(vital.value.split('/')[0])
      : vital.value;
    const range = vital.normal.max - vital.normal.min;
    return Math.min(100, Math.max(0,
      ((val - vital.normal.min) / range) * 100
    ));
  }
}
