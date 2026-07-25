import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface TimelineEvent {
  id: string;
  title: string;
  description?: string;
  details?: string;
  timestamp: Date;
  color?: 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'neutral';
  iconPath?: string;   // SVG path d= (no emoji)
}

// Default SVG icon paths for each color category
const DEFAULT_ICONS: Record<string, string> = {
  primary: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2',
  success: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z',
  warning: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',
  danger:  'M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636',
  info:    'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
  neutral: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z',
};

@Component({
  selector: 'app-timeline',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-0 stagger">
      <div *ngFor="let event of events; let last = last" class="relative flex gap-4">

        <!-- Left: dot + connector -->
        <div class="flex flex-col items-center shrink-0">
          <div [ngClass]="getDotClasses(event.color)"
            class="relative z-10 flex items-center justify-center
                   w-9 h-9 rounded-xl shadow-sm text-white shrink-0">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
                [attr.d]="event.iconPath || getDefaultIcon(event.color)"/>
            </svg>
          </div>
          <!-- Fading connector — no hard border lines -->
          <div *ngIf="!last"
            class="w-px flex-1 my-1.5 bg-gradient-to-b
                   from-surface-200 via-surface-100 to-transparent
                   dark:from-surface-700 dark:via-surface-800 dark:to-transparent">
          </div>
        </div>

        <!-- Right: content -->
        <div class="flex-1 pb-5 min-w-0">
          <div class="card p-4 hover:shadow-card-hover transition-shadow duration-200">
            <div class="flex items-start justify-between gap-3 mb-1">
              <h4 class="text-sm font-semibold text-gray-900 dark:text-white leading-snug">
                {{ event.title }}
              </h4>
              <time class="text-2xs text-gray-400 dark:text-gray-500 whitespace-nowrap shrink-0 mt-0.5">
                {{ event.timestamp | date:'MMM d, h:mm a' }}
              </time>
            </div>

            <p *ngIf="event.description"
              class="text-sm text-gray-600 dark:text-gray-400 leading-relaxed">
              {{ event.description }}
            </p>

            <div *ngIf="event.details"
              class="mt-3 px-3 py-2.5 bg-surface-50 dark:bg-surface-900/60
                     rounded-xl text-xs text-gray-600 dark:text-gray-400 leading-relaxed">
              {{ event.details }}
            </div>
          </div>
        </div>
      </div>

      <!-- Empty state -->
      <div *ngIf="events.length === 0" class="empty-state">
        <div class="empty-icon">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
          </svg>
        </div>
        <h3>No events yet</h3>
        <p>Events will appear here as they are recorded.</p>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TimelineComponent {
  @Input() events: TimelineEvent[] = [];

  getDotClasses(color?: string) {
    return {
      'bg-primary-500 dark:bg-primary-600': !color || color === 'primary',
      'bg-green-500   dark:bg-green-600':   color === 'success',
      'bg-amber-500   dark:bg-amber-600':   color === 'warning',
      'bg-red-500     dark:bg-red-600':     color === 'danger',
      'bg-blue-500    dark:bg-blue-600':    color === 'info',
      'bg-gray-400    dark:bg-gray-500':    color === 'neutral',
    };
  }

  getDefaultIcon(color?: string): string {
    return DEFAULT_ICONS[color || 'primary'] || DEFAULT_ICONS['primary'];
  }
}
