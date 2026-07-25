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

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <aside
      [class.w-64]="!collapsed"
      [class.w-16]="collapsed"
      class="flex flex-col h-screen shrink-0 overflow-hidden
             bg-white dark:bg-surface-900
             border-r border-primary-100/60 dark:border-primary-900/30
             transition-all duration-300 ease-smooth"
    >
      <!-- ── Logo ──────────────────────────────── -->
      <div class="flex items-center gap-3 px-4 py-5 shrink-0">
        <div class="flex items-center justify-center w-9 h-9 rounded-xl
                    bg-gradient-to-br from-primary-500 to-primary-700
                    text-white shrink-0 shadow-md animate-glow-pulse">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2
                 M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2
                 m-3 7h3m-3 4h3m-6-4h.01m-.01 4h.01"/>
          </svg>
        </div>
        <div *ngIf="!collapsed" class="min-w-0 animate-fade-in">
          <p class="text-sm font-bold text-gray-900 dark:text-white truncate leading-tight">EHR Platform</p>
          <p class="text-2xs text-primary-500 dark:text-primary-500 truncate font-medium">Healthcare Management</p>
        </div>
        <button
          (click)="toggleCollapse()"
          class="ml-auto btn-icon-sm shrink-0"
          [attr.aria-label]="collapsed ? 'Expand sidebar' : 'Collapse sidebar'"
        >
          <svg class="w-4 h-4 transition-transform duration-300"
            [class.rotate-180]="collapsed"
            fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
              d="M11 19l-7-7 7-7m8 14l-7-7 7-7"/>
          </svg>
        </button>
      </div>

      <!-- ── Divider ────────────────────────────── -->
      <div class="mx-4 h-px bg-gradient-to-r from-transparent via-primary-100 to-transparent dark:via-primary-900/30 mb-3"></div>

      <!-- ── Nav ───────────────────────────────── -->
      <nav class="flex-1 overflow-y-auto px-2.5 pb-4 space-y-0.5">
        <ng-container *ngFor="let item of navItems">

          <!-- Parent item -->
          <button
            [routerLink]="item.children ? null : item.route"
            [routerLinkActive]="item.children ? '' : 'nav-item-active'"
            (click)="toggleItem(item)"
            [title]="collapsed ? item.label : ''"
            class="nav-item w-full"
            [class.justify-center]="collapsed"
            [class.justify-between]="!collapsed"
          >
            <div class="flex items-center gap-3 min-w-0">
              <svg class="w-[18px] h-[18px] shrink-0 transition-colors"
                fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
                  [attr.d]="item.icon"/>
              </svg>
              <span *ngIf="!collapsed" class="truncate animate-fade-in">{{ item.label }}</span>
            </div>

            <div *ngIf="!collapsed" class="flex items-center gap-1.5 shrink-0">
              <span *ngIf="item.badge"
                class="flex items-center justify-center min-w-[1.25rem] h-5 px-1
                       text-2xs font-bold rounded-full
                       bg-primary-500 text-white shadow-sm">
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

          <!-- Children — clean indent, no border-line gutters -->
          <div
            *ngIf="item.children && item.expanded && !collapsed"
            @expandCollapse
            class="ml-7 mt-0.5 mb-1 space-y-0.5 pl-2"
          >
            <button
              *ngFor="let child of item.children"
              [routerLink]="child.route"
              routerLinkActive="text-primary-700 dark:text-primary-300 font-semibold bg-primary-50 dark:bg-primary-900/20"
              class="w-full text-left px-3 py-2 text-sm text-gray-500 dark:text-gray-400
                     hover:text-primary-700 dark:hover:text-primary-300
                     hover:bg-primary-50/60 dark:hover:bg-primary-900/15
                     rounded-xl transition-all duration-150"
            >
              {{ child.label }}
            </button>
          </div>

        </ng-container>
      </nav>

      <!-- ── Divider ─────────────────────────────── -->
      <div class="mx-4 h-px bg-gradient-to-r from-transparent via-primary-100 to-transparent dark:via-primary-900/30"></div>

      <!-- ── User area ──────────────────────────── -->
      <div class="shrink-0 px-2.5 py-3">
        <div
          [class.justify-center]="collapsed"
          class="flex items-center gap-3 px-2 py-2.5 rounded-xl
                 hover:bg-primary-50/60 dark:hover:bg-primary-900/20
                 transition-colors duration-200 cursor-pointer">
          <div class="w-8 h-8 rounded-xl shrink-0
                      bg-gradient-to-br from-primary-400 to-primary-700
                      flex items-center justify-center
                      text-white text-xs font-bold shadow-sm">
            Dr
          </div>
          <div *ngIf="!collapsed" class="min-w-0 animate-fade-in flex-1">
            <p class="text-xs font-semibold text-gray-900 dark:text-white truncate leading-tight">Dr. Admin</p>
            <p class="text-2xs text-primary-500 dark:text-primary-500 truncate">Administrator</p>
          </div>
          <svg *ngIf="!collapsed" class="w-3.5 h-3.5 text-gray-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 9l4-4 4 4m0 6l-4 4-4-4"/>
          </svg>
        </div>
      </div>
    </aside>
  `,
  animations: [
    trigger('expandCollapse', [
      transition(':enter', [
        style({ opacity: 0, height: 0, overflow: 'hidden' }),
        animate('220ms cubic-bezier(0.16, 1, 0.3, 1)', style({ opacity: 1, height: '*' })),
      ]),
      transition(':leave', [
        style({ overflow: 'hidden' }),
        animate('160ms ease-in', style({ opacity: 0, height: 0 })),
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
