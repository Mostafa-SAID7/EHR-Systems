import { Pipe, PipeTransform } from '@angular/core';
import { formatDistanceToNow } from 'date-fns';

export type DateFormatType = 'short' | 'medium' | 'long' | 'full' | 'iso' | 'relative' | 'time' | 'date';

/**
 * Date Format Pipe
 * Formats dates using date-fns library with multiple format options
 * Usage: {{ date | dateFormat:'medium' }} or {{ date | dateFormat:'relative' }}
 */
@Pipe({
  name: 'dateFormat',
  standalone: true,
})
export class DateFormatPipe implements PipeTransform {
  transform(value: Date | string | number | null | undefined, format: DateFormatType = 'medium'): string {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    if (isNaN(date.getTime())) {
      return '';
    }

    switch (format) {
      case 'short':
        return date.toLocaleDateString('en-US', {
          year: '2-digit',
          month: '2-digit',
          day: '2-digit',
        });

      case 'medium':
        return date.toLocaleDateString('en-US', {
          year: 'numeric',
          month: 'short',
          day: 'numeric',
        });

      case 'long':
        return date.toLocaleDateString('en-US', {
          year: 'numeric',
          month: 'long',
          day: 'numeric',
        });

      case 'full':
        return date.toLocaleDateString('en-US', {
          weekday: 'long',
          year: 'numeric',
          month: 'long',
          day: 'numeric',
        });

      case 'iso':
        return date.toISOString();

      case 'relative':
        return formatDistanceToNow(date, { addSuffix: true });

      case 'time':
        return date.toLocaleTimeString('en-US', {
          hour: '2-digit',
          minute: '2-digit',
        });

      case 'date':
        return date.toLocaleDateString('en-US');

      default:
        return date.toLocaleDateString('en-US');
    }
  }
}
