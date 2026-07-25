import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

/**
 * Auth Layout — cinematic green gradient, medical branding
 */
@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="min-h-screen flex">
      <!-- Left panel — brand / illustration (hidden on mobile) -->
      <div class="hidden lg:flex lg:w-1/2 xl:w-3/5
                  flex-col items-center justify-center
                  bg-gradient-to-br from-primary-600 via-primary-700 to-primary-900
                  relative overflow-hidden px-12 py-16">

        <!-- Background decoration -->
        <div class="absolute inset-0 pointer-events-none">
          <div class="absolute -top-24 -left-24 w-96 h-96 rounded-full bg-white/5 blur-3xl"></div>
          <div class="absolute -bottom-24 -right-12 w-80 h-80 rounded-full bg-primary-400/20 blur-3xl"></div>
          <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] rounded-full bg-white/3 blur-3xl"></div>
        </div>

        <!-- Grid pattern overlay -->
        <div class="absolute inset-0 opacity-5"
          style="background-image: linear-gradient(rgba(255,255,255,.15) 1px, transparent 1px),
                                   linear-gradient(to right, rgba(255,255,255,.15) 1px, transparent 1px);
                 background-size: 40px 40px;"></div>

        <!-- Content -->
        <div class="relative z-10 text-center max-w-md animate-fade-in-up">
          <!-- Logo mark -->
          <div class="inline-flex items-center justify-center w-20 h-20 rounded-3xl bg-white/15
                      backdrop-blur-sm border border-white/20 mb-8 shadow-xl">
            <svg class="w-10 h-10 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01m-.01 4h.01"/>
            </svg>
          </div>

          <h1 class="text-4xl font-bold text-white mb-3 tracking-tight">EHR Platform</h1>
          <p class="text-primary-200 text-lg leading-relaxed mb-10">
            Modern, secure healthcare management for the people who matter most.
          </p>

          <!-- Feature pills -->
          <div class="flex flex-wrap justify-center gap-2">
            <span *ngFor="let f of features"
              class="px-3 py-1.5 rounded-full text-xs font-medium
                     bg-white/10 text-white border border-white/15 backdrop-blur-sm">
              {{ f }}
            </span>
          </div>
        </div>
      </div>

      <!-- Right panel — form -->
      <div class="flex-1 flex flex-col items-center justify-center
                  bg-surface-50 dark:bg-surface-900
                  px-4 sm:px-8 py-12">

        <!-- Mobile logo -->
        <div class="flex items-center gap-3 mb-10 lg:hidden">
          <div class="w-10 h-10 rounded-xl bg-primary-600 flex items-center justify-center text-white shadow-sm">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
            </svg>
          </div>
          <span class="text-lg font-bold text-gray-900 dark:text-white">EHR Platform</span>
        </div>

        <!-- Form card -->
        <div class="w-full max-w-md">
          <div class="bg-white dark:bg-surface-800
                      rounded-3xl shadow-xl
                      border border-surface-200 dark:border-surface-700
                      p-8 animate-scale-in">
            <router-outlet></router-outlet>
          </div>
        </div>

        <!-- Footer -->
        <p class="mt-8 text-xs text-gray-400 dark:text-gray-600 text-center">
          HIPAA compliant · SOC 2 Type II · 256-bit encryption
        </p>
      </div>
    </div>
  `,
})
export class AuthLayoutComponent {
  features = ['Patient Management', 'eRx', 'Lab Results', 'Billing', 'HIPAA Compliant'];
}
