import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * patient-list-page Component
 * Page for patient-list-page
 */
@Component({
  selector: 'app-patient-list-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="patient-list-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          patient-list-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientListPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
