import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';

export interface NavItem {
  id: string;
  label: string;
  icon: string;
  route?: string;
  children?: NavItem[];
  badge?: number;
  expanded?: boolean;
}

/**
 * Sidebar Component — green brand, pill nav items, cinematic collapse
 */
@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <aside
      [class.w-64]="!collapsed"
      [class.w-18]="collapsed"
      class="flex flex-col h-screen
             bg-white dark:bg-surface-900
             border-r border-surface-200 dark:border-surface-800
             transition-all duration-300 ease-smooth
             overflow-hidden shrink-0"
    >
      <!-- Logo header -->
      <div class="flex items-center gap-3 px-4 py-5 border-b border-surface-100 dark:border-surface-800 shrink-0">
        <!-- Icon mark -->
        <div class="flex items-center justify-center w-9 h-9 rounded-xl bg-primary-600 text-white shrink-0 shadow-sm">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01m-.01 4h.01"/>
          </svg>
        </div>
        <div *ngIf="!collapsed" class="min-w-0 animate-fade-in">
          <p class="text-sm font-bold text-gray-900 dark:text-white truncate">EHR Platform</p>
          <p class="text-2xs text-gray-500 dark:text-gray-400">Healthcare Management</p>
        </div>
        <!-- Collapse toggle -->
        <button
          (click)="toggleCollapse()"
          class="ml-auto flex items-center justify-center w-7 h-7 rounded-lg
                 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200
                 hover:bg-surface-100 dark:hover:bg-surface-800
                 transition-all duration-200 shrink-0"
          [attr.aria-label]="collapsed ? 'Expand sidebar' : 'Collapse sidebar'"
        >
          <svg class="w-4 h-4 transition-transform duration-300"
            [class.rotate-180]="collapsed"
            fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 19l-7-7 7-7m8 14l-7-7 7-7"/>
          </svg>
        </button>
      </div>

      <!-- Nav -->
      <nav class="flex-1 overflow-y-auto px-3 py-4 space-y-0.5">
        <ng-container *ngFor="let item of navItems">
          <!-- Main item -->
          <button
            [routerLink]="item.route"
            routerLinkActive="bg-primary-50 dark:bg-primary-900/25 text-primary-700 dark:text-primary-400 font-semibold"
            (click)="toggleItem(item)"
            [title]="collapsed ? item.label : ''"
            class="w-full flex items-center justify-between px-3 py-2.5 rounded-xl
                   text-sm text-gray-700 dark:text-gray-300 font-medium
                   hover:bg-surface-100 dark:hover:bg-surface-800
                   hover:text-gray-900 dark:hover:text-white
                   transition-all duration-200 group"
          >
            <div class="flex items-center gap-3 min-w-0">
              <span class="text-lg leading-none shrink-0 w-5 text-center">{{ item.icon }}</span>
              <span *ngIf="!collapsed" class="truncate animate-fade-in">{{ item.label }}</span>
            </div>
            <div *ngIf="!collapsed" class="flex items-center gap-1.5 shrink-0">
              <span *ngIf="item.badge"
                class="flex items-center justify-center min-w-[1.25rem] h-5 px-1
                       text-2xs font-bold rounded-full bg-red-500 text-white">
                {{ item.badge > 99 ? '99+' : item.badge }}
              </span>
              <svg *ngIf="item.children"
                class="w-3.5 h-3.5 text-gray-400 transition-transform duration-200"
                [class.rotate-180]="item.expanded"
                fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
              </svg>
            </div>
          </button>

          <!-- Sub items -->
          <div
            *ngIf="item.children && item.expanded && !collapsed"
            @expandCollapse
            class="ml-4 pl-3 mt-0.5 mb-1 space-y-0.5"
            style="border-left: 2px solid transparent;
                   background: linear-gradient(#e2fce9, #e2fce9) padding-box,
                               linear-gradient(to bottom, #86efac, transparent) border-box;"
          >
            <button
              *ngFor="let child of item.children"
              [routerLink]="child.route"
              routerLinkActive="text-primary-700 dark:text-primary-400 font-semibold bg-primary-50 dark:bg-primary-900/20"
              class="w-full text-left px-3 py-2 text-sm text-gray-600 dark:text-gray-400
                     hover:text-gray-900 dark:hover:text-white
                     hover:bg-surface-100 dark:hover:bg-surface-800
                     rounded-lg transition-colors duration-150"
            >
              {{ child.label }}
            </button>
          </div>
        </ng-container>
      </nav>

      <!-- Bottom user area -->
      <div class="shrink-0 px-3 py-3 border-t border-surface-100 dark:border-surface-800">
        <div [class.justify-center]="collapsed"
          class="flex items-center gap-3 px-2 py-2 rounded-xl
                 hover:bg-surface-100 dark:hover:bg-surface-800
                 transition-colors duration-200 cursor-pointer">
          <div class="w-8 h-8 rounded-xl bg-gradient-to-br from-primary-400 to-primary-600
                      flex items-center justify-center text-white text-xs font-bold shrink-0">
            Dr
          </div>
          <div *ngIf="!collapsed" class="min-w-0 animate-fade-in">
            <p class="text-xs font-semibold text-gray-900 dark:text-white truncate">Dr. Admin</p>
            <p class="text-2xs text-gray-500 dark:text-gray-400 truncate">Administrator</p>
          </div>
        </div>
      </div>
    </aside>
  `,
  animations: [
    trigger('expandCollapse', [
      transition(':enter', [
        style({ opacity: 0, height: 0, overflow: 'hidden' }),
        animate('200ms ease-out', style({ opacity: 1, height: '*' })),
      ]),
      transition(':leave', [
        style({ overflow: 'hidden' }),
        animate('150ms ease-in', style({ opacity: 0, height: 0 })),
      ]),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  @Input() navItems: NavItem[] = [];
  @Input() collapsed = false;
  @Output() collapsedChange = new EventEmitter<boolean>();

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }

  toggleItem(item: NavItem): void {
    if (item.children) item.expanded = !item.expanded;
  }
}
