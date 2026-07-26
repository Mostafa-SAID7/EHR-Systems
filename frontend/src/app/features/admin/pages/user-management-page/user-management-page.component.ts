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
  templateUrl: './user-management-page.component.html',
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
