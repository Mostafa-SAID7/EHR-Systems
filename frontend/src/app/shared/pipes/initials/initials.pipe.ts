import { Pipe, PipeTransform } from '@angular/core';

/**
 * Pure pipe — extracts up to 2 initials from a full name string.
 * Being pure, Angular only re-runs it when the input reference changes,
 * eliminating the repeated string-split executed on every CD cycle.
 *
 * @example {{ userName | initials }}   // "Sarah Johnson" → "SJ"
 */
@Pipe({
  name: 'initials',
  standalone: true,
  pure: true,
})
export class InitialsPipe implements PipeTransform {
  transform(name: string | null | undefined): string {
    if (!name) return '';
    return name
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map(n => n[0].toUpperCase())
      .join('');
  }
}
