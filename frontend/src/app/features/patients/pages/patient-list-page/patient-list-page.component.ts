import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PatientCardsGridComponent, PatientCard } from '../../components/patient-cards-grid/patient-cards-grid.component';

@Component({
  selector: 'app-patient-list-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, PatientCardsGridComponent],
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
        <div *ngFor="let s of summaryStats" class="card-green flex items-center gap-3 py-3">
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

      <!-- ── Patient grid (subcomponent) ─────────── -->
      <app-patient-cards-grid [patients]="filteredPatients()"></app-patient-cards-grid>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientListPageComponent implements OnInit {
  searchQuery = '';
  activeFilter = 'All';
  filters = ['All', 'Active', 'Critical', 'Inactive'];

  patients: PatientCard[] = [
    { id: '1', name: 'Sarah Johnson',  initials: 'SJ', age: 39, gender: 'Female', mrn: '00-1234', lastVisit: new Date(Date.now() - 2 * 86400000),  status: 'Active',   conditions: ['Hypertension', 'Diabetes'],   color: 'linear-gradient(135deg,#16a34a,#15803d)' },
    { id: '2', name: 'Michael Chen',   initials: 'MC', age: 46, gender: 'Male',   mrn: '00-2345', lastVisit: new Date(Date.now() - 1 * 86400000),  status: 'Active',   conditions: ['Asthma'],                     color: 'linear-gradient(135deg,#2563eb,#1d4ed8)' },
    { id: '3', name: 'Emma Williams',  initials: 'EW', age: 31, gender: 'Female', mrn: '00-3456', lastVisit: new Date(Date.now() - 7 * 86400000),  status: 'Active',   conditions: ['Migraine', 'Anxiety'],        color: 'linear-gradient(135deg,#7c3aed,#6d28d9)' },
    { id: '4', name: 'Robert Davis',   initials: 'RD', age: 59, gender: 'Male',   mrn: '00-4567', lastVisit: new Date(Date.now() - 3 * 86400000),  status: 'Critical', conditions: ['CAD', 'Heart Failure', 'CKD'], color: 'linear-gradient(135deg,#dc2626,#b91c1c)' },
    { id: '5', name: 'Linda Martinez', initials: 'LM', age: 35, gender: 'Female', mrn: '00-5678', lastVisit: new Date(Date.now() - 14 * 86400000), status: 'Active',   conditions: ['Hypothyroidism'],             color: 'linear-gradient(135deg,#0d9488,#0f766e)' },
    { id: '6', name: 'James Wilson',   initials: 'JW', age: 53, gender: 'Male',   mrn: '00-6789', lastVisit: new Date(Date.now() - 30 * 86400000), status: 'Inactive', conditions: ['COPD'],                       color: 'linear-gradient(135deg,#d97706,#b45309)' },
  ];

  summaryStats = [
    { value: '1,234', label: 'Total Patients',  icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z', iconClass: 'icon-box-primary icon-box-md' },
    { value: '18',    label: 'New This Month',   icon: 'M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z', iconClass: 'icon-box-teal icon-box-md' },
    { value: '4',     label: 'Critical Status',  icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', iconClass: 'icon-box-red icon-box-md' },
  ];

  filteredPatients(): PatientCard[] {
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

  ngOnInit(): void {}
}
