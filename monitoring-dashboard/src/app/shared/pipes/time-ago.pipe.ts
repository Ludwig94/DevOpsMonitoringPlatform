import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timeAgo',
  standalone: true
})
export class TimeAgoPipe implements PipeTransform {
  transform(value: Date | string | null | undefined): string {
    if (!value) return '—';

    const date = typeof value === 'string' ? new Date(value) : value;
    const seconds = Math.floor((Date.now() - date.getTime()) / 1000);

    if (seconds < 10) return 'just now';
    if (seconds < 60) return `${seconds} seconds ago`;

    const minutes = Math.floor(seconds / 60);
    if (minutes < 60) return minutes === 1 ? '1 minute ago' : `${minutes} minutes ago`;

    const hours = Math.floor(minutes / 60);
    if (hours < 24) return hours === 1 ? '1 hour ago' : `${hours} hours ago`;

    const days = Math.floor(hours / 24);
    return days === 1 ? '1 day ago' : `${days} days ago`;
  }
}
