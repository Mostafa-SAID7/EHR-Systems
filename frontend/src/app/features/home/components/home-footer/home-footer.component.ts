import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-footer',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <footer class="bg-white/70 dark:bg-[#0c1410] border-t border-primary-100/60 dark:border-primary-900/30 py-12">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex flex-col md:flex-row items-center justify-between gap-6">
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-primary-600 flex items-center justify-center text-white font-bold text-sm shadow-sm">
              E
            </div>
            <span class="font-bold text-gray-900 dark:text-white">EHR Platform</span>
            <span class="text-xs text-gray-500">&bull; Enterprise Healthcare Solutions</span>
          </div>

          <div class="flex items-center gap-6 text-xs font-medium text-gray-600 dark:text-gray-400">
            <a routerLink="/auth/login" class="hover:text-primary-600 transition-colors">Sign In</a>
            <button (click)="quickLogin.emit('doctor@ehr.com')" class="cursor-pointer hover:text-primary-600 transition-colors">Doctor Demo</button>
            <button (click)="quickLogin.emit('admin@ehr.com')" class="cursor-pointer hover:text-primary-600 transition-colors">Admin Portal</button>
            <a routerLink="/reports/compliance" class="hover:text-primary-600 transition-colors">Compliance Center</a>
          </div>

          <div class="text-xs text-gray-500">
            &copy; 2026 EHR Platform. All rights reserved.
          </div>
        </div>
      </div>
    </footer>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeFooterComponent {
  @Output() quickLogin = new EventEmitter<string>();
}
