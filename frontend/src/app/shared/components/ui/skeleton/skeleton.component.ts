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
  template: `
    <!-- Text / Line Skeleton -->
    <div
      *ngIf="shape === 'text'"
      [style.width]="width"
      [style.height]="height || '1rem'"
      class="skeleton-pulse rounded-md"
    ></div>

    <!-- Circle Skeleton -->
    <div
      *ngIf="shape === 'circle' || shape === 'avatar'"
      [style.width]="width || '2.5rem'"
      [style.height]="height || width || '2.5rem'"
      class="skeleton-pulse rounded-full shrink-0"
    ></div>

    <!-- Rectangle Skeleton -->
    <div
      *ngIf="shape === 'rect'"
      [style.width]="width || '100%'"
      [style.height]="height || '8rem'"
      class="skeleton-pulse rounded-xl"
    ></div>

    <!-- Card Skeleton Placeholder -->
    <div
      *ngIf="shape === 'card'"
      class="skeleton-pulse rounded-2xl p-5 border border-surface-200/60 dark:border-surface-700/60 space-y-4"
    >
      <div class="flex items-center gap-3">
        <div class="w-10 h-10 rounded-full skeleton-pulse"></div>
        <div class="space-y-2 flex-1">
          <div class="h-4 w-1/3 skeleton-pulse rounded"></div>
          <div class="h-3 w-1/4 skeleton-pulse rounded"></div>
        </div>
      </div>
      <div class="h-16 w-full skeleton-pulse rounded-xl"></div>
      <div class="flex justify-between items-center">
        <div class="h-4 w-1/4 skeleton-pulse rounded"></div>
        <div class="h-8 w-20 skeleton-pulse rounded-lg"></div>
      </div>
    </div>

    <!-- Table Row Skeleton Placeholder -->
    <tr *ngIf="shape === 'table-row'" class="animate-pulse">
      <td class="px-4 py-4">
        <div class="flex items-center gap-3">
          <div class="w-8 h-8 rounded-full skeleton-pulse"></div>
          <div class="space-y-1">
            <div class="h-3.5 w-28 skeleton-pulse rounded"></div>
            <div class="h-2.5 w-20 skeleton-pulse rounded"></div>
          </div>
        </div>
      </td>
      <td class="px-4 py-4"><div class="h-3.5 w-20 skeleton-pulse rounded"></div></td>
      <td class="px-4 py-4"><div class="h-3.5 w-24 skeleton-pulse rounded"></div></td>
      <td class="px-4 py-4"><div class="h-5 w-16 skeleton-pulse rounded-full"></div></td>
      <td class="px-4 py-4 text-right"><div class="h-8 w-8 ml-auto skeleton-pulse rounded-lg"></div></td>
    </tr>
  `,
  styles: [
    `
      .skeleton-pulse {
        background: linear-gradient(
          90deg,
          rgba(229, 231, 235, 0.8) 0%,
          rgba(243, 244, 246, 0.9) 50%,
          rgba(229, 231, 235, 0.8) 100%
        );
        background-size: 200% 100%;
        animation: shimmer 1.6s infinite ease-in-out;
      }
      :host-context(.dark) .skeleton-pulse {
        background: linear-gradient(
          90deg,
          rgba(30, 41, 59, 0.8) 0%,
          rgba(51, 65, 85, 0.9) 50%,
          rgba(30, 41, 59, 0.8) 100%
        );
        background-size: 200% 100%;
      }
      @keyframes shimmer {
        0% {
          background-position: 200% 0;
        }
        100% {
          background-position: -200% 0;
        }
      }
    `,
  ],
})
export class SkeletonComponent {
  @Input() shape: SkeletonShape = 'text';
  @Input() width = '100%';
  @Input() height = '';
}
