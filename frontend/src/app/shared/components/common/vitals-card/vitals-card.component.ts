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
  templateUrl: './vitals-card.component.html',
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
