import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SidebarComponent, NavItem } from '../../shared/components/layout/sidebar/sidebar.component';
import { TopbarComponent, TopbarAction } from '../../shared/components/layout/topbar/topbar.component';

/**
 * Main Layout — authenticated shell with responsive sidebar
 */
@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, SidebarComponent, TopbarComponent],
  template: `
    <div class="flex h-screen overflow-hidden bg-surface-50 dark:bg-surface-900">

      <!-- Sidebar (desktop) -->
      <app-sidebar
        class="hidden md:flex"
        [navItems]="navItems"
        [(collapsed)]="sidebarCollapsed"
      />

      <!-- Mobile sidebar overlay -->
      <div
        *ngIf="mobileSidebarOpen"
        class="fixed inset-0 z-40 md:hidden"
        (click)="mobileSidebarOpen = false"
      >
        <div class="absolute inset-0 bg-black/50 backdrop-blur-sm"></div>
        <div class="relative z-50 h-full" (click)="$event.stopPropagation()">
          <app-sidebar
            [navItems]="navItems"
            [collapsed]="false"
          />
        </div>
      </div>

      <!-- Main content area -->
      <div class="flex-1 flex flex-col min-w-0 overflow-hidden">

        <!-- Topbar -->
        <app-topbar
          [title]="pageTitle"
          [actions]="topbarActions"
          [userName]="'Dr. Admin'"
          (toggleSidebar)="onToggleSidebar()"
          (logout)="onLogout()"
        />

        <!-- Scrollable content -->
        <main class="flex-1 overflow-y-auto overflow-x-hidden">
          <div class="page-container animate-fade-in-up">
            <router-outlet></router-outlet>
          </div>
        </main>
      </div>
    </div>
  `,
})
export class MainLayoutComponent implements OnInit {
  sidebarCollapsed = false;
  mobileSidebarOpen = false;
  pageTitle = 'Dashboard';

  navItems: NavItem[] = [
    { id: 'dashboard',    label: 'Dashboard',    icon: '📊', route: '/dashboard' },
    { id: 'patients',     label: 'Patients',     icon: '👥', route: '/patients' },
    { id: 'appointments', label: 'Appointments', icon: '📅', route: '/appointments' },
    {
      id: 'clinical', label: 'Clinical', icon: '🩺',
      children: [
        { id: 'notes',   label: 'Clinical Notes', icon: '📝', route: '/clinical/notes' },
        { id: 'vitals',  label: 'Vitals',         icon: '💓', route: '/clinical/vitals' },
        { id: 'labs',    label: 'Lab Results',    icon: '🔬', route: '/clinical/labs' },
      ],
    },
    { id: 'prescriptions', label: 'Prescriptions', icon: '💊', route: '/prescriptions' },
    { id: 'billing',       label: 'Billing',       icon: '💳', route: '/billing' },
    { id: 'reports',       label: 'Reports',       icon: '📈', route: '/reports' },
    { id: 'admin',         label: 'Admin',         icon: '⚙️', route: '/admin' },
  ];

  topbarActions: TopbarAction[] = [
    { id: 'notifications', icon: '🔔', label: 'Notifications', badge: 3 },
    { id: 'search',        icon: '🔍', label: 'Search' },
  ];

  ngOnInit(): void {}

  onToggleSidebar(): void {
    if (window.innerWidth < 768) {
      this.mobileSidebarOpen = !this.mobileSidebarOpen;
    } else {
      this.sidebarCollapsed = !this.sidebarCollapsed;
    }
  }

  onLogout(): void {
    // AuthService.logout()
  }
}
