import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-appointment-schedule-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="space-y-6 stagger max-w-3xl">

      <!-- ── Header ───────────────────────────────────── -->
      <div class="flex items-center gap-3">
        <a routerLink="/appointments" class="btn-icon-sm">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
          </svg>
        </a>
        <div>
          <h1 class="heading-xl">Schedule Appointment</h1>
          <p class="body-text mt-0.5">Book a new patient appointment</p>
        </div>
      </div>

      <!-- ── Patient selection ─────────────────────────── -->
      <div class="card space-y-4">
        <h2 class="heading-sm">Patient Information</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Select Patient *</label>
            <select [(ngModel)]="form.patientId" class="input-base w-full">
              <option value="">— Choose patient —</option>
              <option *ngFor="let p of patients" [value]="p.id">{{ p.name }} (MRN: {{ p.mrn }})</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Or Enter MRN</label>
            <input type="text" [(ngModel)]="form.mrn" placeholder="e.g. 00-1234" class="input-base w-full"/>
          </div>
        </div>
      </div>

      <!-- ── Appointment details ───────────────────────── -->
      <div class="card space-y-4">
        <h2 class="heading-sm">Appointment Details</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Visit Type *</label>
            <select [(ngModel)]="form.type" class="input-base w-full">
              <option value="">— Select type —</option>
              <option *ngFor="let t of visitTypes" [value]="t">{{ t }}</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Provider *</label>
            <select [(ngModel)]="form.provider" class="input-base w-full">
              <option value="">— Select provider —</option>
              <option *ngFor="let d of doctors" [value]="d">{{ d }}</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Date *</label>
            <input type="date" [(ngModel)]="form.date" class="input-base w-full"/>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Time *</label>
            <select [(ngModel)]="form.time" class="input-base w-full">
              <option value="">— Select time —</option>
              <option *ngFor="let t of timeSlots" [value]="t">{{ t }}</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Duration</label>
            <select [(ngModel)]="form.duration" class="input-base w-full">
              <option value="15">15 minutes</option>
              <option value="30">30 minutes</option>
              <option value="45">45 minutes</option>
              <option value="60">60 minutes</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Room</label>
            <select [(ngModel)]="form.room" class="input-base w-full">
              <option value="">— Assign later —</option>
              <option *ngFor="let r of rooms" [value]="r">{{ r }}</option>
            </select>
          </div>
        </div>
      </div>

      <!-- ── Priority + notes ──────────────────────────── -->
      <div class="card space-y-4">
        <h2 class="heading-sm">Additional Information</h2>
        <div>
          <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Priority</label>
          <div class="flex gap-2 flex-wrap">
            <button *ngFor="let p of priorities"
              (click)="form.priority = p.key"
              [class]="form.priority === p.key ? p.active : p.inactive"
              class="px-4 py-2 rounded-xl text-sm font-semibold border transition-all">
              {{ p.label }}
            </button>
          </div>
        </div>
        <div>
          <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Visit Reason / Notes</label>
          <textarea [(ngModel)]="form.notes" rows="3" placeholder="Chief complaint, reason for visit, special instructions…"
            class="input-base w-full resize-none"></textarea>
        </div>
        <div class="flex items-center gap-3">
          <label class="flex items-center gap-2.5 cursor-pointer">
            <input type="checkbox" [(ngModel)]="form.sendReminder" class="w-4 h-4 rounded accent-primary-600"/>
            <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Send SMS reminder to patient</span>
          </label>
        </div>
        <div class="flex items-center gap-3">
          <label class="flex items-center gap-2.5 cursor-pointer">
            <input type="checkbox" [(ngModel)]="form.sendEmail" class="w-4 h-4 rounded accent-primary-600"/>
            <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Send email confirmation</span>
          </label>
        </div>
      </div>

      <!-- ── Summary preview ───────────────────────────── -->
      <div *ngIf="form.patientId && form.type && form.date && form.time"
        class="card bg-primary-50/60 dark:bg-primary-950/30 border border-primary-200/60 dark:border-primary-800/40">
        <div class="flex items-center gap-2 mb-3">
          <div class="icon-box-sm icon-box-primary">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
          </div>
          <span class="text-sm font-semibold text-primary-700 dark:text-primary-300">Appointment Preview</span>
        </div>
        <p class="text-sm text-gray-700 dark:text-gray-300">
          <span class="font-semibold">{{ getPatientName() }}</span>
          &middot; {{ form.type }} &middot; {{ form.date }} at {{ form.time }}
          <span *ngIf="form.provider"> &middot; {{ form.provider }}</span>
          &middot; {{ form.duration }} min
        </p>
      </div>

      <!-- ── Actions ──────────────────────────────────── -->
      <div class="flex items-center gap-3 pb-4">
        <button (click)="submit()" class="btn-primary" [disabled]="!isValid()">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>
          </svg>
          Confirm Appointment
        </button>
        <a routerLink="/appointments" class="btn-ghost">Cancel</a>

        <!-- Success toast -->
        <div *ngIf="submitted"
          class="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-primary-100 dark:bg-primary-900/40 text-primary-700 dark:text-primary-300 text-sm font-semibold border border-primary-200/60 dark:border-primary-800/40 animate-fade-in">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          Appointment booked successfully!
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentSchedulePageComponent implements OnInit {
  submitted = false;

  form = {
    patientId: '',
    mrn: '',
    type: '',
    provider: '',
    date: '',
    time: '',
    duration: '30',
    room: '',
    priority: 'routine',
    notes: '',
    sendReminder: true,
    sendEmail: true,
  };

  patients = [
    { id: '1', name: 'Sarah Johnson',  mrn: '00-1234' },
    { id: '2', name: 'Michael Chen',   mrn: '00-2345' },
    { id: '3', name: 'Emma Williams',  mrn: '00-3456' },
    { id: '4', name: 'Robert Davis',   mrn: '00-4567' },
    { id: '5', name: 'Linda Martinez', mrn: '00-5678' },
  ];

  visitTypes = ['General Checkup', 'Follow-up Visit', 'Lab Results Review', 'Cardiology Consult', 'Annual Physical', 'Urgent Care', 'Telehealth Visit', 'Vaccination', 'Mental Health'];
  doctors  = ['Dr. Patel', 'Dr. Smith', 'Dr. Garcia', 'Dr. Johnson', 'Dr. Lee'];
  rooms    = ['Room 101', 'Room 102', 'Room 103', 'Room 104', 'Room 201', 'Room 202', 'Telehealth'];
  timeSlots = ['08:00 AM', '08:30 AM', '09:00 AM', '09:30 AM', '10:00 AM', '10:30 AM', '11:00 AM', '11:30 AM', '01:00 PM', '01:30 PM', '02:00 PM', '02:30 PM', '03:00 PM', '03:30 PM', '04:00 PM', '04:30 PM'];

  priorities = [
    { key: 'routine',   label: '🟢 Routine',   active: 'border-primary-500 bg-primary-50 text-primary-700 dark:bg-primary-900/40 dark:text-primary-300',   inactive: 'border-surface-200 dark:border-surface-600 text-gray-600 dark:text-gray-400 hover:border-primary-300' },
    { key: 'urgent',    label: '🟡 Urgent',    active: 'border-amber-500 bg-amber-50 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',               inactive: 'border-surface-200 dark:border-surface-600 text-gray-600 dark:text-gray-400 hover:border-amber-300' },
    { key: 'emergency', label: '🔴 Emergency', active: 'border-red-500 bg-red-50 text-red-700 dark:bg-red-900/40 dark:text-red-300',                          inactive: 'border-surface-200 dark:border-surface-600 text-gray-600 dark:text-gray-400 hover:border-red-300' },
  ];

  isValid(): boolean {
    return !!(this.form.patientId && this.form.type && this.form.provider && this.form.date && this.form.time);
  }

  getPatientName(): string {
    return this.patients.find(p => p.id === this.form.patientId)?.name || '';
  }

  submit(): void {
    if (!this.isValid()) return;
    this.submitted = true;
    setTimeout(() => this.submitted = false, 3500);
  }

  ngOnInit(): void {}
}
