import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SkeletonShape = 'text' | 'circle' | 'rect' | 'card' | 'table-row' | 'avatar';

/**
 * Skeleton Loader Component
 * Modern shimmer placeholders for loading states.
 */
@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './skeleton.component.html',
  styleUrl: './skeleton.component.scss'
})
export class SkeletonComponent {
  @Input() shape: SkeletonShape = 'text';
  @Input() width = '100%';
  @Input() height = '';
}
