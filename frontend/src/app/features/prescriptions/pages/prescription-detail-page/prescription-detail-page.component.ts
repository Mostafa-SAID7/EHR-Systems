import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-prescription-detail-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './prescription-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrescriptionDetailPageComponent implements OnInit {
  rx = {
    id: '10042',
    drug: 'Metformin 1000mg',
    genericName: 'Metformin Hydrochloride',
    category: 'Biguanide Antidiabetic Agent',
    date: new Date(2026, 5, 15),
    status: 'Active',
    refills: 3,
    sig: 'Take 1 tablet (1000mg) by mouth TWICE DAILY with meals. Swallow whole — do not crush or chew. Take with food to reduce GI side effects.',
  };

  drugDetails = [
    { label: 'Strength',     value: '1000mg' },
    { label: 'Form',         value: 'Tablet' },
    { label: 'Route',        value: 'Oral' },
    { label: 'Frequency',    value: 'Twice daily' },
    { label: 'Quantity',     value: '60 tablets' },
    { label: 'Days Supply',  value: '30 days' },
    { label: 'Refills',      value: '3 remaining' },
    { label: 'Expires',      value: 'Jun 15, 2027' },
  ];

  refillHistory = [
    { event: 'Original Fill',  pharmacy: 'CVS Pharmacy — Main St', date: 'Jun 15, 2026', status: 'Dispensed' },
    { event: 'Refill #1',     pharmacy: 'CVS Pharmacy — Main St', date: 'Jul 14, 2026', status: 'Dispensed' },
    { event: 'Refill #2',     pharmacy: 'Walgreens — Oak Ave',    date: 'Pending',       status: 'Processing' },
  ];

  ngOnInit(): void {}
}
