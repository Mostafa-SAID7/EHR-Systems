import { Component, OnInit, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

interface Patient {
  id: string;
  name: string;
  initials: string;
  dob: string;
  age: number;
  gender: 'Male' | 'Female' | 'Other';
  mrn: string;
  phone: string;
  lastVisit: Date;
  status: 'Active' | 'Inactive' | 'Critical';
  conditions: string[];
  color: string;
}

@Component({
  selector: 'app-patient-list-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Page header ───────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Patients</h1>
          <p class="body-text mt-1">{{ patients.length }} patients registered</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <button class="btn-secondary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
            </svg>
            Export
          </button>
          <a routerLink="/patients/new" class="btn-primary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
            </svg>
            New Patient
          </a>
        </div>
      </div>

      <!-- ── Search + filters ─────────────────────── -->
      <div class="flex flex-col sm:flex-row gap-3">
        <!-- Search -->
        <div class="relative flex-1">
          <div class="absolute inset-y-0 left-0 flex items-center pl-3.5 pointer-events-none">
            <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
          </div>
          <input
            type="text"
            placeholder="Search patients by name, MRN, or condition…"
            [(ngModel)]="searchQuery"
            class="input-icon w-full"
          />
        </div>
        <!-- Filter pills -->
        <div class="flex items-center gap-2">
          <button *ngFor="let f of filters"
            (click)="activeFilter = f"
            [class]="activeFilter === f ? 'filter-pill-active' : 'filter-pill'">
            {{ f }}
          </button>
        </div>
      </div>

      <!-- ── Stats row ────────────────────────────── -->
      <div class="grid-3-stats">
        <div *ngFor="let s of summaryStats"
          class="card-green flex items-center gap-3 py-3">
          <div [ngClass]="s.iconClass" class="icon-box-md shrink-0">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="s.icon"/>
            </svg>
          </div>
          <div>
            <p class="text-lg font-bold text-gray-900 dark:text-white tabular-nums">{{ s.value }}</p>
            <p class="text-2xs text-gray-500 dark:text-gray-400 font-medium">{{ s.label }}</p>
          </div>
        </div>
      </div>

      <!-- ── Patient grid ──────────────────────────── -->
      <div class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        <div *ngFor="let p of filteredPatients()"
          class="card-hover group">
          <div class="flex items-start gap-3">
            <!-- Avatar -->
            <div class="avatar-custom-lg" [style.background]="p.color">
              {{ p.initials }}
            </div>
            <!-- Info -->
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between gap-2">
                <p class="text-sm font-semibold text-gray-900 dark:text-white truncate">{{ p.name }}</p>
                <span [ngClass]="getStatusClass(p.status)" class="badge shrink-0">{{ p.status }}</span>
              </div>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                {{ p.gender }}, {{ p.age }} yrs · MRN {{ p.mrn }}
              </p>
            </div>
          </div>

          <div class="divider my-3"></div>

          <!-- Conditions -->
          <div class="flex flex-wrap gap-1.5 mb-3">
            <span *ngFor="let c of p.conditions" class="badge-neutral text-2xs">{{ c }}</span>
          </div>

          <!-- Footer -->
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-1.5 text-xs text-gray-400 dark:text-gray-500">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
              Last: {{ p.lastVisit | date:'MMM d' }}
            </div>
            <a [routerLink]="['/patients', p.id]" class="link-primary">
              View
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
              </svg>
            </a>
          </div>
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientListPageComponent implements OnInit {
  searchQuery = '';
  activeFilter = 'All';
  filters = ['All', 'Active', 'Critical', 'Inactive'];

  patients: Patient[] = [
    { id: '1', name: 'Sarah Johnson',   initials: 'SJ', dob: '1985-03-12', age: 39, gender: 'Female', mrn: '00-1234', phone: '555-0101', lastVisit: new Date(Date.now() - 2 * 86400000),  status: 'Active',   conditions: ['Hypertension', 'Diabetes'],   color: 'linear-gradient(135deg,#16a34a,#15803d)' },
    { id: '2', name: 'Michael Chen',    initials: 'MC', dob: '1978-07-22', age: 46, gender: 'Male',   mrn: '00-2345', phone: '555-0102', lastVisit: new Date(Date.now() - 1 * 86400000),  status: 'Active',   conditions: ['Asthma'],                     color: 'linear-gradient(135deg,#2563eb,#1d4ed8)' },
    { id: '3', name: 'Emma Williams',   initials: 'EW', dob: '1992-11-05', age: 31, gender: 'Female', mrn: '00-3456', phone: '555-0103', lastVisit: new Date(Date.now() - 7 * 86400000),  status: 'Active',   conditions: ['Migraine', 'Anxiety'],        color: 'linear-gradient(135deg,#7c3aed,#6d28d9)' },
    { id: '4', name: 'Robert Davis',    initials: 'RD', dob: '1965-01-30', age: 59, gender: 'Male',   mrn: '00-4567', phone: '555-0104', lastVisit: new Date(Date.now() - 3 * 86400000),  status: 'Critical', conditions: ['CAD', 'Heart Failure', 'CKD'], color: 'linear-gradient(135deg,#dc2626,#b91c1c)' },
    { id: '5', name: 'Linda Martinez',  initials: 'LM', dob: '1988-09-14', age: 35, gender: 'Female', mrn: '00-5678', phone: '555-0105', lastVisit: new Date(Date.now() - 14 * 86400000), status: 'Active',   conditions: ['Hypothyroidism'],             color: 'linear-gradient(135deg,#0d9488,#0f766e)' },
    { id: '6', name: 'James Wilson',    initials: 'JW', dob: '1971-04-18', age: 53, gender: 'Male',   mrn: '00-6789', phone: '555-0106', lastVisit: new Date(Date.now() - 30 * 86400000), status: 'Inactive', conditions: ['COPD'],                       color: 'linear-gradient(135deg,#d97706,#b45309)' },
  ];

  summaryStats = [
    { value: '1,234', label: 'Total Patients',  icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z', iconClass: 'icon-box-primary icon-box-md' },
    { value: '18',    label: 'New This Month',   icon: 'M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z', iconClass: 'icon-box-teal icon-box-md' },
    { value: '4',     label: 'Critical Status',  icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', iconClass: 'icon-box-red icon-box-md' },
  ];

  filteredPatients(): Patient[] {
    let list = this.patients;
    if (this.activeFilter !== 'All') {
      list = list.filter(p => p.status === this.activeFilter);
    }
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      list = list.filter(p =>
        p.name.toLowerCase().includes(q) ||
        p.mrn.includes(q) ||
        p.conditions.some(c => c.toLowerCase().includes(q))
      );
    }
    return list;
  }

  getStatusClass(status: string): string {
    return status === 'Active' ? 'badge-success' :
           status === 'Critical' ? 'badge-danger' : 'badge-neutral';
  }

  ngOnInit(): void {}
}
