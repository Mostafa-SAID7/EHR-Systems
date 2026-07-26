import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-prescription-create-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './prescription-create-page.component.html',
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
