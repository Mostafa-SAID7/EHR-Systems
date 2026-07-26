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
  host: {
    class: 'hidden lg:flex lg:w-[48%] xl:w-[52%] shrink-0 flex-col min-h-screen'
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthBrandPanelComponent {
  @Input() highlights: AuthHighlight[] = [];
  @Input() features: string[] = [];
  trackByValue(_: number, val: string | AuthHighlight): string {
    return typeof val === 'string' ? val : val.value + val.label;
  }
}
