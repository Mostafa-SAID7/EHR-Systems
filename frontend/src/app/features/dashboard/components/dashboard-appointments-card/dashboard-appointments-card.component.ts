import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface TodayAppointment {
  patient: string;
  type: string;
  time: string;
  urgent: boolean;
}

@Component({
  selector: 'app-dashboard-appointments-card',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="card">
      <div class="card-header">
        <h2 class="heading-sm">Today's Appointments</h2>
        <a routerLink="/appointments" class="link-primary">
          View all
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
          </svg>
        </a>
      </div>
      <div class="space-y-1 mt-1">
        <div *ngFor="let appt of appointments; let i = index"
          class="data-row group"
          [style.animation-delay]="i * 60 + 'ms'">
          <div class="w-2 h-2 rounded-full bg-primary-500 shrink-0 animate-pulse-soft"></div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-semibold text-gray-900 dark:text-white truncate">{{ appt.patient }}</p>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">{{ appt.type }}</p>
          </div>
          <div class="flex items-center gap-2 shrink-0">
            <span [ngClass]="appt.urgent ? 'badge-danger' : 'badge-primary'">{{ appt.time }}</span>
          </div>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardAppointmentsCardComponent {
  @Input() appointments: TodayAppointment[] = [];
}
