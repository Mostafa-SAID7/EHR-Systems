import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthBrandPanelComponent, AuthHighlight } from '../../features/auth/components/auth-brand-panel/auth-brand-panel.component';
import { ToastContainerComponent } from '../../shared/components/ui/toast/toast.component';
import { CookieConsentComponent } from '../../shared/components/ui/cookie-consent/cookie-consent.component';

@Component({
  selector: 'app-auth-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, AuthBrandPanelComponent, ToastContainerComponent, CookieConsentComponent],
  templateUrl: './auth-layout.component.html',
})
export class AuthLayoutComponent {
  highlights: AuthHighlight[] = [
    { value: '50k+', label: 'Patients' },
    { value: '99.9%', label: 'Uptime' },
    { value: 'HIPAA', label: 'Compliant' },
  ];
  features = ['Patient Management', 'eRx', 'Lab Results', 'Billing', 'Analytics'];
}
