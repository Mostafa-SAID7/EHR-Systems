import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

export interface Breadcrumb {
  label: string;
  url: string;
}

/**
 * Breadcrumbs Component
 * Auto-generated breadcrumb navigation from route
 * Usage: <app-breadcrumbs />
 */
@Component({
  selector: 'app-breadcrumbs',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './breadcrumbs.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BreadcrumbsComponent implements OnInit {
  breadcrumbs: (Breadcrumb & { isLast?: boolean })[] = [];

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe(() => {
        this.breadcrumbs = this.generateBreadcrumbs(this.activatedRoute.root);
      });
  }

  private generateBreadcrumbs(
    route: ActivatedRoute,
    url: string = '',
    breadcrumbs: (Breadcrumb & { isLast?: boolean })[] = []
  ): (Breadcrumb & { isLast?: boolean })[] {
    const ROUTE_DATA_BREADCRUMB = 'breadcrumb';

    // Get the child routes
    const children: ActivatedRoute[] = route.children;

    // Return if there are no more children
    if (children.length === 0) {
      return breadcrumbs;
    }

    // Iterate over child routes
    for (const child of children) {
      // Verify primary route
      if (child.outlet !== 'primary') {
        continue;
      }

      // Get the route's title
      const routeTitle: string = child.snapshot.data[ROUTE_DATA_BREADCRUMB];

      if (routeTitle) {
        // Add route title to breadcrumbs
        const routeURL: string = child.snapshot.url.map((segment) => segment.path).join('/');

        // Only add breadcrumb if it's not empty
        if (routeURL !== '') {
          url += `/${routeURL}`;
          breadcrumbs.push({
            label: routeTitle,
            url: url,
          });
        }
      }

      // Recursive
      return this.generateBreadcrumbs(child, url, breadcrumbs);
    }

    // Mark last breadcrumb
    if (breadcrumbs.length > 0) {
      breadcrumbs[breadcrumbs.length - 1].isLast = true;
    }

    return breadcrumbs;
  }
}
