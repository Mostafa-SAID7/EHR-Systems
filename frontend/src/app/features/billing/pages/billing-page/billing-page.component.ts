import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * billing-page Component
 * Page for billing-page
 */
@Component({
  selector: 'app-billing-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="billing-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          billing-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BillingPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
