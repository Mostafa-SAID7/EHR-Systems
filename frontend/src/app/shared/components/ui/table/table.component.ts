import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface TableColumn {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  align?: 'left' | 'center' | 'right';
}

export interface SortEvent {
  column: string;
  direction: 'asc' | 'desc';
}

/**
 * Table Component — uses .table-base from design system
 */
@Component({
  selector: 'app-table',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './table.component.html',
  host: {
    class: 'w-full block overflow-x-auto'
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() data: any[] = [];
  @Input() emptyMessage = 'No data available';

  @Output() sort = new EventEmitter<SortEvent>();

  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  onSort(column: string): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.sort.emit({ column: this.sortColumn, direction: this.sortDirection });
  }
}
