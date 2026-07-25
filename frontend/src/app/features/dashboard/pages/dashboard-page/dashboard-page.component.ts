import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';
import { VitalsCardComponent, Vital } from '../../../../shared/components/common/vitals-card/vitals-card.component';
import { TimelineComponent, TimelineEvent } from '../../../../shared/components/common/timeline/timeline.component';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterModule, CardComponent, VitalsCardComponent, TimelineComponent],
  template: `
    <div class="space-y-6 stagger">

      <!-- Welcome banner -->
      <div class="flex items-center justify-between">
        <div>
          <h1 class="heading-xl">Good morning 👋</h1>
          <p class="body-text mt-1">Here's what's happening at your practice today.</p>
        </div>
        <div class="hidden sm:flex items-center gap-2">
          <a routerLink="/appointments/new"
            class="btn-primary text-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
            New Appointment
          </a>
        </div>
      </div>

      <!-- Stat cards -->
      <div class="grid-stats">
        <div *ngFor="let stat of stats; let i = index"
          class="stat-card animate-count-up"
          [style.animation-delay]="i * 60 + 'ms'">
          <div class="flex items-start justify-between gap-2">
            <div class="min-w-0">
              <p class="stat-label">{{ stat.label }}</p>
              <p class="stat-value mt-1">{{ stat.value }}</p>
            </div>
            <div [ngClass]="stat.iconBoxClass"
              class="icon-box-lg shrink-0">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
                  [attr.d]="stat.iconPath"/>
              </svg>
            </div>
          </div>
          <p class="mt-2 text-xs"
            [ngClass]="stat.changePositive ? 'stat-change positive' : 'stat-change negative'">
            <span>{{ stat.changePositive ? '↑' : '↓' }}</span>
            {{ stat.change }} vs. last week
          </p>
        </div>
      </div>

      <!-- Two-col layout -->
      <div class="grid-2">

        <!-- Today's appointments -->
        <app-card title="Today's Appointments" variant="default">
          <div card-actions>
            <a routerLink="/appointments"
              class="text-xs font-medium text-primary-600 hover:text-primary-700
                     dark:text-primary-400 transition-colors">
              View all →
            </a>
          </div>
          <div class="space-y-1.5 mt-2">
            <div *ngFor="let appt of appointments"
              class="data-row group">
              <div class="w-2 h-2 rounded-full bg-primary-500 shrink-0 animate-pulse-soft"></div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-medium text-gray-900 dark:text-white truncate">{{ appt.patient }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">{{ appt.type }}</p>
              </div>
              <div class="flex items-center gap-2 shrink-0">
                <span class="badge-primary text-xs">{{ appt.time }}</span>
              </div>
            </div>
          </div>
        </app-card>

        <!-- Recent activity -->
        <app-card title="Recent Activity">
          <app-timeline [events]="recentActivity"></app-timeline>
        </app-card>
      </div>

      <!-- Quick actions -->
      <div>
        <div class="section-header">
          <h2>Quick Actions</h2>
        </div>
        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
          <a *ngFor="let action of quickActions"
            [routerLink]="action.route"
            class="card-hover flex flex-col items-center justify-center gap-3 py-5 text-center cursor-pointer">
            <div [ngClass]="action.iconBoxClass" class="icon-box-lg">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
                  [attr.d]="action.iconPath"/>
              </svg>
            </div>
            <span class="text-sm font-medium text-gray-700 dark:text-gray-200">{{ action.label }}</span>
          </a>
        </div>
      </div>

      <!-- Vitals -->
      <div>
        <div class="section-header">
          <h2>Last Patient Vitals</h2>
          <a routerLink="/clinical/vitals"
            class="text-sm font-medium text-primary-600 hover:text-primary-700
                   dark:text-primary-400 transition-colors">
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
    {
      label: 'Total Patients',
      value: '1,234',
      iconPath: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z',
      iconBoxClass: 'icon-box-primary',
      change: '+12', changePositive: true,
    },
    {
      label: 'Appointments Today',
      value: '18',
      iconPath: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
      iconBoxClass: 'icon-box-blue',
      change: '+3', changePositive: true,
    },
    {
      label: 'Pending Orders',
      value: '8',
      iconPath: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z',
      iconBoxClass: 'icon-box-amber',
      change: '-2', changePositive: true,
    },
    {
      label: 'Prescriptions',
      value: '3',
      iconPath: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
      iconBoxClass: 'icon-box-red',
      change: '+1', changePositive: false,
    },
  ];

  appointments = [
    { patient: 'Sarah Johnson',  type: 'General Checkup',     time: '9:00 AM' },
    { patient: 'Michael Chen',   type: 'Follow-up Visit',     time: '10:30 AM' },
    { patient: 'Emma Williams',  type: 'Lab Results Review',   time: '11:00 AM' },
    { patient: 'Robert Davis',   type: 'Cardiology Consult',   time: '2:00 PM' },
  ];

  quickActions = [
    {
      label: 'New Patient',
      route: '/patients/new',
      iconPath: 'M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z',
      iconBoxClass: 'icon-box-primary',
    },
    {
      label: 'Schedule',
      route: '/appointments',
      iconPath: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
      iconBoxClass: 'icon-box-blue',
    },
    {
      label: 'Lab Order',
      route: '/clinical/labs',
      iconPath: 'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z',
      iconBoxClass: 'icon-box-amber',
    },
    {
      label: 'Prescribe',
      route: '/prescriptions',
      iconPath: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
      iconBoxClass: 'icon-box-teal',
    },
  ];

  recentActivity: TimelineEvent[] = [
    { id: '1', title: 'Lab results received — M. Chen',     color: 'success', icon: '🔬', timestamp: new Date(Date.now() - 20 * 60000) },
    { id: '2', title: 'Prescription sent — E. Williams',    color: 'primary', icon: '💊', timestamp: new Date(Date.now() - 55 * 60000) },
    { id: '3', title: 'Appointment scheduled — R. Davis',   color: 'info',    icon: '📅', timestamp: new Date(Date.now() - 90 * 60000) },
    { id: '4', title: 'Allergy alert updated — S. Johnson', color: 'warning', icon: '⚠️', timestamp: new Date(Date.now() - 3 * 3600000) },
  ];

  sampleVitals: Vital[] = [
    { name: 'Heart Rate',     value: 72,       unit: 'bpm',  normal: { min: 60,   max: 100 }, status: 'normal', trend: 'stable', timestamp: new Date() },
    { name: 'Blood Pressure', value: '118/76', unit: 'mmHg', normal: { min: 90,   max: 130 }, status: 'normal', trend: 'down',   timestamp: new Date() },
    { name: 'Temperature',    value: 37.2,     unit: '°C',   normal: { min: 36.1, max: 37.2 }, status: 'normal', trend: 'stable', timestamp: new Date() },
    { name: 'SpO₂',          value: 97,        unit: '%',    normal: { min: 95,   max: 100 }, status: 'normal', trend: 'stable', timestamp: new Date() },
  ];

  constructor(private authService: AuthService) {}
  ngOnInit(): void {}
}
