import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * prescription-list-page Component
 * Page for prescription-list-page
 */
@Component({
  selector: 'app-prescription-list-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="prescription-list-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          prescription-list-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrescriptionListPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
