import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * patient-timeline-page Component
 * Page for patient-timeline-page
 */
@Component({
  selector: 'app-patient-timeline-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="patient-timeline-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          patient-timeline-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientTimelinePageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
