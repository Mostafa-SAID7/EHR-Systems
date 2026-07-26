import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * record-detail-page Component
 * Page for record-detail-page
 */
@Component({
  selector: 'app-record-detail-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './record-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecordDetailPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
