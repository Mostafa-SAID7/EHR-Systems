import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * prescription-detail-page Component
 * Page for prescription-detail-page
 */
@Component({
  selector: 'app-prescription-detail-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="prescription-detail-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          prescription-detail-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrescriptionDetailPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
