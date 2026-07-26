import { Injectable, signal } from '@angular/core';

export interface CookieOptions {
  expires?: number | Date;
  path?: string;
  domain?: string;
  secure?: boolean;
  sameSite?: 'Lax' | 'Strict' | 'None';
}

export interface CookieConsentPreferences {
  essential: boolean;
  analytics: boolean;
  preferences: boolean;
  marketing: boolean;
  consentedAt?: string;
}

/**
 * Cookie Service
 * Enterprise-grade cookie management with reactive Signal integration,
 * GDPR compliance consent tracking, and secure options.
 */
@Injectable({
  providedIn: 'root',
})
export class CookieService {
  private readonly CONSENT_KEY = 'ehr_cookie_consent';

  // Signals for reactive cookie consent state
  private readonly _consent = signal<CookieConsentPreferences | null>(this.getStoredConsent());
  private readonly _hasConsented = signal<boolean>(!!this.getStoredConsent());

  readonly consent$ = this._consent.asReadonly();
  readonly hasConsented$ = this._hasConsented.asReadonly();

  constructor() {}

  /**
   * Check if a cookie exists
   */
  check(name: string): boolean {
    name = encodeURIComponent(name);
    const reg = new RegExp('(?:^|; )' + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + '=([^;]*)');
    return reg.test(document.cookie);
  }

  /**
   * Get cookie value by name
   */
  get(name: string): string | null {
    if (this.check(name)) {
      name = encodeURIComponent(name);
      const reg = new RegExp('(?:^|; )' + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + '=([^;]*)');
      const matches = document.cookie.match(reg);
      return matches ? decodeURIComponent(matches[1]) : null;
    }
    return null;
  }

  /**
   * Get all cookies as key-value pairs
   */
  getAll(): Record<string, string> {
    const cookies: Record<string, string> = {};
    if (document.cookie && document.cookie !== '') {
      const split = document.cookie.split(';');
      for (const item of split) {
        const [k, v] = item.split('=');
        if (k) {
          cookies[decodeURIComponent(k.trim())] = decodeURIComponent(v ? v.trim() : '');
        }
      }
    }
    return cookies;
  }

  /**
   * Set cookie with options (expires, path, domain, secure, sameSite)
   */
  set(name: string, value: string, options: CookieOptions = {}): void {
    let cookieString = `${encodeURIComponent(name)}=${encodeURIComponent(value)}`;

    const opts: CookieOptions = {
      path: '/',
      sameSite: 'Lax',
      ...options,
    };

    if (opts.expires) {
      if (typeof opts.expires === 'number') {
        const date = new Date();
        date.setTime(date.getTime() + opts.expires * 24 * 60 * 60 * 1000);
        opts.expires = date;
      }
      cookieString += `; expires=${opts.expires.toUTCString()}`;
    }

    if (opts.path) cookieString += `; path=${opts.path}`;
    if (opts.domain) cookieString += `; domain=${opts.domain}`;
    if (opts.secure) cookieString += `; secure`;
    if (opts.sameSite) cookieString += `; samesite=${opts.sameSite}`;

    document.cookie = cookieString;
  }

  /**
   * Delete cookie
   */
  delete(name: string, path = '/', domain?: string): void {
    this.set(name, '', {
      expires: -1,
      path,
      domain,
    });
  }

  /**
   * Clear all non-essential cookies
   */
  clearAll(path = '/'): void {
    const cookies = this.getAll();
    for (const name of Object.keys(cookies)) {
      if (name !== this.CONSENT_KEY) {
        this.delete(name, path);
      }
    }
  }

  // ── GDPR Consent Management ──────────────────────────────────────────

  /**
   * Save user cookie consent preferences
   */
  saveConsent(preferences: Omit<CookieConsentPreferences, 'consentedAt'>): void {
    const consentData: CookieConsentPreferences = {
      ...preferences,
      essential: true, // Always true
      consentedAt: new Date().toISOString(),
    };

    this.set(this.CONSENT_KEY, JSON.stringify(consentData), {
      expires: 365, // 1 year
      path: '/',
      sameSite: 'Lax',
    });

    this._consent.set(consentData);
    this._hasConsented.set(true);
  }

  /**
   * Accept all cookies
   */
  acceptAll(): void {
    this.saveConsent({
      essential: true,
      analytics: true,
      preferences: true,
      marketing: true,
    });
  }

  /**
   * Reject non-essential cookies
   */
  rejectNonEssential(): void {
    this.saveConsent({
      essential: true,
      analytics: false,
      preferences: false,
      marketing: false,
    });
  }

  /**
   * Retrieve stored consent from cookie
   */
  private getStoredConsent(): CookieConsentPreferences | null {
    const raw = this.get(this.CONSENT_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw);
    } catch {
      return null;
    }
  }
}
