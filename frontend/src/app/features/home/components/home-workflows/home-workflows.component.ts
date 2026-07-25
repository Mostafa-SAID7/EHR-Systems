import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-workflows',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <section id="workflows" class="py-20 border-t border-primary-100/60 dark:border-primary-900/30">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="text-center max-w-2xl mx-auto mb-16">
          <h2 class="text-xs font-bold text-primary-600 dark:text-primary-400 uppercase tracking-widest mb-2">Streamlined Clinical Operations</h2>
          <p class="text-3xl font-extrabold text-gray-900 dark:text-white tracking-tight">
            End-to-End Care Delivery Workflows
          </p>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-4 gap-6">
          <div class="p-6 rounded-2xl bg-white dark:bg-[#141e16] border border-green-100/60 dark:border-green-900/20 shadow-card relative">
            <span class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-900/60 text-primary-700 dark:text-primary-300 font-bold text-sm flex items-center justify-center mb-4">1</span>
            <h4 class="font-bold text-lg text-gray-900 dark:text-white mb-2">Patient Intake</h4>
            <p class="text-xs text-gray-500 dark:text-gray-400 leading-relaxed mb-4">Fast digital registration, demographic verification, and insurance eligibility checks.</p>
            <a routerLink="/patients/search" class="text-xs font-semibold text-primary-600 dark:text-primary-400 hover:underline">Intake Search &rarr;</a>
          </div>

          <div class="p-6 rounded-2xl bg-white dark:bg-[#141e16] border border-green-100/60 dark:border-green-900/20 shadow-card relative">
            <span class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-900/60 text-primary-700 dark:text-primary-300 font-bold text-sm flex items-center justify-center mb-4">2</span>
            <h4 class="font-bold text-lg text-gray-900 dark:text-white mb-2">Triage &amp; Vitals</h4>
            <p class="text-xs text-gray-500 dark:text-gray-400 leading-relaxed mb-4">Immediate vital sign logging (BP, HR, SpO2) with automatic baseline alert triggers.</p>
            <a routerLink="/clinical/vitals" class="text-xs font-semibold text-primary-600 dark:text-primary-400 hover:underline">Vitals Log &rarr;</a>
          </div>

          <div class="p-6 rounded-2xl bg-white dark:bg-[#141e16] border border-green-100/60 dark:border-green-900/20 shadow-card relative">
            <span class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-900/60 text-primary-700 dark:text-primary-300 font-bold text-sm flex items-center justify-center mb-4">3</span>
            <h4 class="font-bold text-lg text-gray-900 dark:text-white mb-2">Consult &amp; e-Rx</h4>
            <p class="text-xs text-gray-500 dark:text-gray-400 leading-relaxed mb-4">Physician charting, ordering lab tests, and submitting verified e-prescriptions instantly.</p>
            <a routerLink="/prescriptions/new" class="text-xs font-semibold text-primary-600 dark:text-primary-400 hover:underline">New e-Rx &rarr;</a>
          </div>

          <div class="p-6 rounded-2xl bg-white dark:bg-[#141e16] border border-green-100/60 dark:border-green-900/20 shadow-card relative">
            <span class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-900/60 text-primary-700 dark:text-primary-300 font-bold text-sm flex items-center justify-center mb-4">4</span>
            <h4 class="font-bold text-lg text-gray-900 dark:text-white mb-2">Billing &amp; Audit</h4>
            <p class="text-xs text-gray-500 dark:text-gray-400 leading-relaxed mb-4">Automated claim coding, patient invoicing, and complete HIPAA compliance logging.</p>
            <a routerLink="/reports/compliance" class="text-xs font-semibold text-primary-600 dark:text-primary-400 hover:underline">Compliance Log &rarr;</a>
          </div>
        </div>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeWorkflowsComponent {}
