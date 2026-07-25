import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home-stats',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="border-y border-primary-100/60 dark:border-primary-900/30 bg-white/70 dark:bg-[#141e16]/70 backdrop-blur-md py-10">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="grid grid-cols-2 md:grid-cols-4 gap-8 text-center">
          <div>
            <div class="text-3xl sm:text-4xl font-extrabold text-primary-600 dark:text-primary-400 mb-1">99.99%</div>
            <div class="text-xs sm:text-sm font-medium text-gray-600 dark:text-gray-400">Uptime &amp; Reliability SLA</div>
          </div>
          <div>
            <div class="text-3xl sm:text-4xl font-extrabold text-primary-600 dark:text-primary-400 mb-1">15M+</div>
            <div class="text-xs sm:text-sm font-medium text-gray-600 dark:text-gray-400">Encrypted Patient Records</div>
          </div>
          <div>
            <div class="text-3xl sm:text-4xl font-extrabold text-primary-600 dark:text-primary-400 mb-1">&lt;150ms</div>
            <div class="text-xs sm:text-sm font-medium text-gray-600 dark:text-gray-400">Instant Patient Search</div>
          </div>
          <div>
            <div class="text-3xl sm:text-4xl font-extrabold text-primary-600 dark:text-primary-400 mb-1">100%</div>
            <div class="text-xs sm:text-sm font-medium text-gray-600 dark:text-gray-400">HIPAA &amp; FHIR R4 Compliant</div>
          </div>
        </div>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeStatsComponent {}
