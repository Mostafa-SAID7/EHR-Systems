import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface AuthHighlight {
  value: string;
  label: string;
}

@Component({
  selector: 'app-auth-brand-panel',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="hidden lg:flex lg:w-[55%] flex-col items-center justify-center relative overflow-hidden bg-gradient-to-br from-primary-700 via-primary-600 to-teal-700 px-12 py-16">
      
      <!-- Layered ambient blobs — no hard borders -->
      <div class="absolute inset-0 pointer-events-none overflow-hidden">
        <div class="absolute -top-40 -left-40 w-[500px] h-[500px] rounded-full bg-white/5 blur-[80px]"></div>
        <div class="absolute -bottom-32 -right-20 w-[400px] h-[400px] rounded-full bg-primary-400/15 blur-[60px]"></div>
        <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[700px] h-[700px] rounded-full bg-teal-500/8 blur-[100px]"></div>
        <!-- Subtle grid dots -->
        <div class="absolute inset-0 opacity-[0.04]"
          style="background-image: radial-gradient(circle, rgba(255,255,255,0.8) 1px, transparent 1px); background-size: 36px 36px;"></div>
      </div>

      <!-- Content -->
      <div class="relative z-10 text-center max-w-lg w-full animate-fade-in-up">
        <!-- Logo mark -->
        <div class="inline-flex items-center justify-center w-24 h-24 rounded-3xl bg-white/12 backdrop-blur-md border border-white/20 mb-8 shadow-2xl animate-float">
          <svg class="w-12 h-12 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
              d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2 M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2 m-3 7h3m-3 4h3m-6-4h.01m-.01 4h.01"/>
          </svg>
        </div>

        <h1 class="text-4xl font-bold text-white mb-3 tracking-tight">EHR Platform</h1>
        <p class="text-primary-100 text-lg leading-relaxed mb-10 max-w-sm mx-auto">
          Modern, secure healthcare management for the people who matter most.
        </p>

        <!-- Stat highlights -->
        <div class="grid grid-cols-3 gap-3 mb-10">
          <div *ngFor="let s of highlights; trackBy: trackByValue" class="rounded-2xl bg-white/10 backdrop-blur-sm border border-white/15 p-4 text-center">
            <p class="text-2xl font-bold text-white">{{ s.value }}</p>
            <p class="text-xs text-primary-200 mt-0.5 font-medium">{{ s.label }}</p>
          </div>
        </div>

        <!-- Feature pills -->
        <div class="flex flex-wrap justify-center gap-2">
          <span *ngFor="let f of features; trackBy: trackByValue" class="flex items-center gap-1.5 px-3.5 py-1.5 rounded-full text-xs font-semibold bg-white/10 text-white border border-white/15 backdrop-blur-sm">
            <svg class="w-3 h-3 text-primary-300" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"/>
            </svg>
            {{ f }}
          </span>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthBrandPanelComponent {
  @Input() highlights: AuthHighlight[] = [];
  @Input() features: string[] = [];
  trackByValue(_: number, val: string | AuthHighlight): string {
    return typeof val === 'string' ? val : val.value + val.label;
  }
}
