import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ───────────────────────────────────── -->
      <div>
        <h1 class="heading-xl">Platform Settings</h1>
        <p class="body-text mt-1">Configure system preferences, security, and integrations</p>
      </div>

      <!-- ── Settings layout ──────────────────────────── -->
      <div class="flex flex-col lg:flex-row gap-6">

        <!-- Settings tabs (sidebar) -->
        <div class="lg:w-56 shrink-0">
          <nav class="card p-2 space-y-0.5">
            <button *ngFor="let s of sections"
              (click)="activeSection = s.key"
              [class]="activeSection === s.key
                ? 'flex items-center gap-3 w-full px-3 py-2.5 rounded-xl bg-primary-50 dark:bg-primary-900/30 text-primary-700 dark:text-primary-300 text-sm font-semibold'
                : 'flex items-center gap-3 w-full px-3 py-2.5 rounded-xl text-gray-600 dark:text-gray-400 text-sm font-medium hover:bg-surface-50 dark:hover:bg-surface-800/60 transition-colors'">
              <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="s.icon"/>
              </svg>
              {{ s.label }}
            </button>
          </nav>
        </div>

        <!-- Settings content -->
        <div class="flex-1 space-y-5">

          <!-- ── Profile ──────────────────────────────── -->
          <div *ngIf="activeSection === 'profile'" class="space-y-5">
            <div class="card space-y-5">
              <h2 class="heading-sm">Profile Information</h2>
              <!-- Avatar -->
              <div class="flex items-center gap-4">
                <div class="w-16 h-16 rounded-2xl flex items-center justify-center text-white text-xl font-bold"
                  style="background: linear-gradient(135deg,#15803d,#16a34a,#4ade80)">SA</div>
                <div>
                  <button class="btn-secondary btn-sm">Change Photo</button>
                  <p class="text-2xs text-gray-400 mt-1">JPG, PNG or GIF. Max 2MB.</p>
                </div>
              </div>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">First Name</label>
                  <input type="text" [(ngModel)]="profile.firstName" class="input-base w-full"/>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Last Name</label>
                  <input type="text" [(ngModel)]="profile.lastName" class="input-base w-full"/>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Email Address</label>
                  <input type="email" [(ngModel)]="profile.email" class="input-base w-full"/>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Phone</label>
                  <input type="tel" [(ngModel)]="profile.phone" class="input-base w-full"/>
                </div>
                <div class="sm:col-span-2">
                  <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Job Title / Specialization</label>
                  <input type="text" [(ngModel)]="profile.title" class="input-base w-full"/>
                </div>
              </div>
              <button (click)="saved = true" class="btn-primary btn-sm">
                Save Profile
              </button>
              <span *ngIf="saved" class="text-sm text-primary-600 dark:text-primary-400 font-semibold">✓ Profile updated!</span>
            </div>
          </div>

          <!-- ── Notifications ────────────────────────── -->
          <div *ngIf="activeSection === 'notifications'" class="card space-y-5">
            <h2 class="heading-sm">Notification Preferences</h2>
            <div class="space-y-4">
              <div *ngFor="let n of notifications" class="flex items-start justify-between gap-4 py-3 border-b border-surface-100 dark:border-surface-700/50 last:border-0">
                <div class="min-w-0">
                  <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ n.label }}</p>
                  <p class="text-xs text-gray-400 mt-0.5">{{ n.description }}</p>
                </div>
                <div class="flex items-center gap-3 shrink-0">
                  <label class="flex items-center gap-1.5 text-xs text-gray-500">
                    <input type="checkbox" [(ngModel)]="n.email" class="w-3.5 h-3.5 rounded accent-primary-600"/> Email
                  </label>
                  <label class="flex items-center gap-1.5 text-xs text-gray-500">
                    <input type="checkbox" [(ngModel)]="n.sms" class="w-3.5 h-3.5 rounded accent-primary-600"/> SMS
                  </label>
                </div>
              </div>
            </div>
            <button class="btn-primary btn-sm">Save Preferences</button>
          </div>

          <!-- ── Security ──────────────────────────────── -->
          <div *ngIf="activeSection === 'security'" class="space-y-5">
            <div class="card space-y-4">
              <h2 class="heading-sm">Change Password</h2>
              <div>
                <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Current Password</label>
                <input type="password" placeholder="••••••••" class="input-base w-full max-w-sm"/>
              </div>
              <div>
                <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">New Password</label>
                <input type="password" placeholder="••••••••" class="input-base w-full max-w-sm"/>
              </div>
              <div>
                <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Confirm New Password</label>
                <input type="password" placeholder="••••••••" class="input-base w-full max-w-sm"/>
              </div>
              <button class="btn-primary btn-sm">Update Password</button>
            </div>
            <div class="card space-y-4">
              <h2 class="heading-sm">Two-Factor Authentication</h2>
              <div class="flex items-center justify-between p-4 rounded-xl bg-primary-50 dark:bg-primary-950/30 border border-primary-100 dark:border-primary-900/30">
                <div>
                  <p class="text-sm font-semibold text-primary-800 dark:text-primary-200">2FA is Enabled</p>
                  <p class="text-xs text-primary-600 dark:text-primary-400 mt-0.5">Authenticator app connected (Google Authenticator)</p>
                </div>
                <span class="badge-success badge">Active</span>
              </div>
              <button class="btn-secondary btn-sm">Manage 2FA Devices</button>
            </div>
            <div class="card space-y-3">
              <h2 class="heading-sm">Active Sessions</h2>
              <div *ngFor="let s of sessions" class="flex items-center justify-between p-3 rounded-xl bg-surface-50 dark:bg-surface-800/60">
                <div class="flex items-center gap-3">
                  <div class="icon-box-sm icon-box-primary">
                    <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>
                    </svg>
                  </div>
                  <div>
                    <p class="text-xs font-semibold text-gray-900 dark:text-white">{{ s.device }}</p>
                    <p class="text-2xs text-gray-400">{{ s.location }} &middot; {{ s.time }}</p>
                  </div>
                </div>
                <div class="flex items-center gap-2">
                  <span *ngIf="s.current" class="badge-primary text-2xs badge">Current</span>
                  <button *ngIf="!s.current" class="text-xs font-medium text-red-500 hover:text-red-700 dark:text-red-400">Revoke</button>
                </div>
              </div>
            </div>
          </div>

          <!-- ── System ──────────────────────────────── -->
          <div *ngIf="activeSection === 'system'" class="card space-y-5">
            <h2 class="heading-sm">System Configuration</h2>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Default Language</label>
                <select class="input-base w-full"><option>English (US)</option><option>Spanish</option><option>French</option></select>
              </div>
              <div>
                <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Timezone</label>
                <select class="input-base w-full"><option>UTC-5 (Eastern)</option><option>UTC-6 (Central)</option><option>UTC-8 (Pacific)</option></select>
              </div>
              <div>
                <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Date Format</label>
                <select class="input-base w-full"><option>MM/DD/YYYY</option><option>DD/MM/YYYY</option><option>YYYY-MM-DD</option></select>
              </div>
              <div>
                <label class="block text-xs font-semibold text-gray-500 dark:text-gray-400 mb-1.5">Session Timeout</label>
                <select class="input-base w-full"><option>15 minutes</option><option>30 minutes</option><option>1 hour</option><option>4 hours</option></select>
              </div>
            </div>
            <div class="space-y-3">
              <label *ngFor="let t of toggles" class="flex items-center justify-between p-3 rounded-xl bg-surface-50 dark:bg-surface-800/60">
                <div>
                  <p class="text-sm font-medium text-gray-900 dark:text-white">{{ t.label }}</p>
                  <p class="text-2xs text-gray-400">{{ t.description }}</p>
                </div>
                <input type="checkbox" [(ngModel)]="t.enabled" class="w-4 h-4 rounded accent-primary-600"/>
              </label>
            </div>
            <button class="btn-primary btn-sm">Save Configuration</button>
          </div>

        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsPageComponent implements OnInit {
  activeSection = 'profile';
  saved = false;

  sections = [
    { key: 'profile',       label: 'Profile',       icon: 'M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z' },
    { key: 'notifications', label: 'Notifications',  icon: 'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9' },
    { key: 'security',      label: 'Security',       icon: 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z' },
    { key: 'system',        label: 'System',         icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z' },
  ];

  profile = { firstName: 'Sarah', lastName: 'Admin', email: 'admin@ehrplatform.com', phone: '(555) 000-0000', title: 'System Administrator' };

  notifications = [
    { label: 'Critical Lab Results',   description: 'Notify when a critical lab value is flagged', email: true,  sms: true },
    { label: 'Appointment Reminders',  description: 'Send reminders 24 hours before appointments',  email: true,  sms: false },
    { label: 'New Patient Registration',description: 'Alert when a new patient is registered',       email: false, sms: false },
    { label: 'Prescription Approved',  description: 'Confirm when pharmacy processes a prescription',email: true,  sms: true },
    { label: 'System Alerts',          description: 'Security events and system maintenance notices', email: true,  sms: true },
  ];

  sessions = [
    { device: 'Chrome on Windows 11', location: 'New York, USA', time: 'Active now',   current: true },
    { device: 'Safari on iPhone 15',  location: 'New York, USA', time: '2 hours ago',  current: false },
    { device: 'Firefox on macOS',     location: 'Brooklyn, USA', time: '3 days ago',   current: false },
  ];

  toggles = [
    { label: 'Dark Mode (System Default)',    description: 'Match system appearance preference',       enabled: false },
    { label: 'Audit Trail Logging',          description: 'Log all user actions for compliance',      enabled: true },
    { label: 'Auto-Lock Screen',            description: 'Lock after inactivity timeout',            enabled: true },
    { label: 'Drug Interaction Alerts',     description: 'Show real-time drug interaction warnings', enabled: true },
  ];

  ngOnInit(): void {}
}
