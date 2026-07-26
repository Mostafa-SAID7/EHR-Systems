import { Component, OnInit, ChangeDetectionStrategy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LabStatusStripComponent, LabStatusStat } from '../../components/lab-status-strip/lab-status-strip.component';
import { LabResultCardsComponent, LabResult } from '../../components/lab-result-cards/lab-result-cards.component';

@Component({
  selector: 'app-lab-results-page',
  standalone: true,
  imports: [CommonModule, RouterModule, LabStatusStripComponent, LabResultCardsComponent],
  templateUrl: './lab-results-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabResultsPageComponent implements OnInit {
  readonly activeCategory = signal('All');

  categories = ['All', 'Hematology', 'Chemistry', 'Microbiology', 'Cardiology', 'Endocrinology'];

  statusSummary: LabStatusStat[] = [
    { label: 'Normal',   count: 18, icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z',                                                                                              iconClass: 'icon-box-primary' },
    { label: 'High',     count: 4,  icon: 'M5 15l7-7 7 7',                                                                                                                               iconClass: 'icon-box-red' },
    { label: 'Low',      count: 2,  icon: 'M19 9l-7 7-7-7',                                                                                                                              iconClass: 'icon-box-blue' },
    { label: 'Critical', count: 1,  icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z',     iconClass: 'icon-box-red' },
    { label: 'Pending',  count: 3,  icon: 'M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z',                                                                                                iconClass: 'icon-box-amber' },
  ];

  private readonly _results: LabResult[] = [
    { id: '1', patient: 'Sarah Johnson',  initials: 'SJ', test: 'HbA1c',          category: 'Endocrinology', date: new Date(2026, 6, 23), value: '7.2',  unit: '%',      range: '4.0–5.6',  status: 'High',    color: '#16a34a' },
    { id: '2', patient: 'Michael Chen',   initials: 'MC', test: 'WBC Count',      category: 'Hematology',    date: new Date(2026, 6, 22), value: '9.8',  unit: 'K/µL',   range: '4.5–11.0', status: 'Normal',  color: '#2563eb' },
    { id: '3', patient: 'Emma Williams',  initials: 'EW', test: 'Serum Cortisol', category: 'Endocrinology', date: new Date(2026, 6, 21), value: '28.4', unit: 'µg/dL',  range: '6.0–23.0', status: 'Critical',color: '#7c3aed' },
    { id: '4', patient: 'Robert Davis',   initials: 'RD', test: 'Troponin I',     category: 'Cardiology',    date: new Date(2026, 6, 20), value: '0.04', unit: 'ng/mL',  range: '< 0.04',   status: 'High',    color: '#dc2626' },
    { id: '5', patient: 'Linda Martinez', initials: 'LM', test: 'TSH',            category: 'Endocrinology', date: new Date(2026, 6, 19), value: '4.1',  unit: 'mIU/L',  range: '0.4–4.0',  status: 'High',    color: '#0d9488' },
    { id: '6', patient: 'James Wilson',   initials: 'JW', test: 'eGFR',           category: 'Chemistry',     date: new Date(2026, 6, 18), value: '48',   unit: 'mL/min', range: '> 60',     status: 'Low',     color: '#d97706' },
  ];

  /** Computed — recalculates only when activeCategory signal changes */
  readonly filteredResults = computed(() => {
    const cat = this.activeCategory();
    return cat === 'All' ? this._results : this._results.filter(r => r.category === cat);
  });

  setCategory(cat: string): void { this.activeCategory.set(cat); }

  trackByValue(_: number, val: string): string { return val; }

  ngOnInit(): void {}
}
