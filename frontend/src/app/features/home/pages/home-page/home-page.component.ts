import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { ThemeService } from '../../../../core/services/theme.service';

import { HomeNavComponent } from '../../components/home-nav/home-nav.component';
import { HomeHeroComponent } from '../../components/home-hero/home-hero.component';
import { HomeStatsComponent } from '../../components/home-stats/home-stats.component';
import { HomeFeaturesComponent } from '../../components/home-features/home-features.component';
import { HomeSolutionsComponent } from '../../components/home-solutions/home-solutions.component';
import { HomeWorkflowsComponent } from '../../components/home-workflows/home-workflows.component';
import { HomeComplianceComponent } from '../../components/home-compliance/home-compliance.component';
import { HomeFooterComponent } from '../../components/home-footer/home-footer.component';

@Component({
  selector: 'app-home-page',
  standalone: true,
  imports: [
    CommonModule,
    HomeNavComponent,
    HomeHeroComponent,
    HomeStatsComponent,
    HomeFeaturesComponent,
    HomeSolutionsComponent,
    HomeWorkflowsComponent,
    HomeComplianceComponent,
    HomeFooterComponent,
  ],
  templateUrl: './home-page.component.html',
})
export class HomePageComponent implements OnInit {
  isLoggedIn = false;

  constructor(
    private authService: AuthService,
    private themeService: ThemeService,
    private router: Router
  ) {}

  get isDark(): boolean {
    return this.themeService.isDarkMode();
  }

  toggleTheme(): void {
    this.themeService.toggleDarkMode();
  }

  ngOnInit(): void {
    this.isLoggedIn = this.authService.isAuthenticated();
  }

  scrollTo(elementId: string): void {
    const element = document.getElementById(elementId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth' });
    }
  }

  quickLogin(email: string): void {
    this.authService.login({ email, password: 'Test1234!@' }).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: () => {
        this.router.navigate(['/dashboard']);
      }
    });
  }
}
