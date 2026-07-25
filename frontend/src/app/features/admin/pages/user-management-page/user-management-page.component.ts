import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * user-management-page Component
 * Page for user-management-page
 */
@Component({
  selector: 'app-user-management-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="user-management-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          user-management-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserManagementPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
