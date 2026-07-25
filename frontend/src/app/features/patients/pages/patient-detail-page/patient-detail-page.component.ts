import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * patient-detail-page Component
 * Page for patient-detail-page
 */
@Component({
  selector: 'app-patient-detail-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="patient-detail-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          patient-detail-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientDetailPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
