import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-not-found-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './not-found-page.component.html',
})
export class NotFoundPageComponent {
  private location = inject(Location);
  private router   = inject(Router);

  searchQuery = '';

  goBack(): void {
    this.location.back();
  }

  onSearch(): void {
    const q = this.searchQuery.toLowerCase().trim();
    if (!q) return;

    if (q.includes('patient')) this.router.navigate(['/patients']);
    else if (q.includes('appoint')) this.router.navigate(['/appointments']);
    else if (q.includes('rx') || q.includes('prescrip')) this.router.navigate(['/prescriptions']);
    else if (q.includes('bill')) this.router.navigate(['/billing']);
    else if (q.includes('report') || q.includes('analytic')) this.router.navigate(['/reports']);
    else if (q.includes('user') || q.includes('admin')) this.router.navigate(['/admin/users']);
    else this.router.navigate(['/dashboard']);
  }
}
