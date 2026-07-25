import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';

export interface TopbarAction {
  id: string;
  icon: string;
  label: string;
  badge?: number;
}

/**
 * Topbar Component — clean header, no border-line accents
 */
@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <header class="flex items-center justify-between gap-4
                   px-4 sm:px-6 h-16 shrink-0
                   bg-white dark:bg-surface-900
                   border-b border-surface-200 dark:border-surface-800">

      <!-- Left: hamburger + title -->
      <div class="flex items-center gap-3 min-w-0">
        <button
          (click)="toggleSidebar.emit()"
          class="flex items-center justify-center w-9 h-9 rounded-xl
                 text-gray-500 hover:text-gray-700 dark:hover:text-gray-200
                 hover:bg-surface-100 dark:hover:bg-surface-800
                 transition-all duration-200 shrink-0"
          aria-label="Toggle sidebar"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"/>
          </svg>
        </button>
        <h1 *ngIf="title" class="text-base font-semibold text-gray-900 dark:text-white truncate">
          {{ title }}
        </h1>
      </div>

      <!-- Right: actions + user -->
      <div class="flex items-center gap-1.5 shrink-0">
        <!-- Action buttons -->
        <button
          *ngFor="let action of actions"
          (click)="actionClick.emit(action)"
          [title]="action.label"
          class="relative flex items-center justify-center w-9 h-9 rounded-xl
                 text-gray-500 hover:text-gray-700 dark:hover:text-gray-200
                 hover:bg-surface-100 dark:hover:bg-surface-800
                 transition-all duration-200"
        >
          <span class="text-base leading-none">{{ action.icon }}</span>
          <span
            *ngIf="action.badge"
            class="absolute top-1 right-1 flex items-center justify-center
                   min-w-[1rem] h-4 px-0.5
                   text-2xs font-bold rounded-full
                   bg-red-500 text-white"
          >
            {{ action.badge > 9 ? '9+' : action.badge }}
          </span>
        </button>

        <!-- Divider -->
        <div class="w-px h-6 bg-surface-200 dark:bg-surface-700 mx-1"></div>

        <!-- User menu -->
        <div class="relative">
          <button
            (click)="userMenuOpen = !userMenuOpen"
            class="flex items-center gap-2.5 px-2 py-1.5 rounded-xl
                   hover:bg-surface-100 dark:hover:bg-surface-800
                   transition-all duration-200"
          >
            <!-- Avatar -->
            <div *ngIf="userAvatar; else initials"
              class="w-8 h-8 rounded-xl overflow-hidden ring-2 ring-primary-200 dark:ring-primary-800">
              <img [src]="userAvatar" [alt]="userName" class="w-full h-full object-cover"/>
            </div>
            <ng-template #initials>
              <div class="w-8 h-8 rounded-xl bg-gradient-to-br from-primary-400 to-primary-600
                          flex items-center justify-center text-white text-xs font-bold
                          ring-2 ring-primary-200 dark:ring-primary-800">
                {{ getInitials() }}
              </div>
            </ng-template>

            <span class="hidden sm:block text-sm font-medium text-gray-700 dark:text-gray-200 max-w-[8rem] truncate">
              {{ userName || 'User' }}
            </span>
            <svg class="w-3.5 h-3.5 text-gray-400 hidden sm:block transition-transform duration-200"
              [class.rotate-180]="userMenuOpen"
              fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"/>
            </svg>
          </button>

          <!-- Dropdown -->
          <div
            *ngIf="userMenuOpen"
            @menuAnim
            class="absolute right-0 top-full mt-2 w-52
                   bg-white dark:bg-surface-800
                   rounded-2xl shadow-lg
                   border border-surface-200 dark:border-surface-700
                   z-50 overflow-hidden py-1.5"
          >
            <!-- User info -->
            <div class="px-4 py-3 border-b border-surface-100 dark:border-surface-700 mb-1">
              <p class="text-sm font-semibold text-gray-900 dark:text-white truncate">{{ userName }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400">Administrator</p>
            </div>

            <a href="/profile"
              class="flex items-center gap-2.5 px-4 py-2 text-sm text-gray-700 dark:text-gray-200
                     hover:bg-surface-50 dark:hover:bg-surface-700 transition-colors">
              <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
              </svg>
              Profile
            </a>
            <a href="/settings"
              class="flex items-center gap-2.5 px-4 py-2 text-sm text-gray-700 dark:text-gray-200
                     hover:bg-surface-50 dark:hover:bg-surface-700 transition-colors">
              <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/>
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/>
              </svg>
              Settings
            </a>
            <hr class="my-1 border-surface-200 dark:border-surface-700"/>
            <button
              (click)="logout.emit(); userMenuOpen = false"
              class="w-full flex items-center gap-2.5 px-4 py-2 text-sm text-red-600 dark:text-red-400
                     hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
            >
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"/>
              </svg>
              Sign out
            </button>
          </div>
        </div>
      </div>
    </header>
  `,
  animations: [
    trigger('menuAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.96) translateY(-6px)' }),
        animate('200ms cubic-bezier(0.16, 1, 0.3, 1)',
          style({ opacity: 1, transform: 'scale(1) translateY(0)' })),
      ]),
      transition(':leave', [
        animate('130ms ease-in',
          style({ opacity: 0, transform: 'scale(0.96) translateY(-4px)' })),
      ]),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopbarComponent {
  @Input() title = '';
  @Input() actions: TopbarAction[] = [];
  @Input() userName = '';
  @Input() userAvatar = '';

  @Output() actionClick    = new EventEmitter<TopbarAction>();
  @Output() toggleSidebar  = new EventEmitter<void>();
  @Output() logout         = new EventEmitter<void>();

  userMenuOpen = false;

  getInitials(): string {
    return this.userName
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(n => n[0].toUpperCase())
      .join('');
  }
}
