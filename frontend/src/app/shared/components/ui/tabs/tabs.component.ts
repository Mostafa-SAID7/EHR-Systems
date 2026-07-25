import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface Tab {
  id: string;
  label: string;
  icon?: string;
  badge?: number;
  disabled?: boolean;
}

/**
 * Tabs Component — pill-style active indicator, no generic border-line
 */
@Component({
  selector: 'app-tabs',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div>
      <!-- Tab strip -->
      <div class="flex gap-1 p-1 bg-surface-100 dark:bg-surface-800 rounded-xl overflow-x-auto">
        <button
          *ngFor="let tab of tabs"
          (click)="!tab.disabled && selectTab(tab.id)"
          [disabled]="tab.disabled"
          [ngClass]="activeTab === tab.id
            ? 'bg-white dark:bg-surface-700 text-primary-700 dark:text-primary-400 shadow-sm font-semibold'
            : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 hover:bg-white/50 dark:hover:bg-surface-700/50'"
          class="relative flex items-center gap-2 px-4 py-2 rounded-lg text-sm
                 transition-all duration-200 whitespace-nowrap
                 disabled:opacity-40 disabled:pointer-events-none"
        >
          <span *ngIf="tab.icon" class="text-base leading-none">{{ tab.icon }}</span>
          {{ tab.label }}
          <span
            *ngIf="tab.badge"
            class="inline-flex items-center justify-center min-w-[1.25rem] h-5 px-1.5
                   text-2xs font-bold rounded-full
                   bg-primary-500 text-white"
          >
            {{ tab.badge > 99 ? '99+' : tab.badge }}
          </span>
        </button>
      </div>

      <!-- Content -->
      <div class="mt-4 animate-fade-in">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TabsComponent {
  @Input() tabs: Tab[] = [];
  @Input() activeTab = '';
  @Output() tabChange = new EventEmitter<string>();

  selectTab(id: string): void {
    this.activeTab = id;
    this.tabChange.emit(id);
  }
}
