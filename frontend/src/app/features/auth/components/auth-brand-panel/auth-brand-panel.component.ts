import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface AuthHighlight {
  value: string;
  label: string;
}

@Component({
  selector: 'app-auth-brand-panel',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './auth-brand-panel.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthBrandPanelComponent {
  @Input() highlights: AuthHighlight[] = [];
  @Input() features: string[] = [];
  trackByValue(_: number, val: string | AuthHighlight): string {
    return typeof val === 'string' ? val : val.value + val.label;
  }
}
