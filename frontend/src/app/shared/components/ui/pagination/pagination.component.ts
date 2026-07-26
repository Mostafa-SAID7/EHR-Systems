import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Pagination Component — green primary, clean pill buttons
 */
@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pagination.component.html',
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
