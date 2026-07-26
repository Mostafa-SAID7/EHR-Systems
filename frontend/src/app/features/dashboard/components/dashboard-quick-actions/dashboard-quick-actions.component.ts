import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface QuickAction {
  label: string;
  route: string;
  iconPath: string;
  iconBoxClass: string;
}

@Component({
  selector: 'app-dashboard-quick-actions',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-quick-actions.component.html',
  host: {
    class: 'w-full block'
  },
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardQuickActionsComponent {
  @Input() actions: QuickAction[] = [];
  trackByRoute(_: number, a: QuickAction): string { return a.route; }
}
