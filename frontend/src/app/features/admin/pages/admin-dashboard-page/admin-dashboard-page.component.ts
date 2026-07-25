import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-admin-dashboard-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ──────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Administration</h1>
          <p class="body-text mt-1">System management and configuration</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <span class="badge-success flex items-center gap-1.5">
            <span class="w-1.5 h-1.5 rounded-full bg-primary-500 animate-pulse-soft"></span>
            System Online
          </span>
        </div>
      </div>

      <!-- ── System health ─────────────────────────── -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <div *ngFor="let s of systemStats; let i = index"
          class="stat-card animate-count-up"
          [style.animation-delay]="i * 60 + 'ms'">
          <div class="flex items-start justify-between gap-2">
            <div class="min-w-0">
              <p class="stat-label">{{ s.label }}</p>
              <p class="stat-value mt-1.5">{{ s.value }}</p>
            </div>
            <div [ngClass]="s.iconClass" class="icon-box-md shrink-0">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="s.icon"/>
              </svg>
            </div>
          </div>
          <div class="mt-2 text-xs" [ngClass]="s.positive ? 'stat-change positive' : 'stat-change negative'">
            <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5"
                [attr.d]="s.positive ? 'M5 15l7-7 7 7' : 'M19 9l-7 7-7-7'"/>
            </svg>
            {{ s.change }}
          </div>
        </div>
      </div>

      <!-- ── Quick links ───────────────────────────── -->
      <div class="grid-3">
        <a *ngFor="let link of quickLinks; let i = index"
          [routerLink]="link.route"
          class="card-hover flex items-start gap-4"
          [style.animation-delay]="i * 55 + 'ms'">
          <div [ngClass]="link.iconClass" class="icon-box-lg shrink-0">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="link.icon"/>
            </svg>
          </div>
          <div class="min-w-0">
            <div class="flex items-center gap-2">
              <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ link.label }}</p>
              <span *ngIf="link.badge" class="badge-danger text-2xs">{{ link.badge }}</span>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5 leading-relaxed">{{ link.description }}</p>
          </div>
          <svg class="w-4 h-4 text-gray-300 dark:text-gray-600 shrink-0 mt-0.5 group-hover:text-primary-500 transition-colors"
            fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"/>
          </svg>
        </a>
      </div>

      <!-- ── Recent admin activity ──────────────────── -->
      <div class="card">
        <div class="flex items-center justify-between mb-4">
          <h2 class="heading-sm">Recent Admin Activity</h2>
          <span class="badge-neutral text-2xs">Last 24 hours</span>
        </div>
        <div class="space-y-0">
          <div *ngFor="let log of auditLogs" class="list-item">
            <div [ngClass]="log.iconClass" class="icon-box-sm shrink-0 mt-0.5">
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75" [attr.d]="log.icon"/>
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-sm font-medium text-gray-900 dark:text-white">{{ log.action }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">{{ log.user }} &middot; {{ log.time }}</p>
            </div>
            <span [ngClass]="log.badgeClass" class="badge shrink-0">{{ log.type }}</span>
          </div>
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminDashboardPageComponent implements OnInit {
  systemStats = [
    { label: 'Active Users',    value: '42',    change: '+3 today',      positive: true,  icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z', iconClass: 'icon-box-primary' },
    { label: 'System Uptime',   value: '99.9%', change: '30d streak',    positive: true,  icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-teal' },
    { label: 'Pending Approvals',value: '7',   change: '+2 new',         positive: false, icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', iconClass: 'icon-box-amber' },
    { label: 'Storage Used',    value: '68%',  change: '+1.2GB today',   positive: false, icon: 'M4 7v10c0 2.21 3.582 4 8 4s8-1.79 8-4V7M4 7c0 2.21 3.582 4 8 4s8-1.79 8-4M4 7c0-2.21 3.582-4 8-4s8 1.79 8 4', iconClass: 'icon-box-blue' },
  ];

  quickLinks = [
    { route: '/admin/users',  label: 'User Management',   description: 'Manage accounts, roles, and access permissions', icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z', iconClass: 'icon-box-primary', badge: null },
    { route: '/admin/roles',  label: 'Role Management',   description: 'Configure roles and permission sets for staff',   icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z', iconClass: 'icon-box-teal',  badge: null },
    { route: '/admin/audit',  label: 'Audit Logs',        description: 'Review access logs and compliance activity',      icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01m-.01 4h.01', iconClass: 'icon-box-blue',  badge: null },
    { route: '/admin/settings',label: 'System Settings',  description: 'Configure integrations, SMTP, and system defaults',icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z', iconClass: 'icon-box-purple', badge: null },
    { route: '/admin/users',  label: 'Pending Approvals', description: 'Review and approve new user account requests',    icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4', iconClass: 'icon-box-amber', badge: '7' },
    { route: '/admin/settings',label: 'HIPAA Compliance', description: 'Monitor compliance posture and required actions',  icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z', iconClass: 'icon-box-teal',  badge: null },
  ];

  auditLogs = [
    { action: 'User Dr. Smith logged in from 192.168.1.42',    user: 'System',    time: '2 min ago',  type: 'Login',    badgeClass: 'badge-success', icon: 'M11 16l-4-4m0 0l4-4m-4 4h14m-5 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h7a3 3 0 013 3v1', iconClass: 'icon-box-primary icon-box-sm' },
    { action: 'Permission "prescribe" added to Nurse role',     user: 'Dr. Admin', time: '14 min ago', type: 'Roles',    badgeClass: 'badge-info',    icon: 'M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z', iconClass: 'icon-box-teal icon-box-sm' },
    { action: 'Failed login attempt for admin@clinic.com',      user: 'System',    time: '1 hr ago',   type: 'Security', badgeClass: 'badge-danger',  icon: 'M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z', iconClass: 'icon-box-red icon-box-sm' },
    { action: 'System backup completed successfully (4.2 GB)',   user: 'System',    time: '3 hr ago',   type: 'System',   badgeClass: 'badge-success', icon: 'M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-8l-4-4m0 0L8 8m4-4v12', iconClass: 'icon-box-blue icon-box-sm' },
    { action: 'New user account created: Nurse Patricia Lee',    user: 'Dr. Admin', time: '5 hr ago',   type: 'Users',    badgeClass: 'badge-primary', icon: 'M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z', iconClass: 'icon-box-primary icon-box-sm' },
  ];

  ngOnInit(): void {}
}
