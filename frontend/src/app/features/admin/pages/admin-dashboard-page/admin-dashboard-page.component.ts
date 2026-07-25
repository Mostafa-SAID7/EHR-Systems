import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * admin-dashboard-page Component
 * Page for admin-dashboard-page
 */
@Component({
  selector: 'app-admin-dashboard-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="admin-dashboard-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          admin-dashboard-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
