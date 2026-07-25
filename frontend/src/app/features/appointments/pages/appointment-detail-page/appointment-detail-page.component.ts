import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * appointment-detail-page Component
 * Page for appointment-detail-page
 */
@Component({
  selector: 'app-appointment-detail-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="appointment-detail-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          appointment-detail-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentDetailPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
