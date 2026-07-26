import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CookieService, CookieConsentPreferences } from '../../../../core/services/cookie.service';

@Component({
  selector: 'app-cookie-consent',
  standalone: true,
  imports: [CommonModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './cookie-consent.component.html',
})
export class CookieConsentComponent {
  readonly cookieService = inject(CookieService);
  readonly showDetails = signal(false);

  customPrefs = {
    analytics: true,
    preferences: true,
    marketing: false,
  };

  onAcceptAll(): void {
    this.cookieService.acceptAll();
  }

  onDecline(): void {
    this.cookieService.rejectNonEssential();
  }

  onSaveCustom(): void {
    this.cookieService.saveConsent({
      essential: true,
      analytics: this.customPrefs.analytics,
      preferences: this.customPrefs.preferences,
      marketing: this.customPrefs.marketing,
    });
  }
}
