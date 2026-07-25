import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-auth-header-logo',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="text-center mb-8">
      <div class="inline-flex items-center justify-center w-14 h-14 rounded-2xl bg-gradient-to-tr from-primary-700 via-primary-600 to-primary-400 text-white shadow-lg shadow-primary-600/25 mb-4 animate-glow-pulse">
        <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"/>
        </svg>
      </div>
      <h1 class="text-2xl font-extrabold tracking-tight text-gray-900 dark:text-white">{{ title }}</h1>
      <p class="text-xs text-gray-500 dark:text-gray-400 mt-1">{{ subtitle }}</p>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthHeaderLogoComponent {
  @Input() title = 'EHR Platform';
  @Input() subtitle = 'Enterprise Healthcare OS';
}
