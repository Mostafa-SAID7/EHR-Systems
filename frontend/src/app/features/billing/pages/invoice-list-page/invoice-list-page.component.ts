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
  templateUrl: './invoice-list-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InvoiceListPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
