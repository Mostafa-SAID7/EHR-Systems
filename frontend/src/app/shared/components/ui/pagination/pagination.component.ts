import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Pagination Component — green primary, clean pill buttons
 */
@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flex items-center justify-between gap-4 flex-wrap">
      <!-- Info -->
      <span class="text-sm text-gray-500 dark:text-gray-400">
        Page <span class="font-semibold text-gray-700 dark:text-gray-200">{{ currentPage }}</span>
        of
        <span class="font-semibold text-gray-700 dark:text-gray-200">{{ totalPages }}</span>
      </span>

      <!-- Controls -->
      <div class="flex items-center gap-1">
        <!-- Prev -->
        <button
          (click)="previousPage()"
          [disabled]="currentPage === 1"
          class="flex items-center gap-1.5 px-3 py-2 text-sm font-medium rounded-xl
                 border border-surface-200 dark:border-surface-700
                 bg-white dark:bg-surface-800
                 text-gray-700 dark:text-gray-300
                 hover:bg-surface-50 dark:hover:bg-surface-700
                 disabled:opacity-40 disabled:pointer-events-none
                 transition-all duration-200"
        >
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"/>
          </svg>
          <span class="hidden sm:inline">Prev</span>
        </button>

        <!-- Page numbers -->
        <div class="flex gap-1">
          <button
            *ngFor="let page of getPageNumbers()"
            (click)="goToPage(page)"
            [ngClass]="page === currentPage
              ? 'bg-primary-600 text-white border-primary-600 shadow-sm'
              : 'bg-white dark:bg-surface-800 text-gray-700 dark:text-gray-300 border-surface-200 dark:border-surface-700 hover:bg-surface-50 dark:hover:bg-surface-700'"
            class="w-9 h-9 flex items-center justify-center text-sm font-medium rounded-xl
                   border transition-all duration-200"
          >
            {{ page }}
          </button>
        </div>

        <!-- Next -->
        <button
          (click)="nextPage()"
          [disabled]="currentPage === totalPages"
          class="flex items-center gap-1.5 px-3 py-2 text-sm font-medium rounded-xl
                 border border-surface-200 dark:border-surface-700
                 bg-white dark:bg-surface-800
                 text-gray-700 dark:text-gray-300
                 hover:bg-surface-50 dark:hover:bg-surface-700
                 disabled:opacity-40 disabled:pointer-events-none
                 transition-all duration-200"
        >
          <span class="hidden sm:inline">Next</span>
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
          </svg>
        </button>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PaginationComponent {
  @Input() currentPage = 1;
  @Input() totalPages = 1;
  @Input() maxButtons = 5;

  @Output() pageChange = new EventEmitter<number>();

  previousPage(): void { if (this.currentPage > 1) this.goToPage(this.currentPage - 1); }
  nextPage():     void { if (this.currentPage < this.totalPages) this.goToPage(this.currentPage + 1); }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) this.pageChange.emit(page);
  }

  getPageNumbers(): number[] {
    const half  = Math.floor(this.maxButtons / 2);
    let start   = Math.max(1, this.currentPage - half);
    let end     = Math.min(this.totalPages, this.currentPage + half);
    if (end - start < this.maxButtons - 1) {
      if (start === 1) end   = Math.min(this.totalPages, start + this.maxButtons - 1);
      else             start = Math.max(1, end - this.maxButtons + 1);
    }
    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  }
}
