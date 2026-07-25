import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * reset-password-page Component
 * Page for reset-password-page
 */
@Component({
  selector: 'app-reset-password-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="reset-password-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          reset-password-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPasswordPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
