import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * patient-search-page Component
 * Page for patient-search-page
 */
@Component({
  selector: 'app-patient-search-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="patient-search-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          patient-search-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientSearchPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
