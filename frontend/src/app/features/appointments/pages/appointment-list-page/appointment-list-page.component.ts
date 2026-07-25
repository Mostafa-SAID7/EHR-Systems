import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * appointment-list-page Component
 * Page for appointment-list-page
 */
@Component({
  selector: 'app-appointment-list-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="appointment-list-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          appointment-list-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentListPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
