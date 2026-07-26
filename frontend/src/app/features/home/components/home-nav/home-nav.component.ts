import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-nav',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home-nav.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeNavComponent {
  @Input() isLoggedIn = false;
  @Input() isDark = false;

  @Output() scrollTo = new EventEmitter<string>();
  @Output() toggleTheme = new EventEmitter<void>();
}
