import { User, Role, Permission } from '../../core/models';

/**
 * Mock User Roles
 */
export const MOCK_ROLES: Role[] = [
  {
    id: 'role-doctor',
    name: 'Doctor',
    description: 'Physician with full access to patient records',
    permissions: [],
    isActive: true,
  },
  {
    id: 'role-nurse',
    name: 'Nurse',
    description: 'Nursing staff with limited write access',
    permissions: [],
    isActive: true,
  },
  {
    id: 'role-admin',
    name: 'Admin',
    description: 'System administrator with full access',
    permissions: [],
    isActive: true,
  },
  {
    id: 'role-pharmacist',
    name: 'Pharmacist',
    description: 'Pharmacy staff',
    permissions: [],
    isActive: true,
  },
  {
    id: 'role-nurse-2',
    name: 'Receptionist',
    description: 'Front-desk reception staff',
    permissions: [],
    isActive: true,
  },
  {
    id: 'role-lab',
    name: 'LabTechnician',
    description: 'Laboratory technician',
    permissions: [],
    isActive: true,
  },
];


/**
 * Mock Permissions
 */
export const MOCK_PERMISSIONS: Permission[] = [
  {
    id: 'perm-1',
    name: 'View Patients',
    resource: 'patients',
    action: 'read',
    description: 'Can view patient information',
  },
  {
    id: 'perm-2',
    name: 'Create Patient',
    resource: 'patients',
    action: 'create',
    description: 'Can create new patient records',
  },
  {
    id: 'perm-3',
    name: 'Edit Patient',
    resource: 'patients',
    action: 'update',
    description: 'Can edit patient information',
  },
  {
    id: 'perm-4',
    name: 'Delete Patient',
    resource: 'patients',
    action: 'delete',
    description: 'Can delete patient records',
  },
  {
    id: 'perm-5',
    name: 'View Medical Records',
    resource: 'medical-records',
    action: 'read',
    description: 'Can view medical records',
  },
  {
    id: 'perm-6',
    name: 'Create Medical Records',
    resource: 'medical-records',
    action: 'create',
    description: 'Can create medical records',
  },
  {
    id: 'perm-7',
    name: 'Manage Prescriptions',
    resource: 'prescriptions',
    action: 'create',
    description: 'Can create and manage prescriptions',
  },
  {
    id: 'perm-8',
    name: 'View Reports',
    resource: 'reports',
    action: 'read',
    description: 'Can view analytics reports',
  },
];

/**
 * Mock Users
 */
export const MOCK_USERS: User[] = [
  {
    id: 'user-1',
    email: 'doctor@ehr.com',
    firstName: 'John',
    lastName: 'Smith',
    phone: '555-0101',
    avatar: undefined,
    roles: [MOCK_ROLES[0]], // Doctor
    permissions: MOCK_PERMISSIONS.slice(0, 7),
    isActive: true,
    lastLogin: new Date(Date.now() - 3600000),
    createdAt: new Date('2024-01-15'),
    updatedAt: new Date('2024-07-20'),
  },
  {
    id: 'user-2',
    email: 'nurse@ehr.com',
    firstName: 'Sarah',
    lastName: 'Johnson',
    phone: '555-0102',
    avatar: undefined,
    roles: [MOCK_ROLES[1]], // Nurse
    permissions: MOCK_PERMISSIONS.slice(0, 5),
    isActive: true,
    lastLogin: new Date(Date.now() - 7200000),
    createdAt: new Date('2024-02-10'),
    updatedAt: new Date('2024-07-19'),
  },
  {
    id: 'user-3',
    email: 'admin@ehr.com',
    firstName: 'Michael',
    lastName: 'Brown',
    phone: '555-0103',
    avatar: undefined,
    roles: [MOCK_ROLES[2]], // Admin
    permissions: MOCK_PERMISSIONS,
    isActive: true,
    lastLogin: new Date(Date.now() - 1800000),
    createdAt: new Date('2024-01-01'),
    updatedAt: new Date('2024-07-20'),
  },
  {
    id: 'user-4',
    email: 'pharmacist@ehr.com',
    firstName: 'Emily',
    lastName: 'Davis',
    phone: '555-0104',
    avatar: undefined,
    roles: [MOCK_ROLES[3]], // Pharmacist
    permissions: MOCK_PERMISSIONS.slice(5, 8),
    isActive: true,
    lastLogin: new Date(Date.now() - 14400000),
    createdAt: new Date('2024-03-05'),
    updatedAt: new Date('2024-07-18'),
  },
];
