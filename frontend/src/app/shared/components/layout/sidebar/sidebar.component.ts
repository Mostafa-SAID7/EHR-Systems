import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { filter } from 'rxjs/operators';

export interface NavItem {
  id: string;
  label: string;
  icon: string;
  route?: string;
  children?: NavItem[];
  badge?: number;
  expanded?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  host: {
    class: 'h-full flex flex-col shrink-0'
  },
  animations: [
    trigger('expandCollapse', [
      transition(':enter', [
        style({ opacity: 0, height: 0, overflow: 'hidden' }),
        animate('220ms cubic-bezier(0.16, 1, 0.3, 1)', style({ opacity: 1, height: '*' })),
      ]),
      transition(':leave', [
        style({ overflow: 'hidden' }),
        animate('160ms ease-in', style({ opacity: 0, height: 0 })),
      ]),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent implements OnInit {
  private router = inject(Router);

  @Input() navItems: NavItem[] = [];
  @Input() collapsed = false;
  @Input() isMobile = false;
  @Output() collapsedChange = new EventEmitter<boolean>();
  @Output() navigate = new EventEmitter<void>();

  ngOnInit(): void {
    this.autoExpandActiveParents();
    this.router.events
      .pipe(filter(e => e instanceof NavigationEnd))
      .subscribe(() => this.autoExpandActiveParents());
  }

  autoExpandActiveParents(): void {
    const currentUrl = this.router.url;
    for (const item of this.navItems) {
      if (item.children && item.children.some(c => c.route && currentUrl.startsWith(c.route))) {
        item.expanded = true;
      }
    }
  }

  isParentActive(item: NavItem): boolean {
    if (!item.children) return false;
    const currentUrl = this.router.url;
    return item.children.some(c => c.route && (c.route === '/admin' ? currentUrl === '/admin' : currentUrl.startsWith(c.route)));
  }

  trackById(_: number, item: NavItem): string { return item.id; }

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }

  toggleItem(item: NavItem): void {
    if (item.children) item.expanded = !item.expanded;
  }

  onParentLinkClick(item: NavItem): void {
    item.expanded = true;
    this.navigate.emit();
  }

  toggleCaret(event: MouseEvent, item: NavItem): void {
    event.preventDefault();
    event.stopPropagation();
    item.expanded = !item.expanded;
  }

  onItemClick(item: NavItem): void {
    this.navigate.emit();
  }

  onChildClick(): void {
    this.navigate.emit();
  }
}
