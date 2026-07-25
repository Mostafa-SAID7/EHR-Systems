import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-hero',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <section id="hero" class="relative pt-12 pb-24 md:pt-20 md:pb-32 overflow-hidden">
      <!-- Centralized Green Ambient Glows -->
      <div class="absolute top-1/4 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[650px] h-[650px] bg-primary-500/10 dark:bg-primary-500/15 rounded-full blur-3xl pointer-events-none"></div>
      <div class="absolute top-1/3 right-10 w-[450px] h-[450px] bg-primary-400/10 dark:bg-primary-400/15 rounded-full blur-3xl pointer-events-none"></div>

      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative z-10">
        <div class="text-center max-w-3xl mx-auto">
          
          <!-- Pill Badge -->
          <div class="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-primary-50/80 dark:bg-primary-950/60 border border-primary-200/80 dark:border-primary-800/60 text-primary-700 dark:text-primary-300 text-xs font-semibold tracking-wide mb-6 shadow-xs">
            <span class="w-2 h-2 rounded-full bg-primary-500 animate-pulse"></span>
            Enterprise EHR Platform 2.0 &bull; Centralized Green Design System
          </div>

          <!-- Main Title -->
          <h1 class="text-4xl sm:text-5xl lg:text-6xl font-extrabold tracking-tight text-gray-900 dark:text-white leading-[1.15] mb-6">
            Next-Generation Clinical EHR &amp; Health Operations
          </h1>

          <!-- Subtitle -->
          <p class="text-lg sm:text-xl text-gray-600 dark:text-gray-300 mb-10 leading-relaxed font-normal">
            Empower care teams with instant patient timelines, e-prescribing, real-time lab diagnostics, and intelligent billing—all in one high-performance EHR Platform.
          </p>

          <!-- Hero Action Buttons -->
          <div class="flex flex-col sm:flex-row items-center justify-center gap-4 mb-14">
            <button (click)="quickLogin.emit('doctor@ehr.com')" class="w-full sm:w-auto cursor-pointer btn-primary text-base px-8 py-4 rounded-xl shadow-glow">
              <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"/>
              </svg>
              <span>Enter Doctor Demo Portal</span>
            </button>
            
            <a routerLink="/auth/login" class="w-full sm:w-auto btn-secondary text-base px-8 py-4 rounded-xl">
              <span>Sign In Page</span>
              <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
              </svg>
            </a>
          </div>

          <!-- Quick Login Persona Selector -->
          <div class="inline-flex flex-wrap items-center justify-center gap-2 p-2 rounded-2xl bg-white/80 dark:bg-surface-800/80 border border-primary-100 dark:border-primary-900/30 backdrop-blur-md shadow-xs">
            <span class="text-xs font-semibold text-gray-500 dark:text-gray-400 px-3">Quick Demo Access:</span>
            <button (click)="quickLogin.emit('doctor@ehr.com')" class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-primary-50 dark:bg-primary-950/60 text-primary-700 dark:text-primary-300 hover:bg-primary-100 transition-colors">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>
              </svg>
              <span>Doctor</span>
            </button>
            <button (click)="quickLogin.emit('admin@ehr.com')" class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-primary-100/70 dark:bg-primary-900/40 text-primary-800 dark:text-primary-200 hover:bg-primary-200 transition-colors">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"/>
              </svg>
              <span>Admin</span>
            </button>
            <button (click)="quickLogin.emit('nurse@ehr.com')" class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold bg-green-50 dark:bg-green-950/60 text-green-700 dark:text-green-300 hover:bg-green-100 transition-colors">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"/>
              </svg>
              <span>Nurse</span>
            </button>
          </div>

        </div>

        <!-- Hero UI Mockup Card -->
        <div class="mt-16 relative max-w-5xl mx-auto">
          <div class="rounded-2xl p-1 bg-gradient-to-b from-primary-200 via-primary-500/20 to-primary-950/40 shadow-2xl">
            <div class="rounded-xl bg-[#141e16] text-gray-100 p-6 md:p-8 overflow-hidden border border-green-900/30">
              <!-- Mock Top Toolbar -->
              <div class="flex items-center justify-between pb-6 border-b border-green-900/40 mb-6">
                <div class="flex items-center gap-2">
                  <span class="w-3 h-3 rounded-full bg-red-500"></span>
                  <span class="w-3 h-3 rounded-full bg-amber-500"></span>
                  <span class="w-3 h-3 rounded-full bg-primary-500"></span>
                  <span class="text-xs font-mono text-gray-400 ml-2">EHR Platform Workspace v2.4</span>
                </div>
                <div class="flex items-center gap-3">
                  <span class="px-2.5 py-1 rounded-md bg-primary-500/10 text-primary-400 text-xs font-mono border border-primary-500/20">● SYSTEM LIVE</span>
                  <span class="text-xs font-mono text-gray-400">Response: 14ms</span>
                </div>
              </div>

              <!-- Mock Dashboard Grid -->
              <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
                <div class="p-4 rounded-xl bg-surface-800/80 border border-primary-900/30">
                  <div class="text-gray-400 text-xs font-medium mb-1">Active Patients</div>
                  <div class="text-2xl font-bold text-white">1,248</div>
                  <div class="text-xs text-primary-400 mt-1 font-mono">↑ +12.4% this week</div>
                </div>
                <div class="p-4 rounded-xl bg-surface-800/80 border border-primary-900/30">
                  <div class="text-gray-400 text-xs font-medium mb-1">Today's Appointments</div>
                  <div class="text-2xl font-bold text-primary-400">42</div>
                  <div class="text-xs text-gray-400 mt-1 font-mono">8 in progress</div>
                </div>
                <div class="p-4 rounded-xl bg-surface-800/80 border border-primary-900/30">
                  <div class="text-gray-400 text-xs font-medium mb-1">e-Prescriptions Sent</div>
                  <div class="text-2xl font-bold text-primary-300">189</div>
                  <div class="text-xs text-primary-400 mt-1 font-mono">100% verified</div>
                </div>
                <div class="p-4 rounded-xl bg-surface-800/80 border border-primary-900/30">
                  <div class="text-gray-400 text-xs font-medium mb-1">Clean Claims Rate</div>
                  <div class="text-2xl font-bold text-primary-200">99.2%</div>
                  <div class="text-xs text-primary-300 mt-1 font-mono">RCM automated</div>
                </div>
              </div>

              <!-- Mock Patient Timeline Stream -->
              <div class="p-4 rounded-xl bg-surface-800/40 border border-primary-900/20">
                <div class="flex items-center justify-between mb-3">
                  <span class="text-xs font-semibold text-gray-300 uppercase tracking-wider">Live Clinical Stream</span>
                  <span class="text-[11px] text-primary-400 font-mono">Realtime updates</span>
                </div>
                <div class="space-y-2.5">
                  <div class="flex items-center justify-between p-2.5 rounded-lg bg-surface-800/80 border border-primary-900/30 text-xs">
                    <div class="flex items-center gap-3">
                      <span class="w-2 h-2 rounded-full bg-primary-400"></span>
                      <span class="font-semibold text-white">Eleanor Vance (MRN-4920)</span>
                      <span class="text-gray-400">SOAP Note added by Dr. Smith</span>
                    </div>
                    <span class="text-gray-500 font-mono">2 mins ago</span>
                  </div>
                  <div class="flex items-center justify-between p-2.5 rounded-lg bg-surface-800/80 border border-primary-900/30 text-xs">
                    <div class="flex items-center gap-3">
                      <span class="w-2 h-2 rounded-full bg-primary-500"></span>
                      <span class="font-semibold text-white">James Sterling (MRN-3104)</span>
                      <span class="text-gray-400">Amoxicillin 500mg e-Rx dispatched</span>
                    </div>
                    <span class="text-gray-500 font-mono">7 mins ago</span>
                  </div>
                </div>
              </div>

            </div>
          </div>
        </div>

      </div>
    </section>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeHeroComponent {
  @Output() quickLogin = new EventEmitter<string>();
}
