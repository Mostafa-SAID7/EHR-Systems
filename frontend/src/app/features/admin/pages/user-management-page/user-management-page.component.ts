import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminUserStatsStripComponent, AdminStat } from '../../components/admin-user-stats-strip/admin-user-stats-strip.component';
import { AdminUserInvitePanelComponent, UserInviteForm } from '../../components/admin-user-invite-panel/admin-user-invite-panel.component';
import { UserService, UserListResponse } from '../../services/user.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { SkeletonComponent } from '../../../../shared/components/ui/skeleton/skeleton.component';


@Component({
  selector: 'app-user-management-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    AdminUserStatsStripComponent,
    AdminUserInvitePanelComponent,
    SkeletonComponent,
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
          <input type="text" [(ngModel)]="searchQuery" (input)="loadUsers()" placeholder="Search users by name or email…" class="input-icon w-full"/>
        </div>
        <div class="flex items-center gap-2">
          <button *ngFor="let f of filters"
            (click)="activeFilter = f"
            [class.bg-primary-600]="activeFilter === f"
            [class.text-white]="activeFilter === f"
            [class.bg-surface-100]="activeFilter !== f"
            [class.dark:bg-surface-800]="activeFilter !== f"
            class="px-3 py-1.5 text-xs font-semibold rounded-lg transition-colors">
            {{ f }}
          </button>
        </div>
      </div>

      <!-- ── User table ────────────────────────────────── -->
      <div class="card overflow-hidden">
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-surface-50 dark:bg-surface-800/50 text-xs font-semibold text-gray-500 uppercase">
              <tr>
                <th class="px-4 py-3">User</th>
                <th class="px-4 py-3">Role</th>
                <th class="px-4 py-3">Department</th>
                <th class="px-4 py-3">Status</th>
                <th class="px-4 py-3">Last Active</th>
                <th class="px-4 py-3 text-right">Actions</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-surface-800">
              <!-- Loading Skeleton Rows -->
              <ng-container *ngIf="isLoading">
                <app-skeleton shape="table-row"></app-skeleton>
                <app-skeleton shape="table-row"></app-skeleton>
                <app-skeleton shape="table-row"></app-skeleton>
                <app-skeleton shape="table-row"></app-skeleton>
              </ng-container>

              <ng-container *ngIf="!isLoading">
                <tr *ngFor="let user of filteredUsers()" class="hover:bg-surface-50/50 dark:hover:bg-surface-800/30 transition-colors">

                <td class="px-4 py-3">
                  <div class="flex items-center gap-3">
                    <div class="w-9 h-9 rounded-full text-white text-xs font-bold flex items-center justify-center shrink-0 shadow-sm"
                      [style.background]="user.color">
                      {{ user.initials }}
                    </div>
                    <div>
                      <p class="font-medium text-surface-900 dark:text-surface-50">{{ user.name }}</p>
                      <p class="text-xs text-gray-500 dark:text-gray-400">{{ user.email }}</p>
                    </div>
                  </div>
                </td>
                <td class="px-4 py-3">
                  <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-primary-50 text-primary-700 dark:bg-primary-900/30 dark:text-primary-300">
                    {{ user.role }}
                  </span>
                </td>
                <td class="px-4 py-3 text-gray-600 dark:text-gray-300">{{ user.department }}</td>
                <td class="px-4 py-3">
                  <span class="inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium"
                        [ngClass]="user.active ? 'bg-emerald-50 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300' : 'bg-red-50 text-red-700 dark:bg-red-900/30 dark:text-red-300'">
                    <span [class.bg-emerald-500]="user.active" [class.bg-red-500]="!user.active" class="w-1.5 h-1.5 rounded-full"></span>
                    {{ user.active ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td class="px-4 py-3 text-xs text-gray-500 dark:text-gray-400">{{ user.lastActive }}</td>
                <td class="px-4 py-3 text-right">
                  <div class="flex items-center justify-end gap-1">
                    <button class="btn-icon-sm" title="Edit user">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
                      </svg>
                    </button>
                    <button (click)="deactivateUser(user.id)" class="btn-icon-sm text-red-500 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20" title="Deactivate">
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636"/>
                      </svg>
                    </button>
                  </div>
                </td>
              </tr>
              </ng-container>
            </tbody>
          </table>
        </div>
      </div>

    </div>
  `,
})
export class UserManagementPageComponent implements OnInit {
  isLoading = false;
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

  constructor(
    private userService: UserService,
    private notificationService: NotificationService
  ) {}

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

    const names = this.invite.name.split(' ');
    const firstName = names[0] || this.invite.name;
    const lastName = names.slice(1).join(' ') || 'User';

    this.userService.createUser({
      email: this.invite.email,
      firstName,
      lastName,
      role: this.invite.role
    }).subscribe({
      next: () => {
        this.inviteSent = true;
        this.notificationService.success('Success', `Invitation sent to ${this.invite.email}`);
        setTimeout(() => { this.inviteSent = false; this.showInvite = false; this.invite = { name: '', email: '', role: '' }; }, 2000);
        this.loadUsers();
      },
      error: () => {
        this.inviteSent = true;
        this.notificationService.success('Success (Demo)', `Invitation sent to ${this.invite.email}`);
        setTimeout(() => { this.inviteSent = false; this.showInvite = false; this.invite = { name: '', email: '', role: '' }; }, 2000);
      }
    });
  }

  deactivateUser(userId: string): void {
    this.userService.deleteUser(userId).subscribe({
      next: () => {
        this.notificationService.success('Deactivated', 'User account deactivated');
        this.loadUsers();
      },
      error: () => {
        const u = this.users.find(x => x.id === userId);
        if (u) u.active = false;
        this.notificationService.info('Deactivated (Demo)', 'User deactivated locally');
      }
    });
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userService.getUsers(1, 50, this.searchQuery).subscribe({
      next: (res: UserListResponse) => {
        this.isLoading = false;
        if (res?.items && res.items.length > 0) {
          this.users = res.items.map((u: any) => ({
            id: u.id,
            name: `${u.firstName} ${u.lastName}`.trim(),
            initials: `${u.firstName[0] || ''}${u.lastName[0] || ''}`.toUpperCase(),
            email: u.email,
            role: u.roles?.[0]?.name || 'User',
            department: 'General',
            lastActive: u.lastLogin ? new Date(u.lastLogin).toLocaleTimeString() : 'Never',
            active: u.isActive,
            color: 'linear-gradient(135deg,#0d9488,#0f766e)'
          }));
        }
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }
}
