import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface Appointment {
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
  selector: 'app-appointment-list-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ───────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Appointments</h1>
          <p class="body-text mt-1">{{ todayLabel }} — {{ todayCount }} appointments</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <div class="view-toggle">
            <button *ngFor="let v of views"
              (click)="activeView = v.key"
              [class]="activeView === v.key ? 'view-toggle-btn-active' : 'view-toggle-btn'">
              {{ v.label }}
            </button>
          </div>
          <a routerLink="/appointments/new" class="btn-primary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
            New
          </a>
        </div>
      </div>

      <!-- ── Day navigation ────────────────────────── -->
      <div class="flex items-center justify-between">
        <button class="btn-ghost btn-sm">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
          </svg>
          Previous
        </button>
        <div class="flex items-center gap-2">
          <div class="icon-box-sm icon-box-primary">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
            </svg>
          </div>
          <span class="text-sm font-semibold text-gray-900 dark:text-white">{{ todayLabel }}</span>
        </div>
        <button class="btn-ghost btn-sm">
          Next
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
          </svg>
        </button>
      </div>

      <!-- ── Status summary pills ──────────────────── -->
      <div class="grid grid-cols-2 sm:grid-cols-5 gap-3">
        <div *ngFor="let s of statusSummary"
          class="card flex items-center gap-3 p-3 cursor-pointer hover:shadow-card-hover hover:-translate-y-0.5 transition-all duration-200">
          <div class="w-2.5 h-2.5 rounded-full shrink-0" [ngClass]="s.dotClass"></div>
          <div>
            <p class="text-base font-bold text-gray-900 dark:text-white tabular-nums">{{ s.count }}</p>
            <p class="text-2xs text-gray-500 dark:text-gray-400 font-medium">{{ s.label }}</p>
          </div>
        </div>
      </div>

      <!-- ── Appointments list ─────────────────────── -->
      <div class="card p-0 overflow-hidden">
        <div class="card-header">
          <h2 class="heading-sm">Schedule</h2>
          <span class="badge-primary">{{ appointments.length }} total</span>
        </div>

        <div class="divide-y divide-surface-100 dark:divide-surface-700/50">
          <div *ngFor="let a of appointments"
            class="flex items-center gap-4 px-5 py-4
                   hover:bg-primary-50/40 dark:hover:bg-primary-900/10
                   transition-colors duration-150 cursor-pointer">

            <!-- Time block -->
            <div class="w-16 shrink-0 text-center">
              <p class="text-sm font-bold text-gray-900 dark:text-white">{{ a.date | date:'h:mm' }}</p>
              <p class="text-2xs text-gray-400">{{ a.date | date:'a' }}</p>
            </div>

            <!-- Color accent (duration bar) -->
            <div class="w-1 rounded-full shrink-0 self-stretch min-h-[2.5rem]"
              [style.background]="a.color"></div>

            <!-- Avatar -->
            <div class="avatar-custom-md" [style.background]="a.color">
              {{ a.initials }}
            </div>

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

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentListPageComponent implements OnInit {
  activeView = 'day';
  todayLabel = new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });
  todayCount = 8;

  views = [
    { key: 'day',   label: 'Day' },
    { key: 'week',  label: 'Week' },
    { key: 'month', label: 'Month' },
  ];

  statusSummary = [
    { label: 'Scheduled',   count: 5, dotClass: 'bg-blue-500' },
    { label: 'In Progress', count: 1, dotClass: 'bg-primary-500 animate-pulse-soft' },
    { label: 'Completed',   count: 2, dotClass: 'bg-green-500' },
    { label: 'No Show',     count: 0, dotClass: 'bg-gray-400' },
    { label: 'Cancelled',   count: 1, dotClass: 'bg-red-500' },
  ];

  appointments: Appointment[] = [
    { id: '1', patient: 'Sarah Johnson',  initials: 'SJ', type: 'General Checkup',    doctor: 'Dr. Patel',   date: this.today(9,0),  duration: 30, status: 'Completed',  room: '101', color: '#16a34a' },
    { id: '2', patient: 'Michael Chen',   initials: 'MC', type: 'Follow-up Visit',    doctor: 'Dr. Smith',   date: this.today(10,30),duration: 20, status: 'In Progress',room: '102', color: '#2563eb' },
    { id: '3', patient: 'Emma Williams',  initials: 'EW', type: 'Lab Results Review', doctor: 'Dr. Patel',   date: this.today(11,0), duration: 15, status: 'Scheduled',  room: '103', color: '#7c3aed' },
    { id: '4', patient: 'Robert Davis',   initials: 'RD', type: 'Cardiology Consult', doctor: 'Dr. Garcia',  date: this.today(14,0), duration: 45, status: 'Scheduled',  room: '201', color: '#dc2626' },
    { id: '5', patient: 'Linda Martinez', initials: 'LM', type: 'Annual Physical',    doctor: 'Dr. Patel',   date: this.today(15,30),duration: 60, status: 'Scheduled',  room: '101', color: '#0d9488' },
    { id: '6', patient: 'James Wilson',   initials: 'JW', type: 'Follow-up Visit',    doctor: 'Dr. Smith',   date: this.today(16,0), duration: 20, status: 'Cancelled',  room: '104', color: '#d97706' },
  ];

  today(h: number, m: number): Date {
    const d = new Date(); d.setHours(h, m, 0); return d;
  }

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

  ngOnInit(): void {}
}
