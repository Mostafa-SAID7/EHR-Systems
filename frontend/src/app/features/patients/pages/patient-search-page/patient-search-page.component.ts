import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-patient-search-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './patient-search-page.component.html',
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
