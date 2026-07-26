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
  templateUrl: './role-management-page.component.html',
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
