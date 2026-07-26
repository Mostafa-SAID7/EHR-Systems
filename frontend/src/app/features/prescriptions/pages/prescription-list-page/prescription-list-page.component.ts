import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PrescriptionStatsStripComponent, PrescriptionStat } from '../../components/prescription-stats-strip/prescription-stats-strip.component';

interface Prescription {
  id: string;
  patient: string;
  initials: string;
  drug: string;
  dosage: string;
  frequency: string;
  prescribedBy: string;
  date: Date;
  refills: number;
  status: 'Active' | 'Expired' | 'Discontinued' | 'Pending';
  color: string;
}

@Component({
  selector: 'app-prescription-list-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    PrescriptionStatsStripComponent
  ],
  templateUrl: './prescription-list-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrescriptionListPageComponent implements OnInit {
  searchQuery = '';
  activeFilter = 'All';
  filters = ['All', 'Active', 'Pending', 'Expired', 'Discontinued'];

  stats: PrescriptionStat[] = [
    { label: 'Total e-Rx',    value: '284',  icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z', iconClass: 'icon-box-primary' },
    { label: 'Active',        value: '198',  icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-teal' },
    { label: 'Pending Refill',value: '12',   icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-amber' },
    { label: 'Sent Today',    value: '24',   icon: 'M12 19l9 2-9-18-9 18 9-2zm0 0v-8', iconClass: 'icon-box-primary' },
  ];

  prescriptions: Prescription[] = [
    { id: '1', patient: 'Sarah Johnson',  initials: 'SJ', drug: 'Metformin 1000mg',     dosage: '1000mg', frequency: 'Twice daily',   prescribedBy: 'Dr. Patel',   date: new Date(2026, 5, 15), refills: 3, status: 'Active',       color: 'linear-gradient(135deg,#16a34a,#15803d)' },
    { id: '2', patient: 'Sarah Johnson',  initials: 'SJ', drug: 'Lisinopril 10mg',      dosage: '10mg',   frequency: 'Once daily',     prescribedBy: 'Dr. Patel',   date: new Date(2026, 3, 10), refills: 5, status: 'Active',       color: 'linear-gradient(135deg,#16a34a,#15803d)' },
    { id: '3', patient: 'Michael Chen',   initials: 'MC', drug: 'Albuterol Inhaler',    dosage: '90mcg',  frequency: 'As needed',      prescribedBy: 'Dr. Smith',   date: new Date(2026, 6, 1),  refills: 2, status: 'Active',       color: 'linear-gradient(135deg,#0d9488,#0f766e)' },
    { id: '4', patient: 'Robert Davis',   initials: 'RD', drug: 'Carvedilol 6.25mg',    dosage: '6.25mg', frequency: 'Twice daily',     prescribedBy: 'Dr. Garcia',  date: new Date(2026, 6, 22), refills: 0, status: 'Pending',      color: 'linear-gradient(135deg,#dc2626,#b91c1c)' },
    { id: '5', patient: 'Linda Martinez', initials: 'LM', drug: 'Levothyroxine 75mcg',  dosage: '75mcg',  frequency: 'Once daily AM',   prescribedBy: 'Dr. Patel',   date: new Date(2026, 6, 19), refills: 2, status: 'Active',       color: 'linear-gradient(135deg,#d97706,#b45309)' },
    { id: '6', patient: 'Emma Williams',  initials: 'EW', drug: 'Sumatriptan 100mg',    dosage: '100mg',  frequency: 'As needed',      prescribedBy: 'Dr. Patel',   date: new Date(2025, 11, 1), refills: 0, status: 'Expired',      color: 'linear-gradient(135deg,#7c3aed,#6d28d9)' },
    { id: '7', patient: 'James Wilson',   initials: 'JW', drug: 'Tiotropium Inhaler',   dosage: '18mcg',  frequency: 'Once daily',     prescribedBy: 'Dr. Smith',   date: new Date(2026, 2, 10), refills: 1, status: 'Active',       color: 'linear-gradient(135deg,#16a34a,#4ade80)' },
    { id: '8', patient: 'Robert Davis',   initials: 'RD', drug: 'Furosemide 40mg',      dosage: '40mg',   frequency: 'Once daily',     prescribedBy: 'Dr. Garcia',  date: new Date(2025, 8, 1),  refills: 0, status: 'Discontinued', color: 'linear-gradient(135deg,#dc2626,#b91c1c)' },
  ];

  filtered(): Prescription[] {
    let list = this.prescriptions;
    if (this.activeFilter !== 'All') list = list.filter(rx => rx.status === this.activeFilter);
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      list = list.filter(rx => rx.patient.toLowerCase().includes(q) || rx.drug.toLowerCase().includes(q));
    }
    return list;
  }

  statusClass(s: string): string {
    return s === 'Active' ? 'badge-success' : s === 'Pending' ? 'badge-warning' : s === 'Expired' ? 'badge-danger' : 'badge-neutral';
  }

  ngOnInit(): void {}
}
