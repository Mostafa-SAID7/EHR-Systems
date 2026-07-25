import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-solutions',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <section id="solutions" class="py-20 border-t border-primary-100/60 dark:border-primary-900/30">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center max-w-2xl mx-auto mb-16">
          <h2 class="text-xs font-bold text-primary-600 dark:text-primary-400 uppercase tracking-widest mb-2">Specialized Clinical Solutions</h2>
          <p class="text-3xl font-extrabold text-gray-900 dark:text-white tracking-tight">
            Tailored Tools for Every Healthcare Persona
          </p>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
          <div class="card-hover">
            <div class="w-12 h-12 rounded-xl bg-teal-100/80 dark:bg-teal-900/40 text-teal-700 dark:text-teal-300 flex items-center justify-center mb-6">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-2">Physicians &amp; Specialists</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 leading-relaxed mb-4">
              Rapid charting, customizable SOAP note templates, lab ordering, and instant diagnosis search.
            </p>
            <a routerLink="/medical-records" class="link-primary">Open Medical Records &rarr;</a>
          </div>

          <div class="card-hover">
            <div class="w-12 h-12 rounded-xl bg-primary-100/80 dark:bg-primary-900/40 text-primary-700 dark:text-primary-300 flex items-center justify-center mb-6">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/>
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-2">Nurses &amp; Triage Teams</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 leading-relaxed mb-4">
              Real-time vital entry, bed tracking, patient check-in queues, and immediate allergy alerts.
            </p>
            <a routerLink="/clinical/vitals" class="link-primary">Open Vitals Tracker &rarr;</a>
          </div>

          <div class="card-hover">
            <div class="w-12 h-12 rounded-xl bg-amber-100/80 dark:bg-amber-900/40 text-amber-700 dark:text-amber-300 flex items-center justify-center mb-6">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z"/>
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-2">Billing &amp; Practice Managers</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 leading-relaxed mb-4">
              Automated claim scrubbers, ICD-10/CPT code integration, revenue dashboards, and invoicing.
            </p>
            <a routerLink="/billing" class="link-primary">Open Billing Hub &rarr;</a>
          </div>
        </div>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeSolutionsComponent {}
