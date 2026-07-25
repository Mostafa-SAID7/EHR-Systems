import { Component, ChangeDetectionStrategy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Role {
  id: string;
  name: string;
  description: string;
  userCount: number;
  color: string;
  permissions: string[];
}

@Component({
  selector: 'app-role-management-page',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6 stagger">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="heading-xl">Role Management</h1>
          <p class="body-text mt-1">Define and manage access roles across the EHR platform</p>
        </div>
        <button class="btn-primary flex items-center gap-2">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"/></svg>
          Create Role
        </button>
      </div>

      <!-- Roles Grid -->
      <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
        @for (role of roles; track role.id) {
          <div class="card p-6 cursor-pointer transition-all hover:shadow-card-hover"
               (click)="selectedRole.set(selectedRole()?.id === role.id ? null : role)">
            <div class="flex items-start justify-between mb-3">
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-xl flex items-center justify-center text-white text-sm font-bold" [style.background]="role.color">
                  {{ role.name.charAt(0) }}
                </div>
                <div>
                  <h3 class="font-semibold text-gray-900 dark:text-white">{{ role.name }}</h3>
                  <p class="text-xs text-gray-500">{{ role.userCount }} users assigned</p>
                </div>
              </div>
              <div class="flex gap-2">
                <button class="btn-icon-sm" (click)="$event.stopPropagation()">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z"/></svg>
                </button>
                <button class="btn-icon-sm text-red-400 hover:text-red-600" (click)="$event.stopPropagation()">
                  <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/></svg>
                </button>
              </div>
            </div>
            <p class="text-sm text-gray-600 dark:text-gray-400 mb-4">{{ role.description }}</p>
            <div class="flex flex-wrap gap-1.5">
              @for (perm of role.permissions.slice(0,4); track perm) {
                <span class="px-2 py-0.5 text-xs rounded-md bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400">{{ perm }}</span>
              }
              @if (role.permissions.length > 4) {
                <span class="px-2 py-0.5 text-xs rounded-md bg-primary-50 dark:bg-primary-900/20 text-primary-600 dark:text-primary-400">+{{ role.permissions.length - 4 }} more</span>
              }
            </div>
            @if (selectedRole()?.id === role.id) {
              <div class="mt-4 pt-4 border-t border-gray-100 dark:border-gray-800">
                <p class="text-xs font-semibold text-gray-500 uppercase tracking-wide mb-2">All Permissions</p>
                <div class="grid grid-cols-2 gap-1">
                  @for (perm of role.permissions; track perm) {
                    <div class="flex items-center gap-1.5 text-xs text-gray-600 dark:text-gray-400">
                      <svg class="w-3 h-3 text-emerald-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7"/></svg>
                      {{ perm }}
                    </div>
                  }
                </div>
              </div>
            }
          </div>
        }
      </div>

      <!-- Permission Matrix -->
      <div class="card overflow-hidden">
        <div class="px-6 py-4 border-b border-gray-100 dark:border-gray-800">
          <h2 class="font-semibold text-gray-900 dark:text-white">Permission Matrix</h2>
          <p class="text-xs text-gray-500 mt-0.5">Overview of role access across platform modules</p>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="bg-gray-50 dark:bg-gray-800/50">
                <th class="px-6 py-3 text-left text-xs font-semibold text-gray-500 uppercase">Module</th>
                @for (role of roles; track role.id) {
                  <th class="px-4 py-3 text-center text-xs font-semibold" [style.color]="role.color">{{ role.name }}</th>
                }
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
              @for (mod of modules; track mod.name) {
                <tr class="hover:bg-gray-50 dark:hover:bg-gray-800/30">
                  <td class="px-6 py-3 font-medium text-gray-700 dark:text-gray-300">{{ mod.name }}</td>
                  @for (role of roles; track role.id) {
                    <td class="px-4 py-3 text-center">
                      @if (hasAccess(role, mod.key)) {
                        <svg class="w-4 h-4 text-emerald-500 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2.5" d="M5 13l4 4L19 7"/></svg>
                      } @else {
                        <svg class="w-4 h-4 text-gray-200 dark:text-gray-700 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/></svg>
                      }
                    </td>
                  }
                </tr>
              }
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
})
export class RoleManagementPageComponent {
  selectedRole = signal<Role | null>(null);

  roles: Role[] = [
    { id: 'r1', name: 'Administrator', description: 'Full system access with user management, configuration, and audit capabilities.', userCount: 4, color: '#7c3aed',
      permissions: ['Manage Users', 'Manage Roles', 'View Audit Logs', 'System Configuration', 'Data Export', 'Patient Records', 'Billing Management', 'Reports'] },
    { id: 'r2', name: 'Physician', description: 'Clinical access for patient care, prescriptions, lab orders, and medical records.', userCount: 28, color: '#2563eb',
      permissions: ['View Patient Records', 'Edit Clinical Notes', 'Issue Prescriptions', 'Order Labs', 'View Lab Results', 'Billing View', 'Appointment Scheduling'] },
    { id: 'r3', name: 'Registered Nurse', description: 'Nursing workflows including vitals, care plans, medication administration, and patient tracking.', userCount: 54, color: '#0d9488',
      permissions: ['View Patient Records', 'Update Vitals', 'Medication Administration', 'Care Plan View', 'Appointment View', 'Clinical Notes View'] },
    { id: 'r4', name: 'Billing Staff', description: 'Financial operations including invoicing, insurance claims, and payment processing.', userCount: 12, color: '#d97706',
      permissions: ['Billing Management', 'Invoice Creation', 'Payment Processing', 'Insurance Claims', 'Financial Reports', 'Patient Demographics View'] },
    { id: 'r5', name: 'Receptionist', description: 'Front-desk operations including appointment scheduling and patient check-in.', userCount: 18, color: '#db2777',
      permissions: ['Appointment Scheduling', 'Patient Check-in', 'Patient Demographics View', 'Basic Reports'] },
    { id: 'r6', name: 'Lab Technician', description: 'Laboratory specimen management, test processing, and results entry.', userCount: 9, color: '#ea580c',
      permissions: ['Lab Orders View', 'Lab Results Entry', 'Specimen Management', 'Patient ID View'] },
  ];

  modules = [
    { name: 'Patient Records', key: 'patient' },
    { name: 'Clinical Notes', key: 'clinical' },
    { name: 'Prescriptions', key: 'prescriptions' },
    { name: 'Lab Results', key: 'labs' },
    { name: 'Appointments', key: 'appointments' },
    { name: 'Billing', key: 'billing' },
    { name: 'User Management', key: 'users' },
    { name: 'Audit Logs', key: 'audit' },
  ];

  hasAccess(role: Role, moduleKey: string): boolean {
    const matrix: Record<string, string[]> = {
      r1: ['patient','clinical','prescriptions','labs','appointments','billing','users','audit'],
      r2: ['patient','clinical','prescriptions','labs','appointments','billing'],
      r3: ['patient','clinical','labs','appointments'],
      r4: ['patient','billing','appointments'],
      r5: ['patient','appointments'],
      r6: ['patient','labs'],
    };
    return (matrix[role.id] || []).includes(moduleKey);
  }
}
