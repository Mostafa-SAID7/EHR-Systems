import { Component, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';

interface Invoice {
  id: string; patient: string; initials: string; service: string;
  date: Date; amount: number; paid: number;
  status: 'Paid' | 'Pending' | 'Overdue' | 'Partial';
  insurance: string; color: string;
}

@Component({
  selector: 'app-invoice-list-page',
  standalone: true,
  imports: [CommonModule, DecimalPipe],
  templateUrl: './invoice-list-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvoiceListPageComponent {
  filters   = ['All', 'Paid', 'Pending', 'Overdue', 'Partial'];
  activeFilter = 'All';
  searchTerm = signal('');
  sortKey    = signal('date-desc');

  summary = [
    { label: 'Total Invoices',   value: '284',       sub: 'This month',        cls: 'text-gray-900 dark:text-white' },
    { label: 'Total Billed',     value: '$124,850',   sub: 'MTD',               cls: 'text-gray-900 dark:text-white' },
    { label: 'Amount Collected', value: '$98,420',    sub: '78.8% collection',  cls: 'text-emerald-600' },
    { label: 'Outstanding',      value: '$26,430',    sub: 'A/R balance',       cls: 'text-amber-600' },
  ];

  invoices: Invoice[] = [
    { id: 'INV-2024-0284', patient: 'Sarah Johnson',  initials: 'SJ', service: 'Annual Physical',    date: new Date(2026,6,24), amount: 350,   paid: 350,   status: 'Paid',    insurance: 'BlueCross',  color: '#16a34a' },
    { id: 'INV-2024-0283', patient: 'Michael Chen',   initials: 'MC', service: 'Cardiology Consult', date: new Date(2026,6,22), amount: 820,   paid: 820,   status: 'Paid',    insurance: 'Aetna',      color: '#2563eb' },
    { id: 'INV-2024-0282', patient: 'Emma Williams',  initials: 'EW', service: 'Lab Panel + Office', date: new Date(2026,6,21), amount: 540,   paid: 270,   status: 'Partial', insurance: 'UnitedHealth',color: '#7c3aed' },
    { id: 'INV-2024-0281', patient: 'Robert Davis',   initials: 'RD', service: 'Emergency Visit',    date: new Date(2026,6,18), amount: 1250,  paid: 0,     status: 'Overdue', insurance: 'Medicare',   color: '#dc2626' },
    { id: 'INV-2024-0280', patient: 'Linda Martinez', initials: 'LM', service: 'Follow-up Visit',    date: new Date(2026,6,17), amount: 185,   paid: 0,     status: 'Pending', insurance: 'Medicaid',   color: '#0d9488' },
    { id: 'INV-2024-0279', patient: 'James Wilson',   initials: 'JW', service: 'MRI + Radiology',    date: new Date(2026,6,15), amount: 2100,  paid: 2100,  status: 'Paid',    insurance: 'Cigna',      color: '#d97706' },
    { id: 'INV-2024-0278', patient: 'Patricia Moore', initials: 'PM', service: 'Diabetes Management', date: new Date(2026,6,14), amount: 420,  paid: 0,     status: 'Pending', insurance: 'BlueCross',  color: '#9333ea' },
    { id: 'INV-2024-0277', patient: 'Charles Taylor', initials: 'CT', service: 'Pulmonology Consult', date: new Date(2026,6,10), amount: 680,  paid: 0,     status: 'Overdue', insurance: 'Aetna',      color: '#0891b2' },
    { id: 'INV-2024-0276', patient: 'Jennifer Lee',   initials: 'JL', service: 'Pre-op Assessment',  date: new Date(2026,6,8),  amount: 310,   paid: 310,   status: 'Paid',    insurance: 'UnitedHealth',color: '#be185d' },
    { id: 'INV-2024-0275', patient: 'David Brown',    initials: 'DB', service: 'Orthopedic Visit',   date: new Date(2026,6,5),  amount: 950,   paid: 475,   status: 'Partial', insurance: 'Medicare',   color: '#b45309' },
  ];

  filtered = computed(() => {
    let r = this.invoices;
    if (this.activeFilter !== 'All') r = r.filter(i => i.status === this.activeFilter);
    const t = this.searchTerm().toLowerCase();
    if (t) r = r.filter(i => i.patient.toLowerCase().includes(t) || i.id.toLowerCase().includes(t) || i.service.toLowerCase().includes(t));
    return r;
  });

  onSearch(e: Event) { this.searchTerm.set((e.target as HTMLInputElement).value); }
  onSort(e: Event)   { this.sortKey.set((e.target as HTMLSelectElement).value); }
}
