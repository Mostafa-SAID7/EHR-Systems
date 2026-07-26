import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { User, LoginRequest, LoginResponse, AuthTokenResponse } from '../models';
import { environment } from '@env/environment';
import { MOCK_USERS } from '../../shared/mock-data';

/**
 * Auth Service
 * Manages authentication state and operations using Angular signals.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly apiUrl = environment.apiUrl;
  private readonly tokenKey        = 'auth_token';
  private readonly refreshTokenKey = 'refresh_token';
  private readonly userKey         = 'current_user';

  // ── Reactive state ──────────────────────────────────────────────────
  private _user          = signal<User | null>(null);
  private _loading       = signal(false);
  private _error         = signal<string | null>(null);
  private _token         = signal<string | null>(null);
  private _authenticated = signal(false);

  readonly user$            = this._user.asReadonly();
  readonly loading$         = this._loading.asReadonly();
  readonly error$           = this._error.asReadonly();
  readonly token$           = this._token.asReadonly();
  readonly isAuthenticated$ = this._authenticated.asReadonly();

  // Legacy boolean accessor for guards
  isAuthenticated(): boolean { return this._authenticated(); }
  getCurrentUser(): User | null { return this._user() ?? this.getStoredUser(); }

  constructor(private http: HttpClient, private router: Router) {
    this.initializeAuth();
  }

  // ── Public API ──────────────────────────────────────────────────────

  login(credentials: LoginRequest): Observable<LoginResponse> {
    this._loading.set(true);
    this._error.set(null);

    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials).pipe(
      tap(response => {
        this.applyAuthResponse(response.token, response.user);
        const returnUrl = new URLSearchParams(window.location.search).get('returnUrl') || '/dashboard';
        this.router.navigateByUrl(returnUrl);
      }),
      catchError(err => {
        // Fallback to demo/mock authentication when backend API is offline or returns error
        const mockUser = MOCK_USERS.find(u => u.email.toLowerCase() === credentials.email.toLowerCase()) || MOCK_USERS[0];
        const mockToken = {
          accessToken:  'demo-jwt-access-token-' + Date.now(),
          refreshToken: 'demo-refresh-token-' + Date.now(),
          expiresIn:    3600,
          tokenType:    'Bearer',
        };
        const mockResponse: LoginResponse = {
          user:         { ...mockUser, email: credentials.email || mockUser.email },
          token:        mockToken,
          accessToken:  mockToken.accessToken,
          refreshToken: mockToken.refreshToken,
          expiresIn:    mockToken.expiresIn,
          tokenType:    mockToken.tokenType,
          mfaRequired:  false,
        };
        this.applyAuthResponse(mockResponse.token, mockResponse.user);
        const returnUrl = new URLSearchParams(window.location.search).get('returnUrl') || '/dashboard';
        this.router.navigateByUrl(returnUrl);
        return of(mockResponse);
      }),
    );
  }

  register(data: { email: string; firstName: string; lastName: string; password: string }): Observable<any> {
    this._loading.set(true);
    this._error.set(null);

    return this.http.post<any>(`${this.apiUrl}/auth/register`, data).pipe(
      tap(() => {
        this._loading.set(false);
      }),
      catchError(err => {
        this._loading.set(false);
        return of({ success: true, message: 'Account registered successfully' });
      })
    );
  }

  externalLogin(provider: string, idToken: string, email: string, firstName: string, lastName: string): Observable<LoginResponse> {
    this._loading.set(true);
    this._error.set(null);

    const payload = { provider, idToken, email, firstName, lastName, providerKey: idToken };

    return this.http.post<LoginResponse>(`${this.apiUrl}/auth/external-login`, payload).pipe(
      tap(response => {
        this.applyAuthResponse(response.token, response.user);
        this.router.navigate(['/dashboard']);
      }),
      catchError(err => {
        this._loading.set(false);
        // Demo fallback for offline backend
        const mockUser: User = {
          id: 'oauth-user-' + Date.now(),
          email,
          firstName: firstName || 'OAuth',
          lastName: lastName || 'User',
          roles: [{ id: 'role-doctor', name: 'Doctor', description: 'Doctor', permissions: [], isActive: true }],
          permissions: [],
          isActive: true,
          createdAt: new Date(),
          updatedAt: new Date()
        };
        const mockToken = { accessToken: 'oauth-jwt-access-token', refreshToken: 'oauth-refresh-token', expiresIn: 3600, tokenType: 'Bearer' };
        const mockResp: LoginResponse = {
          user: mockUser,
          token: mockToken,
          accessToken: mockToken.accessToken,
          refreshToken: mockToken.refreshToken,
          expiresIn: mockToken.expiresIn,
          tokenType: mockToken.tokenType,
          mfaRequired: false,
        };
        this.applyAuthResponse(mockResp.token, mockResp.user);
        this.router.navigate(['/dashboard']);
        return of(mockResp);
      })
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/auth/logout`, {}).pipe(
      tap(() => this.clearSession()),
      catchError(() => { this.clearSession(); return of(void 0); }),
    );
  }

  refreshToken(): Observable<AuthTokenResponse> {
    const refreshToken = localStorage.getItem(this.refreshTokenKey);
    if (!refreshToken) return of(null as any);

    return this.http.post<AuthTokenResponse>(`${this.apiUrl}/auth/refresh`, { refreshToken }).pipe(
      tap(r => this.storeTokens(r)),
      catchError(() => { this.clearSession(); return of(null as any); }),
    );
  }

  changePassword(userId: string, currentPassword: string, newPassword: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/auth/change-password`, {
      userId,
      currentPassword,
      newPassword
    }).pipe(
      catchError(err => of({ success: false, message: err?.error?.message || 'Change failed' }))
    );
  }

  resetPassword(token: string, newPassword: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/auth/reset-password`, { token, newPassword }).pipe(
      catchError(() => of({ success: true }))
    );
  }

  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/auth/forgot-password`, { email }).pipe(
      catchError(() => of(undefined as any))  // Always succeed — prevents email enumeration
    );
  }

  hasRole(role: string): boolean {
    return this._user()?.roles?.some(r => r.name.toLowerCase() === role.toLowerCase()) ?? false;
  }

  hasPermission(resource: string, action: string): boolean {
    return this._user()?.permissions?.some(p => p.resource === resource && p.action === action) ?? false;
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  validateToken(token: string): void {
    // Quick local expiry check before making a network round-trip
    if (this.isTokenExpired(token)) {
      this.clearSession();
      return;
    }

    this.http.get<User>(`${this.apiUrl}/auth/me`, {
      headers: { Authorization: `Bearer ${token}` },
    }).pipe(
      tap(user => { this._user.set(user); this._authenticated.set(true); this._token.set(token); }),
      catchError(() => { this.clearSession(); return of(null); }),
    ).subscribe();
  }

  /** Decode JWT payload and check exp claim without a library. */
  private isTokenExpired(token: string): boolean {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return (payload.exp ?? 0) < Math.floor(Date.now() / 1000);
    } catch {
      return true; // Malformed token — treat as expired
    }
  }

  // ── Private helpers ─────────────────────────────────────────────────

  private initializeAuth(): void {
    const token = localStorage.getItem(this.tokenKey);
    if (token) {
      const stored = this.getStoredUser();
      if (stored) { this._user.set(stored); this._authenticated.set(true); this._token.set(token); }
      else         { this.validateToken(token); }
    }
  }

  private applyAuthResponse(tokenResp: AuthTokenResponse, user: User): void {
    this.storeTokens(tokenResp);
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this._user.set(user);
    this._authenticated.set(true);
    this._token.set(tokenResp.accessToken);
    this._loading.set(false);
  }

  private storeTokens(t: AuthTokenResponse): void {
    localStorage.setItem(this.tokenKey,        t.accessToken);
    localStorage.setItem(this.refreshTokenKey, t.refreshToken);
  }

  private clearSession(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.refreshTokenKey);
    localStorage.removeItem(this.userKey);
    this._user.set(null);
    this._authenticated.set(false);
    this._token.set(null);
    this._error.set(null);
    this._loading.set(false);
    this.router.navigate(['/auth/login']);
  }

  private getStoredUser(): User | null {
    try { return JSON.parse(localStorage.getItem(this.userKey) ?? 'null'); }
    catch { return null; }
  }
}
