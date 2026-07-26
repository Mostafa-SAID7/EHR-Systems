import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * lab-result-detail-page Component
 * Page for lab-result-detail-page
 */
@Component({
  selector: 'app-lab-result-detail-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  templateUrl: './lab-result-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LabResultDetailPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
