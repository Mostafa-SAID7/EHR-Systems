import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * role-management-page Component
 * Page for role-management-page
 */
@Component({
  selector: 'app-role-management-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="role-management-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          role-management-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RoleManagementPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
