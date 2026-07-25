import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * forgot-password-page Component
 * Page for forgot-password-page
 */
@Component({
  selector: 'app-forgot-password-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="forgot-password-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          forgot-password-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
