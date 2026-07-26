import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-auth-header-logo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './auth-header-logo.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthHeaderLogoComponent {
  @Input() title = 'EHR Platform';
  @Input() subtitle = 'Enterprise Healthcare OS';
}
