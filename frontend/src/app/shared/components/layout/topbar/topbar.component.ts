import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { ThemeService } from '../../../../core/services/theme.service';
import { InitialsPipe } from '../../../pipes/initials/initials.pipe';

export interface TopbarAction {
  id: string;
  iconPath: string;
  label: string;
  badge?: number;
}

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, RouterModule, InitialsPipe],
  templateUrl: './topbar.component.html',
  animations: [
    trigger('menuAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95) translateY(-8px)' }),
        animate('200ms cubic-bezier(0.16, 1, 0.3, 1)',
          style({ opacity: 1, transform: 'scale(1) translateY(0)' })),
      ]),
      transition(':leave', [
        animate('130ms ease-in',
          style({ opacity: 0, transform: 'scale(0.95) translateY(-4px)' })),
      ]),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TopbarComponent {
  private themeService = inject(ThemeService);

  @Input() title = '';
  @Input() actions: TopbarAction[] = [];
  @Input() userName = '';
  @Input() userAvatar = '';

  @Output() actionClick   = new EventEmitter<TopbarAction>();
  @Output() toggleSidebar = new EventEmitter<void>();
  @Output() logout        = new EventEmitter<void>();

  userMenuOpen = false;

  get isDark(): boolean {
    return this.themeService.isDarkMode();
  }

  onToggleTheme(): void {
    this.themeService.toggleDarkMode();
  }

  trackById(_: number, action: TopbarAction): string { return action.id; }
}
