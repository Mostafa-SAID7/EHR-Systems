import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * compliance-page Component
 * Page for compliance-page
 */
@Component({
  selector: 'app-compliance-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="compliance-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          compliance-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CompliancePageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
