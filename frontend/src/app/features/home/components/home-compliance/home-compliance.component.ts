import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-compliance',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <section id="security" class="py-16 bg-[#101912] text-white relative overflow-hidden border-t border-primary-900/40">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
        <div class="flex flex-col lg:flex-row items-center justify-between gap-10">
          <div class="max-w-xl text-center lg:text-left">
            <span class="text-xs font-bold text-primary-400 uppercase tracking-widest">Bank-Grade Data Protection</span>
            <h2 class="text-3xl font-bold mt-2 mb-4 text-white">Zero Trust Security &amp; End-to-End Encryption</h2>
            <p class="text-gray-300 text-sm leading-relaxed mb-6">
              All patient health information (PHI) is encrypted at rest using AES-256 and in transit via TLS 1.3. Immutable audit logs track every record access event.
            </p>
            <a routerLink="/reports/compliance" class="btn-primary inline-flex items-center gap-2">
              <span>View Compliance Center</span>
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 5l7 7m0 0l-7 7m7-7H3"/>
              </svg>
            </a>
          </div>
          
          <div class="flex flex-wrap items-center justify-center gap-4">
            <a routerLink="/reports/compliance" class="px-5 py-3 rounded-xl bg-[#141e16] hover:bg-[#1c2b1e] transition-colors border border-primary-900/40 text-center cursor-pointer">
              <div class="text-xs font-bold text-primary-400">HIPAA</div>
              <div class="text-[11px] text-gray-400">Compliant</div>
            </a>
            <a routerLink="/reports/compliance" class="px-5 py-3 rounded-xl bg-[#141e16] hover:bg-[#1c2b1e] transition-colors border border-primary-900/40 text-center cursor-pointer">
              <div class="text-xs font-bold text-primary-300">SOC 2</div>
              <div class="text-[11px] text-gray-400">Type II Verified</div>
            </a>
            <a routerLink="/reports/compliance" class="px-5 py-3 rounded-xl bg-[#141e16] hover:bg-[#1c2b1e] transition-colors border border-primary-900/40 text-center cursor-pointer">
              <div class="text-xs font-bold text-primary-400">FHIR R4</div>
              <div class="text-[11px] text-gray-400">Interoperable</div>
            </a>
            <a routerLink="/reports/compliance" class="px-5 py-3 rounded-xl bg-[#141e16] hover:bg-[#1c2b1e] transition-colors border border-primary-900/40 text-center cursor-pointer">
              <div class="text-xs font-bold text-primary-300">ISO 27001</div>
              <div class="text-[11px] text-gray-400">Certified</div>
            </a>
          </div>
        </div>
      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComplianceComponent {}
