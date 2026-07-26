import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, MainLayoutComponent],
  templateUrl: './app.component.html',
})
export class AppComponent implements OnInit {
  title = 'Modern EHR Platform';
  isDarkMode = signal(false);

  constructor(private themeService: ThemeService) {}

  ngOnInit(): void {
    // Initialize theme
    this.themeService.initializeTheme();
    
    // Subscribe to theme changes
    this.themeService.isDarkMode$.subscribe(isDark => {
      this.isDarkMode.set(isDark);
    });
  }
}
