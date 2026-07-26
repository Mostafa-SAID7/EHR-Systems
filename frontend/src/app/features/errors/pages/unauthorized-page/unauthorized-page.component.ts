import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';

/**
 * 403 Access Denied / Unauthorized Page
 * Lives under features/errors/pages/unauthorized-page
 */
@Component({
  selector: 'app-unauthorized-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './unauthorized-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UnauthorizedPageComponent {
  private location    = inject(Location);
  private authService = inject(AuthService);

  readonly user = this.authService.user$;

  goBack(): void {
    this.location.back();
  }

  getUserRoleName(): string {
    const u = this.user();
    if (!u || !u.roles?.length) return 'Guest';
    return u.roles.map(r => r.name).join(', ');
  }
}
