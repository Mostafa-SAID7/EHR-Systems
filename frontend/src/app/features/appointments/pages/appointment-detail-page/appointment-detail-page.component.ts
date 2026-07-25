import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AppointmentNotesCardComponent } from '../../components/appointment-notes-card/appointment-notes-card.component';
import { AppointmentVitalsCardComponent } from '../../components/appointment-vitals-card/appointment-vitals-card.component';

@Component({
  selector: 'app-appointment-detail-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    AppointmentNotesCardComponent,
    AppointmentVitalsCardComponent,
  ],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ───────────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div class="flex items-center gap-3">
          <a routerLink="/appointments" class="btn-icon-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
            </svg>
          </a>
          <div>
            <h1 class="heading-xl">Appointment Detail</h1>
            <p class="body-text mt-0.5">APT-{{ appt.id }} &middot; {{ appt.date | date:'EEEE, MMMM d, y' }}</p>
          </div>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <span [ngClass]="statusClass(appt.status)" class="badge">{{ appt.status }}</span>
          <button class="btn-secondary btn-sm">Reschedule</button>
          <button class="btn-danger btn-sm" *ngIf="appt.status === 'Scheduled'">Cancel</button>
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

        <!-- Main info -->
        <div class="lg:col-span-2 space-y-5">

          <!-- Patient card -->
          <div class="card">
            <div class="flex items-center gap-4">
              <div class="avatar-custom-lg" [style.background]="appt.color">{{ appt.initials }}</div>
              <div class="flex-1 min-w-0">
                <p class="text-base font-bold text-gray-900 dark:text-white">{{ appt.patient }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">MRN {{ appt.mrn }} &middot; {{ appt.gender }}, {{ appt.age }} yrs</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">{{ appt.phone }}</p>
              </div>
              <a [routerLink]="['/patients', appt.patientId]" class="btn-ghost btn-sm shrink-0">
                View Profile
              </a>
            </div>
          </div>

          <!-- Appointment info grid -->
          <div class="card">
            <h2 class="heading-sm mb-4">Appointment Details</h2>
            <div class="grid grid-cols-2 sm:grid-cols-3 gap-4">
              <div *ngFor="let d of details" class="p-3 rounded-xl bg-surface-50 dark:bg-surface-800/60 border border-surface-100 dark:border-surface-700/40">
                <p class="text-2xs font-semibold text-gray-400 uppercase tracking-wider mb-1">{{ d.label }}</p>
                <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ d.value }}</p>
              </div>
            </div>
          </div>

          <!-- Clinical Notes Subcomponent -->
          <app-appointment-notes-card
            [(notes)]="appt.notes"
          ></app-appointment-notes-card>

          <!-- Vitals Subcomponent -->
          <app-appointment-vitals-card
            [vitals]="vitals"
            [show]="appt.status !== 'Scheduled'"
          ></app-appointment-vitals-card>

        </div>

        <!-- Sidebar -->
        <div class="space-y-5">

          <!-- Quick actions -->
          <div class="card space-y-2">
            <h2 class="heading-sm mb-3">Quick Actions</h2>
            <a routerLink="/prescriptions/new" class="flex items-center gap-3 p-3 rounded-xl hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors group">
              <div class="icon-box-sm icon-box-teal">
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
                </svg>
              </div>
              <span class="text-sm font-medium text-gray-700 dark:text-gray-300 group-hover:text-primary-600 transition-colors">Write Prescription</span>
            </a>
            <a routerLink="/lab-results" class="flex items-center gap-3 p-3 rounded-xl hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors group">
              <div class="icon-box-sm icon-box-primary">
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z"/>
                </svg>
              </div>
              <span class="text-sm font-medium text-gray-700 dark:text-gray-300 group-hover:text-primary-600 transition-colors">Order Lab Tests</span>
            </a>
            <a routerLink="/medical-records" class="flex items-center gap-3 p-3 rounded-xl hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors group">
              <div class="icon-box-sm icon-box-amber">
                <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
                </svg>
              </div>
              <span class="text-sm font-medium text-gray-700 dark:text-gray-300 group-hover:text-primary-600 transition-colors">Add to Medical Record</span>
            </a>
          </div>

          <!-- Previous appointments -->
          <div class="card">
            <h2 class="heading-sm mb-3">Previous Visits</h2>
            <div class="space-y-2">
              <div *ngFor="let v of previousVisits" class="flex items-center justify-between p-2.5 rounded-lg hover:bg-primary-50/60 dark:hover:bg-primary-900/10 transition-colors cursor-pointer">
                <div class="min-w-0">
                  <p class="text-xs font-semibold text-gray-900 dark:text-white truncate">{{ v.type }}</p>
                  <p class="text-2xs text-gray-400">{{ v.date }}</p>
                </div>
                <span class="badge-success badge text-2xs shrink-0">{{ v.status }}</span>
              </div>
            </div>
          </div>

        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentDetailPageComponent implements OnInit {
  appt = {
    id: '1042',
    patientId: '1',
    patient: 'Sarah Johnson',
    initials: 'SJ',
    mrn: '00-1234',
    gender: 'Female',
    age: 39,
    phone: '(555) 010-1234',
    color: 'linear-gradient(135deg,#15803d,#16a34a,#4ade80)',
    type: 'Annual Physical Exam',
    doctor: 'Dr. Patel',
    date: new Date(2026, 6, 23, 10, 30),
    duration: 30,
    room: '101',
    status: 'Completed',
    priority: 'Routine',
    notes: 'Patient presents for annual physical. BP slightly elevated at 138/88. HbA1c 7.2% — continues metformin. Dietary counseling given. Return in 3 months.',
  };

  details = [
    { label: 'Visit Type',  value: 'Annual Physical Exam' },
    { label: 'Provider',    value: 'Dr. Patel' },
    { label: 'Time',        value: '10:30 AM' },
    { label: 'Duration',    value: '30 minutes' },
    { label: 'Room',        value: 'Room 101' },
    { label: 'Priority',    value: 'Routine' },
  ];

  vitals = [
    { label: 'Blood Pressure', value: '138/88', unit: 'mmHg' },
    { label: 'Heart Rate',     value: '72',     unit: 'bpm' },
    { label: 'Temperature',    value: '98.6',   unit: '°F' },
    { label: 'Weight',         value: '154',    unit: 'lbs' },
    { label: 'Height',         value: '5\'6"',  unit: '' },
    { label: 'O₂ Sat.',       value: '98',     unit: '%' },
  ];

  previousVisits = [
    { type: 'Diabetes Management Review', date: 'Apr 15, 2026', status: 'Completed' },
    { type: 'Hypertension Follow-up',     date: 'Jan 10, 2026', status: 'Completed' },
    { type: 'Annual Physical',            date: 'Jul 20, 2025', status: 'Completed' },
  ];

  statusClass(s: string): string {
    return s === 'Scheduled' ? 'badge-info' : s === 'In Progress' ? 'badge-primary' : s === 'Completed' ? 'badge-success' : s === 'Cancelled' ? 'badge-danger' : 'badge-neutral';
  }

  ngOnInit(): void {}
}
