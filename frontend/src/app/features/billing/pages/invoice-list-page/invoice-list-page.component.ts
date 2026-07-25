import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * invoice-list-page Component
 * Page for invoice-list-page
 */
@Component({
  selector: 'app-invoice-list-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="invoice-list-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          invoice-list-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvoiceListPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
