import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface TimelineEvent {
  id: string;
  title: string;
  description?: string;
  details?: string;
  timestamp: Date;
  color?: 'primary' | 'success' | 'warning' | 'danger' | 'info' | 'neutral';
  icon?: string;
}

/**
 * Timeline Component — clean vertical flow, no border-l gutter lines
 */
@Component({
  selector: 'app-timeline',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="space-y-0 stagger">
      <div *ngFor="let event of events; let last = last" class="relative flex gap-4">

        <!-- Left: dot + connecting line -->
        <div class="flex flex-col items-center shrink-0">
          <!-- Dot -->
          <div [ngClass]="getDotClasses(event.color)"
            class="relative z-10 flex items-center justify-center w-9 h-9 rounded-xl shadow-sm text-white text-sm shrink-0">
            <span *ngIf="event.icon">{{ event.icon }}</span>
            <svg *ngIf="!event.icon" class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <circle cx="12" cy="12" r="4" stroke-width="2.5"/>
            </svg>
          </div>
          <!-- Connector -->
          <div *ngIf="!last"
            class="w-px flex-1 my-1 bg-gradient-to-b from-surface-200 to-transparent dark:from-surface-700"></div>
        </div>

        <!-- Right: content -->
        <div class="flex-1 pb-6 min-w-0">
          <div class="card p-4 hover:shadow-md transition-shadow duration-200">
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
              class="mt-3 px-3 py-2.5 bg-surface-50 dark:bg-surface-900/60 rounded-xl
                     text-xs text-gray-600 dark:text-gray-400 leading-relaxed">
              {{ event.details }}
            </div>
          </div>
        </div>
      </div>

      <!-- Empty state -->
      <div *ngIf="events.length === 0" class="empty-state">
        <div class="empty-icon">📋</div>
        <p class="empty-title">No events yet</p>
        <p class="empty-body">Events will appear here as they are recorded.</p>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TimelineComponent {
  @Input() events: TimelineEvent[] = [];

  getDotClasses(color?: string) {
    return {
      'bg-primary-500 dark:bg-primary-600':  !color || color === 'primary',
      'bg-green-500   dark:bg-green-600':    color === 'success',
      'bg-yellow-500  dark:bg-yellow-600':   color === 'warning',
      'bg-red-500     dark:bg-red-600':      color === 'danger',
      'bg-blue-500    dark:bg-blue-600':     color === 'info',
      'bg-gray-400    dark:bg-gray-500':     color === 'neutral',
    };
  }
}
