import { Component, OnInit, inject, computed, ChangeDetectionStrategy, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SidebarComponent, NavItem } from '../../shared/components/layout/sidebar/sidebar.component';
import { TopbarComponent, TopbarAction } from '../../shared/components/layout/topbar/topbar.component';
import { AuthService } from '../../core/services/auth.service';
import { ToastContainerComponent } from '../../shared/components/ui/toast/toast.component';
import { CookieConsentComponent } from '../../shared/components/ui/cookie-consent/cookie-consent.component';

// SVG path constants — centralised so icons are consistent across sidebar + topbar
const ICONS = {
  dashboard:     'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6',
  patients:      'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z',
  appointments:  'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z',
  clinical:      'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01',
  notes:         'M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z',
  vitals:        'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z',
  labs:          'M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z',
  prescriptions: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
  billing:       'M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z',
  reports:       'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z',
  admin:         'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z',
  bell:          'M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9',
  search:        'M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z',
};

/**
 * All navigation items with role restrictions.
 * `roles: []` means visible to ALL authenticated users.
 * `roles: ['admin']` means only admins see this item.
 */
interface NavItemDef extends NavItem {
  roles?: string[];
  children?: NavItemDef[];
}

const ALL_NAV_ITEMS: NavItemDef[] = [
  // Dashboard — all roles
  {
    id: 'dashboard', label: 'Dashboard', icon: ICONS.dashboard, route: '/dashboard',
    roles: [],
  },

  // Patients — doctors, nurses, admins
  {
    id: 'patients', label: 'Patients', icon: ICONS.patients, route: '/patients',
    roles: ['doctor', 'nurse', 'admin'],
  },

  // Appointments — doctors, nurses, admins, receptionists
  {
    id: 'appointments', label: 'Appointments', icon: ICONS.appointments, route: '/appointments',
    roles: ['doctor', 'nurse', 'admin', 'receptionist'],
  },

  // Clinical — doctors, nurses, admins
  {
    id: 'clinical', label: 'Clinical', icon: ICONS.clinical,
    roles: ['doctor', 'nurse', 'admin'],
    children: [
      { id: 'notes',  label: 'Clinical Notes', icon: ICONS.notes,  route: '/clinical/notes',  roles: ['doctor', 'nurse', 'admin'] },
      { id: 'vitals', label: 'Vitals',          icon: ICONS.vitals, route: '/clinical/vitals', roles: ['doctor', 'nurse', 'admin'] },
      { id: 'labs',   label: 'Lab Results',     icon: ICONS.labs,   route: '/lab-results',     roles: ['doctor', 'nurse', 'labtechnician', 'lab-tech', 'admin'] },
    ],
  },

  // Prescriptions — doctors, pharmacists, admins
  {
    id: 'prescriptions', label: 'Prescriptions', icon: ICONS.prescriptions, route: '/prescriptions',
    roles: ['doctor', 'pharmacist', 'admin'],
  },

  // Billing — admins, billing officers
  {
    id: 'billing', label: 'Billing', icon: ICONS.billing, route: '/billing',
    roles: ['admin', 'billing-officer', 'billingofficer'],
  },

  // Reports — admins, doctors, managers
  {
    id: 'reports', label: 'Reports', icon: ICONS.reports, route: '/reports',
    roles: ['admin', 'doctor', 'manager'],
    children: [
      { id: 'reports-main', label: 'Analytics',        icon: ICONS.reports, route: '/reports',                    roles: ['admin', 'doctor', 'manager'] },
      { id: 'pop-health',   label: 'Population Health', icon: ICONS.vitals,  route: '/reports/population-health', roles: ['admin', 'doctor', 'manager'] },
      { id: 'compliance',   label: 'Compliance',        icon: ICONS.admin,   route: '/reports/compliance',        roles: ['admin', 'manager'] },
    ],
  },

  // Admin — admins only
  {
    id: 'admin', label: 'Admin', icon: ICONS.admin, route: '/admin',
    roles: ['admin'],
    children: [
      { id: 'admin-dash',  label: 'Overview',   icon: ICONS.dashboard, route: '/admin',            roles: ['admin'] },
      { id: 'admin-users', label: 'Users',       icon: ICONS.patients,  route: '/admin/users',      roles: ['admin'] },
      { id: 'admin-roles', label: 'Roles',       icon: ICONS.clinical,  route: '/admin/roles',      roles: ['admin'] },
      { id: 'admin-audit', label: 'Audit Logs',  icon: ICONS.notes,     route: '/admin/audit-logs', roles: ['admin'] },
      { id: 'admin-set',   label: 'Settings',    icon: ICONS.admin,     route: '/admin/settings',   roles: ['admin'] },
    ],
  },
];

/** Returns true if the user has at least one of the required roles (or item has no restriction). */
function hasAccess(userRoles: string[], requiredRoles: string[] | undefined): boolean {
  if (!requiredRoles || requiredRoles.length === 0) return true;
  const userRolesLower = userRoles.map(r => r.toLowerCase());
  return requiredRoles.some(r => userRolesLower.includes(r.toLowerCase()));
}

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, TopbarComponent, ToastContainerComponent, CookieConsentComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './main-layout.component.html',
})
export class MainLayoutComponent implements OnInit {
  private authService = inject(AuthService);
  private router      = inject(Router);
  private destroyRef  = inject(DestroyRef);

  sidebarCollapsed  = false;
  mobileSidebarOpen = false;
  pageTitle         = 'Dashboard';

  // ── Computed display info ─────────────────────────────────────────────
  readonly displayName = computed(() => {
    const user = this.authService.user$();
    if (!user) return 'User';
    const parts = [user.firstName, user.lastName].filter(Boolean);
    return parts.length ? parts.join(' ') : (user.email ?? 'User');
  });

  readonly userAvatar = computed(() => {
    const user = this.authService.user$() as any;
    return user?.avatar ?? user?.profileImage ?? '';
  });

  // ── Role-filtered navigation ──────────────────────────────────────────
  /**
   * Computed nav items filtered by the current user's roles.
   * Recalculates automatically whenever the auth signal changes.
   */
  readonly navItems = computed<NavItem[]>(() => {
    const user      = this.authService.user$();
    const userRoles = user?.roles?.map(r => r.name) ?? [];

    return ALL_NAV_ITEMS
      .filter(item => hasAccess(userRoles, item.roles))
      .map(item => ({
        ...item,
        children: item.children
          ?.filter(child => hasAccess(userRoles, (child as NavItemDef).roles))
          .map(({ roles: _r, ...rest }) => rest),
      }))
      .map(({ roles: _r, ...rest }) => rest);
  });

  topbarActions: TopbarAction[] = [
    { id: 'search',        iconPath: ICONS.search, label: 'Search patients' },
    { id: 'notifications', iconPath: ICONS.bell,   label: 'Notifications', badge: 3 },
  ];

  ngOnInit(): void {
    // Update page title from route data and close mobile sidebar on navigation
    this.router.events
      .pipe(
        filter(e => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.mobileSidebarOpen = false;
        let route = this.router.routerState.snapshot.root;
        while (route.firstChild) route = route.firstChild;
        this.pageTitle = route.data['title'] ?? 'EHR Platform';
      });
  }

  onToggleSidebar(): void {
    if (window.innerWidth < 768) {
      this.mobileSidebarOpen = !this.mobileSidebarOpen;
    } else {
      this.sidebarCollapsed = !this.sidebarCollapsed;
    }
  }

  onLogout(): void {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/auth/login']);
    });
  }
}
