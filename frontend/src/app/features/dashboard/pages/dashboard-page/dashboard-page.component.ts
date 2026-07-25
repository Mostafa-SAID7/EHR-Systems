import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';
import { VitalsCardComponent, Vital } from '../../../../shared/components/common/vitals-card/vitals-card.component';
import { TimelineComponent, TimelineEvent } from '../../../../shared/components/common/timeline/timeline.component';

/**
 * Dashboard Page — showcase of the new design system
 */
@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterModule, CardComponent, VitalsCardComponent, TimelineComponent],
  template: `
    <div class="space-y-6 stagger">

      <!-- Welcome -->
      <div>
        <h1 class="heading-xl">Good morning 👋</h1>
        <p class="body-text mt-1">Here's what's happening at your practice today.</p>
      </div>

      <!-- Stat cards -->
      <div class="grid-stats">
        <div *ngFor="let stat of stats; let i = index"
          class="stat-card animate-count-up"
          [style.animation-delay]="i * 60 + 'ms'">
          <div class="flex items-start justify-between">
            <div>
              <p class="stat-label">{{ stat.label }}</p>
              <p class="stat-value mt-1">{{ stat.value }}</p>
            </div>
            <div [ngClass]="stat.iconBg"
              class="w-11 h-11 rounded-2xl flex items-center justify-center text-xl shadow-sm shrink-0">
              {{ stat.icon }}
            </div>
          </div>
          <p [ngClass]="stat.changePositive ? 'stat-change positive' : 'stat-change negative'"
            class="mt-2 text-xs">
            {{ stat.change }} vs. last week
          </p>
        </div>
      </div>

      <!-- Two-col layout -->
      <div class="grid-2">

        <!-- Upcoming appointments -->
        <app-card title="Today's Appointments" variant="default">
          <div card-actions>
            <a routerLink="/appointments"
              class="text-xs font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 transition-colors">
              View all →
            </a>
          </div>

          <div class="space-y-2 mt-1">
            <div *ngFor="let appt of appointments"
              class="flex items-center gap-3 p-3 rounded-xl
                     hover:bg-surface-50 dark:hover:bg-surface-900/50
                     transition-colors duration-150 cursor-pointer group">
              <div class="w-2 h-2 rounded-full bg-primary-500 shrink-0 animate-pulse-soft"></div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900 dark:text-white truncate">{{ appt.patient }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">{{ appt.type }}</p>
              </div>
              <span class="text-xs font-semibold text-primary-600 dark:text-primary-400 shrink-0">{{ appt.time }}</span>
            </div>
          </div>
        </app-card>

        <!-- Quick stats / recent -->
        <app-card title="Recent Activity">
          <app-timeline [events]="recentActivity"></app-timeline>
        </app-card>
      </div>

      <!-- Vitals section -->
      <div>
        <div class="section-header">
          <h2>Last Patient Vitals</h2>
          <a routerLink="/clinical/vitals"
            class="text-sm font-medium text-primary-600 hover:text-primary-700 dark:text-primary-400 transition-colors">
            View details →
          </a>
        </div>
        <app-vitals-card [vitals]="sampleVitals"></app-vitals-card>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardPageComponent implements OnInit {
  currentUser = this.authService.getCurrentUser();

  stats = [
    { label: 'Total Patients',      value: '1,234', icon: '👥', iconBg: 'bg-primary-50 dark:bg-primary-900/30', change: '+12',  changePositive: true },
    { label: 'Appointments Today',  value: '18',    icon: '📅', iconBg: 'bg-blue-50   dark:bg-blue-900/30',    change: '+3',   changePositive: true },
    { label: 'Pending Orders',      value: '8',     icon: '🔬', iconBg: 'bg-yellow-50 dark:bg-yellow-900/30',  change: '-2',   changePositive: true },
    { label: 'Prescriptions',       value: '3',     icon: '💊', iconBg: 'bg-red-50    dark:bg-red-900/30',     change: '+1',   changePositive: false },
  ];

  appointments = [
    { patient: 'Sarah Johnson',  type: 'General Checkup',    time: '9:00 AM' },
    { patient: 'Michael Chen',   type: 'Follow-up Visit',    time: '10:30 AM' },
    { patient: 'Emma Williams',  type: 'Lab Results Review',  time: '11:00 AM' },
    { patient: 'Robert Davis',   type: 'Cardiology Consult',  time: '2:00 PM' },
  ];

  recentActivity: TimelineEvent[] = [
    { id: '1', title: 'Lab results received — M. Chen',      color: 'success', icon: '🔬', timestamp: new Date(Date.now() - 20 * 60000) },
    { id: '2', title: 'Prescription sent — E. Williams',     color: 'primary', icon: '💊', timestamp: new Date(Date.now() - 55 * 60000) },
    { id: '3', title: 'Appointment scheduled — R. Davis',    color: 'info',    icon: '📅', timestamp: new Date(Date.now() - 90 * 60000) },
    { id: '4', title: 'Allergy alert updated — S. Johnson',  color: 'warning', icon: '⚠️', timestamp: new Date(Date.now() - 3 * 3600000) },
  ];

  sampleVitals: Vital[] = [
    { name: 'Heart Rate',       value: 72,   unit: 'bpm',    normal: { min: 60,  max: 100  }, status: 'normal',   trend: 'stable',   timestamp: new Date() },
    { name: 'Blood Pressure',   value: '118/76', unit: 'mmHg', normal: { min: 90, max: 130 }, status: 'normal',   trend: 'down',     timestamp: new Date() },
    { name: 'Temperature',      value: 37.2, unit: '°C',     normal: { min: 36.1, max: 37.2 }, status: 'normal', trend: 'stable',   timestamp: new Date() },
    { name: 'SpO₂',            value: 97,   unit: '%',      normal: { min: 95,  max: 100  }, status: 'normal',   trend: 'stable',   timestamp: new Date() },
  ];

  constructor(private authService: AuthService) {}
  ngOnInit(): void {}
}
