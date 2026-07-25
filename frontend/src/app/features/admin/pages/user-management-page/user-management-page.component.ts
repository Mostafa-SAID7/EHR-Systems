import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminUserStatsStripComponent, AdminStat } from '../../components/admin-user-stats-strip/admin-user-stats-strip.component';
import { AdminUserInvitePanelComponent, UserInviteForm } from '../../components/admin-user-invite-panel/admin-user-invite-panel.component';

@Component({
  selector: 'app-user-management-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    AdminUserStatsStripComponent,
    AdminUserInvitePanelComponent,
  ],
  template: `
    <div class="space-y-6 stagger">

      <!-- ── Header ───────────────────────────────────── -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">User Management</h1>
          <p class="body-text mt-1">Manage platform users, roles, and access permissions</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <button class="btn-secondary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"/>
            </svg>
            Export
          </button>
          <button (click)="showInvite = !showInvite" class="btn-primary btn-sm">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z"/>
            </svg>
            Invite User
          </button>
        </div>
      </div>

      <!-- ── Stats Subcomponent ────────────────────────── -->
      <app-admin-user-stats-strip
        [stats]="stats"
      ></app-admin-user-stats-strip>

      <!-- ── Invite Panel Subcomponent ─────────────────── -->
      <app-admin-user-invite-panel
        [show]="showInvite"
        [invite]="invite"
        [roles]="roles"
        [inviteSent]="inviteSent"
        (send)="sendInvite()"
        (close)="showInvite = false"
      ></app-admin-user-invite-panel>

      <!-- ── Search + filter ───────────────────────────── -->
      <div class="flex flex-col sm:flex-row gap-3">
        <div class="relative flex-1">
          <div class="absolute inset-y-0 left-0 flex items-center pl-3.5 pointer-events-none">
            <svg class="w-4 h-4 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
            </svg>
          </div>
          <input type="text" [(ngModel)]="searchQuery" placeholder="Search users by name or email…" class="input-icon w-full"/>
        </div>
        <div class="flex items-center gap-2">
          <button *ngFor="let f of filters"
            (click)="activeFilter = f"
            [class]="activeFilter === f ? 'filter-pill-active' : 'filter-pill'">{{ f }}</button>
        </div>
      </div>

      <!-- ── Users table ───────────────────────────────── -->
      <div class="card p-0 overflow-hidden">
        <div class="overflow-x-auto">
          <table class="table-base">
            <thead>
              <tr>
                <th>User</th>
                <th>Role</th>
                <th>Department</th>
                <th>Last Active</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let u of filteredUsers()"
                class="hover:bg-primary-50/30 dark:hover:bg-primary-900/10 transition-colors">
                <td>
                  <div class="flex items-center gap-3">
                    <div class="avatar-custom-md" [style.background]="u.color">{{ u.initials }}</div>
                    <div>
                      <p class="font-semibold text-gray-900 dark:text-white">{{ u.name }}</p>
                      <p class="text-2xs text-gray-400">{{ u.email }}</p>
                    </div>
                  </div>
                </td>
                <td>
                  <span class="badge-neutral text-2xs">{{ u.role }}</span>
                </td>
                <td class="text-xs text-gray-600 dark:text-gray-400">{{ u.department }}</td>
                <td class="text-xs text-gray-500 dark:text-gray-400">{{ u.lastActive }}</td>
                <td>
                  <div class="flex items-center gap-1.5">
                    <span class="w-2 h-2 rounded-full" [ngClass]="u.active ? 'bg-primary-500' : 'bg-gray-300'"></span>
                    <span class="text-xs font-medium" [ngClass]="u.active ? 'text-primary-600 dark:text-primary-400' : 'text-gray-400'">
                      {{ u.active ? 'Active' : 'Inactive' }}
                    </span>
                  </div>
                </td>
                <td>
                  <div class="flex items-center gap-1">
                    <button class="btn-icon-sm" title="Edit user">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/>
                      </svg>
                    </button>
                    <button class="btn-icon-sm" title="Manage roles">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"/>
                      </svg>
                    </button>
                    <button class="btn-icon-sm text-red-500 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20" title="Deactivate">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/>
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserManagementPageComponent implements OnInit {
  searchQuery = '';
  activeFilter = 'All';
  showInvite = false;
  inviteSent = false;
  filters = ['All', 'Active', 'Inactive', 'Doctors', 'Nurses', 'Admin'];
  roles = ['Doctor', 'Nurse', 'Admin', 'Billing Specialist', 'Lab Technician', 'Receptionist', 'System Administrator'];

  stats: AdminStat[] = [
    { label: 'Total Users',    value: '48',  icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z', iconClass: 'icon-box-primary' },
    { label: 'Active Now',     value: '31',  icon: 'M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z', iconClass: 'icon-box-teal' },
    { label: 'Pending Invite', value: '3',   icon: 'M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z', iconClass: 'icon-box-amber' },
    { label: 'Inactive',       value: '14',  icon: 'M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636', iconClass: 'icon-box-red' },
  ];

  invite: UserInviteForm = { name: '', email: '', role: '' };

  users = [
    { id: '1', name: 'Dr. Ramesh Patel',   initials: 'RP', email: 'r.patel@ehrplatform.com',   role: 'Doctor',               department: 'Internal Medicine', lastActive: '2 mins ago', active: true,  color: 'linear-gradient(135deg,#16a34a,#15803d)' },
    { id: '2', name: 'Dr. James Smith',    initials: 'JS', email: 'j.smith@ehrplatform.com',   role: 'Doctor',               department: 'General Practice',  lastActive: '1 hr ago',   active: true,  color: 'linear-gradient(135deg,#0d9488,#0f766e)' },
    { id: '3', name: 'Dr. Maria Garcia',   initials: 'MG', email: 'm.garcia@ehrplatform.com',  role: 'Doctor',               department: 'Cardiology',        lastActive: '30 mins ago',active: true,  color: 'linear-gradient(135deg,#7c3aed,#6d28d9)' },
    { id: '4', name: 'Nurse Aisha Brown',  initials: 'AB', email: 'a.brown@ehrplatform.com',   role: 'Nurse',                department: 'ICU',               lastActive: '5 mins ago', active: true,  color: 'linear-gradient(135deg,#d97706,#b45309)' },
    { id: '5', name: 'Sarah Admin',        initials: 'SA', email: 'admin@ehrplatform.com',     role: 'System Administrator', department: 'IT & Operations',   lastActive: 'Just now',   active: true,  color: 'linear-gradient(135deg,#dc2626,#b91c1c)' },
    { id: '6', name: 'Marcus Billing',     initials: 'MB', email: 'm.billing@ehrplatform.com', role: 'Billing Specialist',   department: 'Finance',           lastActive: '2 days ago', active: false, color: 'linear-gradient(135deg,#16a34a,#4ade80)' },
  ];

  filteredUsers() {
    let list = this.users;
    if (this.searchQuery.trim()) {
      const q = this.searchQuery.toLowerCase();
      list = list.filter(u => u.name.toLowerCase().includes(q) || u.email.toLowerCase().includes(q));
    }
    if (this.activeFilter === 'Active')   list = list.filter(u => u.active);
    if (this.activeFilter === 'Inactive') list = list.filter(u => !u.active);
    if (this.activeFilter === 'Doctors')  list = list.filter(u => u.role === 'Doctor');
    if (this.activeFilter === 'Nurses')   list = list.filter(u => u.role === 'Nurse');
    if (this.activeFilter === 'Admin')    list = list.filter(u => u.role.includes('Admin'));
    return list;
  }

  sendInvite(): void {
    if (!this.invite.name || !this.invite.email || !this.invite.role) return;
    this.inviteSent = true;
    setTimeout(() => { this.inviteSent = false; this.showInvite = false; this.invite = { name: '', email: '', role: '' }; }, 2500);
  }

  ngOnInit(): void {}
}
