import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-prescription-create-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="space-y-6 stagger max-w-3xl">

      <!-- ── Header ───────────────────────────────────── -->
      <div class="flex items-center gap-3">
        <a routerLink="/prescriptions" class="btn-icon-sm">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
          </svg>
        </a>
        <div>
          <h1 class="heading-xl">New e-Prescription</h1>
          <p class="body-text mt-0.5">Create and send a new electronic prescription</p>
        </div>
      </div>

      <!-- ── Patient selection ─────────────────────────── -->
      <div class="card space-y-4">
        <h2 class="heading-sm">Patient</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Select Patient *</label>
            <select [(ngModel)]="form.patientId" class="input-base w-full">
              <option value="">— Choose patient —</option>
              <option *ngFor="let p of patients" [value]="p.id">{{ p.name }} (MRN: {{ p.mrn }})</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Prescribing Provider *</label>
            <select [(ngModel)]="form.provider" class="input-base w-full">
              <option value="">— Select provider —</option>
              <option *ngFor="let d of doctors" [value]="d">{{ d }}</option>
            </select>
          </div>
        </div>
      </div>

      <!-- ── Medication details ────────────────────────── -->
      <div class="card space-y-4">
        <h2 class="heading-sm">Medication Details</h2>

        <!-- Drug search -->
        <div>
          <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Medication Name *</label>
          <div class="relative">
            <input type="text" [(ngModel)]="form.drug" (input)="filterDrugs()"
              placeholder="Search drug name…" class="input-base w-full pr-10"/>
            <svg class="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
          </div>
          <!-- Drug suggestions -->
          <div *ngIf="drugSuggestions.length > 0 && form.drug.length > 1"
            class="mt-1 rounded-xl border border-surface-100 dark:border-surface-700 bg-white dark:bg-surface-800 shadow-lg overflow-hidden">
            <button *ngFor="let d of drugSuggestions" (click)="selectDrug(d)"
              class="w-full text-left px-4 py-2.5 text-sm hover:bg-primary-50 dark:hover:bg-primary-900/20 transition-colors text-gray-700 dark:text-gray-300">
              {{ d }}
            </button>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Strength / Dosage *</label>
            <input type="text" [(ngModel)]="form.dosage" placeholder="e.g. 500mg" class="input-base w-full"/>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Route</label>
            <select [(ngModel)]="form.route" class="input-base w-full">
              <option *ngFor="let r of routes" [value]="r">{{ r }}</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Frequency *</label>
            <select [(ngModel)]="form.frequency" class="input-base w-full">
              <option value="">— Select —</option>
              <option *ngFor="let f of frequencies" [value]="f">{{ f }}</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Quantity</label>
            <input type="number" [(ngModel)]="form.quantity" placeholder="e.g. 30" class="input-base w-full"/>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Days Supply</label>
            <input type="number" [(ngModel)]="form.days" placeholder="e.g. 30" class="input-base w-full"/>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Refills</label>
            <select [(ngModel)]="form.refills" class="input-base w-full">
              <option *ngFor="let r of [0,1,2,3,4,5,6]" [value]="r">{{ r }}</option>
            </select>
          </div>
        </div>

        <div>
          <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Patient Instructions (Sig)</label>
          <textarea [(ngModel)]="form.instructions" rows="2" placeholder="e.g. Take 1 tablet by mouth twice daily with food."
            class="input-base w-full resize-none"></textarea>
        </div>
      </div>

      <!-- ── Pharmacy & options ────────────────────────── -->
      <div class="card space-y-4">
        <h2 class="heading-sm">Pharmacy &amp; Options</h2>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Preferred Pharmacy</label>
            <select [(ngModel)]="form.pharmacy" class="input-base w-full">
              <option value="">— Select pharmacy —</option>
              <option *ngFor="let p of pharmacies" [value]="p">{{ p }}</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Diagnosis / Indication</label>
            <input type="text" [(ngModel)]="form.indication" placeholder="e.g. Type 2 Diabetes" class="input-base w-full"/>
          </div>
        </div>
        <div class="flex flex-col gap-2.5">
          <label class="flex items-center gap-2.5 cursor-pointer">
            <input type="checkbox" [(ngModel)]="form.genericOk" class="w-4 h-4 rounded accent-primary-600"/>
            <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Substitution permitted (generic OK)</span>
          </label>
          <label class="flex items-center gap-2.5 cursor-pointer">
            <input type="checkbox" [(ngModel)]="form.daw" class="w-4 h-4 rounded accent-primary-600"/>
            <span class="text-sm font-medium text-gray-700 dark:text-gray-300">Dispense as written (DAW)</span>
          </label>
          <label class="flex items-center gap-2.5 cursor-pointer">
            <input type="checkbox" [(ngModel)]="form.prn" class="w-4 h-4 rounded accent-primary-600"/>
            <span class="text-sm font-medium text-gray-700 dark:text-gray-300">PRN (as needed)</span>
          </label>
        </div>
        <div>
          <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Prescriber Notes</label>
          <textarea [(ngModel)]="form.notes" rows="2" class="input-base w-full resize-none" placeholder="Internal notes (not sent to pharmacy)…"></textarea>
        </div>
      </div>

      <!-- ── Drug interaction warning ──────────────────── -->
      <div *ngIf="form.drug.toLowerCase().includes('ibuprofen')"
        class="card border border-amber-200 dark:border-amber-800/60 bg-amber-50 dark:bg-amber-950/30">
        <div class="flex items-start gap-3">
          <div class="icon-box-sm icon-box-amber shrink-0 mt-0.5">
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
            </svg>
          </div>
          <div>
            <p class="text-sm font-semibold text-amber-800 dark:text-amber-200">Drug Interaction Warning</p>
            <p class="text-xs text-amber-700 dark:text-amber-300 mt-0.5">This patient is on Metformin. NSAIDs may affect renal function. Consider alternative analgesic.</p>
          </div>
        </div>
      </div>

      <!-- ── Actions ──────────────────────────────────── -->
      <div class="flex items-center gap-3 pb-4">
        <button (click)="submit()" [disabled]="!isValid()" class="btn-primary">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8"/>
          </svg>
          Send to Pharmacy
        </button>
        <button class="btn-secondary">Save as Draft</button>
        <a routerLink="/prescriptions" class="btn-ghost">Cancel</a>

        <div *ngIf="submitted"
          class="flex items-center gap-2 px-4 py-2.5 rounded-xl bg-primary-100 dark:bg-primary-900/40 text-primary-700 dark:text-primary-300 text-sm font-semibold border border-primary-200/60 dark:border-primary-800/40">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          Prescription sent!
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrescriptionCreatePageComponent implements OnInit {
  submitted = false;

  form = {
    patientId: '', provider: '', drug: '', dosage: '', route: 'Oral',
    frequency: '', quantity: '', days: '', refills: 0, instructions: '',
    pharmacy: '', indication: '', notes: '', genericOk: true, daw: false, prn: false,
  };

  drugSuggestions: string[] = [];
  allDrugs = ['Metformin 500mg', 'Metformin 1000mg', 'Lisinopril 10mg', 'Lisinopril 20mg', 'Atorvastatin 40mg', 'Atorvastatin 80mg', 'Levothyroxine 50mcg', 'Levothyroxine 75mcg', 'Albuterol Inhaler 90mcg', 'Tiotropium 18mcg', 'Carvedilol 6.25mg', 'Furosemide 40mg', 'Aspirin 81mg', 'Sumatriptan 100mg', 'Sertraline 50mg', 'Ibuprofen 400mg', 'Amoxicillin 500mg', 'Azithromycin 250mg', 'Omeprazole 20mg', 'Amlodipine 5mg'];
  patients = [
    { id: '1', name: 'Sarah Johnson',  mrn: '00-1234' },
    { id: '2', name: 'Michael Chen',   mrn: '00-2345' },
    { id: '3', name: 'Emma Williams',  mrn: '00-3456' },
    { id: '4', name: 'Robert Davis',   mrn: '00-4567' },
    { id: '5', name: 'Linda Martinez', mrn: '00-5678' },
  ];
  doctors    = ['Dr. Patel', 'Dr. Smith', 'Dr. Garcia', 'Dr. Johnson', 'Dr. Lee'];
  routes     = ['Oral', 'Topical', 'Inhalation', 'Intravenous', 'Subcutaneous', 'Sublingual', 'Ophthalmic', 'Otic'];
  frequencies = ['Once daily', 'Twice daily', 'Three times daily', 'Four times daily', 'Every 4 hours', 'Every 6 hours', 'Every 8 hours', 'Every 12 hours', 'As needed (PRN)', 'At bedtime', 'Before meals', 'Weekly'];
  pharmacies = ['CVS Pharmacy — Main St', 'Walgreens — Oak Ave', 'Rite Aid — Elm Blvd', 'Hospital Pharmacy', 'Mail-Order Pharmacy'];

  filterDrugs(): void {
    const q = this.form.drug.toLowerCase();
    this.drugSuggestions = q.length < 2 ? [] : this.allDrugs.filter(d => d.toLowerCase().includes(q)).slice(0, 6);
  }

  selectDrug(d: string): void {
    this.form.drug = d;
    this.drugSuggestions = [];
  }

  isValid(): boolean {
    return !!(this.form.patientId && this.form.provider && this.form.drug && this.form.dosage && this.form.frequency);
  }

  submit(): void {
    if (!this.isValid()) return;
    this.submitted = true;
    setTimeout(() => this.submitted = false, 3500);
  }

  ngOnInit(): void {}
}
