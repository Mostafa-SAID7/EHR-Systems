import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-patient-search-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ──────────────────────────────────── -->
      <div>
        <h1 class="heading-xl">Patient Search</h1>
        <p class="body-text mt-1">Find patients by name, MRN, date of birth, phone, or diagnosis</p>
      </div>

      <!-- ── Search hero ──────────────────────────────── -->
      <div class="card bg-gradient-to-br from-primary-50 to-white dark:from-primary-950/30 dark:to-surface-800 border border-primary-100 dark:border-primary-900/30">
        <div class="relative">
          <div class="absolute inset-y-0 left-0 flex items-center pl-4 pointer-events-none">
            <svg class="w-5 h-5 text-primary-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
          </div>
          <input
            type="text"
            [(ngModel)]="query"
            (input)="onSearch()"
            placeholder="Search by name, MRN, date of birth, or condition…"
            class="w-full pl-12 pr-4 py-4 text-base rounded-xl border border-primary-200/80 dark:border-primary-800/50 bg-white dark:bg-surface-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary-500/40 transition-all"
          />
        </div>

        <!-- Quick filter chips -->
        <div class="flex flex-wrap gap-2 mt-4">
          <span class="text-xs font-medium text-gray-500 dark:text-gray-400 self-center mr-1">Quick filters:</span>
          <button *ngFor="let f of quickFilters"
            (click)="applyFilter(f)"
            [class]="activeQuickFilter === f ? 'filter-pill-active' : 'filter-pill'">
            {{ f }}
          </button>
        </div>
      </div>

      <!-- ── Advanced filters (collapsible) ──────────── -->
      <div class="card">
        <button class="flex items-center justify-between w-full" (click)="showAdvanced = !showAdvanced">
          <span class="text-sm font-semibold text-gray-700 dark:text-gray-300">Advanced Filters</span>
          <svg class="w-4 h-4 text-gray-400 transition-transform" [class.rotate-180]="showAdvanced" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
          </svg>
        </button>
        <div *ngIf="showAdvanced" class="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-4 pt-4 border-t border-surface-100 dark:border-surface-700/50">
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Gender</label>
            <select class="input-base w-full">
              <option>Any</option>
              <option>Male</option>
              <option>Female</option>
              <option>Other</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Status</label>
            <select class="input-base w-full">
              <option>Any</option>
              <option>Active</option>
              <option>Critical</option>
              <option>Inactive</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Provider</label>
            <select class="input-base w-full">
              <option>Any</option>
              <option>Dr. Patel</option>
              <option>Dr. Smith</option>
              <option>Dr. Garcia</option>
            </select>
          </div>
        </div>
      </div>

      <!-- ── Results ──────────────────────────────────── -->
      <div *ngIf="results.length > 0">
        <div class="flex items-center justify-between mb-3">
          <p class="text-sm font-semibold text-gray-700 dark:text-gray-300">
            {{ results.length }} result{{ results.length !== 1 ? 's' : '' }} found
          </p>
          <button class="btn-secondary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
            </svg>
            Export
          </button>
        </div>
        <div class="card p-0 overflow-hidden">
          <div class="overflow-x-auto">
            <table class="table-base">
              <thead>
                <tr>
                  <th>Patient</th>
                  <th>MRN</th>
                  <th>DOB / Age</th>
                  <th>Conditions</th>
                  <th>Last Visit</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let p of results" class="hover:bg-primary-50/40 dark:hover:bg-primary-900/10 transition-colors cursor-pointer">
                  <td>
                    <div class="flex items-center gap-2.5">
                      <div class="avatar-custom-md" [style.background]="p.color">{{ p.initials }}</div>
                      <div>
                        <p class="font-semibold text-gray-900 dark:text-white">{{ p.name }}</p>
                        <p class="text-2xs text-gray-400">{{ p.phone }}</p>
                      </div>
                    </div>
                  </td>
                  <td class="font-mono text-xs text-gray-600 dark:text-gray-400">{{ p.mrn }}</td>
                  <td class="text-xs text-gray-600 dark:text-gray-400">{{ p.dob }}<br><span class="text-gray-400">{{ p.age }} yrs</span></td>
                  <td>
                    <div class="flex flex-wrap gap-1">
                      <span *ngFor="let c of p.conditions.slice(0,2)" class="badge-neutral text-2xs">{{ c }}</span>
                      <span *ngIf="p.conditions.length > 2" class="text-2xs text-gray-400">+{{ p.conditions.length - 2 }}</span>
                    </div>
                  </td>
                  <td class="text-xs text-gray-500 dark:text-gray-400">{{ p.lastVisit | date:'MMM d, y' }}</td>
                  <td>
                    <span [ngClass]="p.status === 'Active' ? 'badge-success' : p.status === 'Critical' ? 'badge-danger' : 'badge-neutral'" class="badge">{{ p.status }}</span>
                  </td>
                  <td>
                    <a [routerLink]="['/patients', p.id]" class="btn-icon-sm">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
                      </svg>
                    </a>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- ── Empty / initial state ────────────────────── -->
      <div *ngIf="results.length === 0 && query.length === 0" class="card text-center py-16">
        <div class="icon-box-xl icon-box-primary mx-auto mb-4">
          <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
          </svg>
        </div>
        <p class="text-base font-semibold text-gray-900 dark:text-white mb-2">Search Patients</p>
        <p class="body-text text-sm max-w-xs mx-auto">Enter a name, MRN, or diagnosis to find a patient record</p>
        <div class="mt-6">
          <p class="text-2xs font-semibold text-gray-400 uppercase tracking-wider mb-3">Recent Searches</p>
          <div class="flex flex-wrap justify-center gap-2">
            <button *ngFor="let r of recentSearches" (click)="query = r; onSearch()"
              class="px-3 py-1.5 rounded-lg text-xs font-medium bg-primary-50 dark:bg-primary-950/40 text-primary-700 dark:text-primary-300 hover:bg-primary-100 transition-colors">
              {{ r }}
            </button>
          </div>
        </div>
      </div>

      <!-- ── No results ────────────────────────────────── -->
      <div *ngIf="results.length === 0 && query.length > 0" class="card text-center py-16">
        <p class="text-base font-semibold text-gray-900 dark:text-white mb-2">No patients found</p>
        <p class="body-text text-sm">No results for "<span class="font-semibold text-primary-600">{{ query }}</span>". Try a different search term.</p>
        <a routerLink="/patients/new" class="btn-primary btn-sm mt-6 inline-flex">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/>
          </svg>
          Register New Patient
        </a>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientSearchPageComponent implements OnInit {
  query = '';
  showAdvanced = false;
  activeQuickFilter = '';
  recentSearches = ['Sarah Johnson', 'Diabetes', 'MRN 00-1234', 'Dr. Patel'];
  quickFilters = ['Active', 'Critical', 'New This Month', 'Overdue Follow-up'];

  allPatients = [
    { id: '1', name: 'Sarah Johnson',   initials: 'SJ', dob: 'Mar 12, 1985', age: 39, gender: 'Female', mrn: '00-1234', phone: '555-0101', lastVisit: new Date(Date.now() - 2*86400000),  status: 'Active',   conditions: ['Hypertension','Diabetes','Hyperlipidemia'], color: 'linear-gradient(135deg,#16a34a,#15803d)' },
    { id: '2', name: 'Michael Chen',    initials: 'MC', dob: 'Jul 22, 1978',  age: 46, gender: 'Male',   mrn: '00-2345', phone: '555-0102', lastVisit: new Date(Date.now() - 1*86400000),  status: 'Active',   conditions: ['Asthma','Allergic Rhinitis'],               color: 'linear-gradient(135deg,#0d9488,#0f766e)' },
    { id: '3', name: 'Emma Williams',   initials: 'EW', dob: 'Nov 5, 1992',   age: 31, gender: 'Female', mrn: '00-3456', phone: '555-0103', lastVisit: new Date(Date.now() - 7*86400000),  status: 'Active',   conditions: ['Migraine','Anxiety'],                       color: 'linear-gradient(135deg,#7c3aed,#6d28d9)' },
    { id: '4', name: 'Robert Davis',    initials: 'RD', dob: 'Jan 30, 1965',  age: 59, gender: 'Male',   mrn: '00-4567', phone: '555-0104', lastVisit: new Date(Date.now() - 3*86400000),  status: 'Critical', conditions: ['CAD','Heart Failure','CKD'],                color: 'linear-gradient(135deg,#dc2626,#b91c1c)' },
    { id: '5', name: 'Linda Martinez',  initials: 'LM', dob: 'Sep 14, 1988',  age: 35, gender: 'Female', mrn: '00-5678', phone: '555-0105', lastVisit: new Date(Date.now() - 14*86400000), status: 'Active',   conditions: ['Hypothyroidism'],                          color: 'linear-gradient(135deg,#d97706,#b45309)' },
    { id: '6', name: 'James Wilson',    initials: 'JW', dob: 'Apr 18, 1971',  age: 53, gender: 'Male',   mrn: '00-6789', phone: '555-0106', lastVisit: new Date(Date.now() - 30*86400000), status: 'Inactive', conditions: ['COPD','Sleep Apnea'],                       color: 'linear-gradient(135deg,#16a34a,#4ade80)' },
  ];

  results: typeof this.allPatients = [];

  onSearch(): void {
    const q = this.query.toLowerCase().trim();
    if (!q) { this.results = []; return; }
    this.results = this.allPatients.filter(p =>
      p.name.toLowerCase().includes(q) ||
      p.mrn.includes(q) ||
      p.conditions.some(c => c.toLowerCase().includes(q)) ||
      p.phone.includes(q)
    );
  }

  applyFilter(f: string): void {
    this.activeQuickFilter = this.activeQuickFilter === f ? '' : f;
    if (f === 'Active')   { this.query = 'active';   }
    if (f === 'Critical') { this.query = 'critical';  }
    this.onSearch();
  }

  ngOnInit(): void {}
}
