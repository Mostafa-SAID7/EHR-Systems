import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface AppointmentRow {
  id: string;
  patient: string;
  initials: string;
  type: string;
  doctor: string;
  date: Date;
  duration: number;
  status: 'Scheduled' | 'In Progress' | 'Completed' | 'Cancelled' | 'No Show';
  room?: string;
  color: string;
}

@Component({
  selector: 'app-appointment-schedule-table',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="card p-0 overflow-hidden">
      <div class="card-header">
        <h2 class="heading-sm">Schedule</h2>
        <span class="badge-primary">{{ appointments.length }} total</span>
      </div>

      <div class="divide-y divide-surface-100 dark:divide-surface-700/50">
        <div *ngFor="let a of appointments; trackBy: trackById"
          class="flex items-center gap-4 px-5 py-4
                 hover:bg-primary-50/40 dark:hover:bg-primary-900/10
                 transition-colors duration-150 cursor-pointer">

          <!-- Time block -->
          <div class="w-16 shrink-0 text-center">
            <p class="text-sm font-bold text-gray-900 dark:text-white">{{ a.date | date:'h:mm' }}</p>
            <p class="text-2xs text-gray-400">{{ a.date | date:'a' }}</p>
          </div>

          <!-- Color accent bar -->
          <div class="w-1 rounded-full shrink-0 self-stretch min-h-[2.5rem]" [style.background]="a.color"></div>

          <!-- Avatar -->
          <div class="avatar-custom-md" [style.background]="a.color">{{ a.initials }}</div>

          <!-- Info -->
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 flex-wrap">
              <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ a.patient }}</p>
              <span [ngClass]="getStatusClass(a.status)" class="badge">{{ a.status }}</span>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
              {{ a.type }} &middot; {{ a.doctor }} &middot; {{ a.duration }} min
              <span *ngIf="a.room"> &middot; Room {{ a.room }}</span>
            </p>
          </div>

          <!-- Actions -->
          <div class="flex items-center gap-1 shrink-0">
            <button class="btn-icon-sm" title="Start appointment">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z"/>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
            </button>
            <button class="btn-icon-sm" title="More options">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentScheduleTableComponent {
  @Input() appointments: AppointmentRow[] = [];

  trackById(_: number, a: AppointmentRow): string { return a.id; }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      'Scheduled':   'badge-info',
      'In Progress': 'badge-primary',
      'Completed':   'badge-success',
      'Cancelled':   'badge-danger',
      'No Show':     'badge-neutral',
    };
    return map[status] || 'badge-neutral';
  }
}
