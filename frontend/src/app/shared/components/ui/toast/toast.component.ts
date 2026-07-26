import { Component, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NotificationService, Notification, NotificationType } from '../../../../core/services/notification.service';

interface ActiveToast extends Notification {
  paused?: boolean;
  progress?: number;
}

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './toast.component.html',
})
export class ToastContainerComponent implements OnInit, OnDestroy {
  private notificationService = inject(NotificationService);
  private cdr                 = inject(ChangeDetectorRef);
  private destroy$            = new Subject<void>();

  toasts: ActiveToast[] = [];
  private timers: Map<string, any> = new Map();

  ngOnInit(): void {
    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe((notification) => {
        this.addToast(notification);
      });

    this.notificationService.close$
      .pipe(takeUntil(this.destroy$))
      .subscribe((id) => {
        this.dismiss(id);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.timers.forEach((t) => clearInterval(t));
  }

  trackById(_index: number, toast: ActiveToast): string {
    return toast.id;
  }

  private addToast(notification: Notification): void {
    const toast: ActiveToast = { ...notification, progress: 100, paused: false };
    this.toasts = [toast, ...this.toasts.slice(0, 5)]; // max 6 toasts visible
    this.startTimer(toast);
    this.cdr.markForCheck();
  }

  dismiss(id: string): void {
    if (this.timers.has(id)) {
      clearInterval(this.timers.get(id));
      this.timers.delete(id);
    }
    this.toasts = this.toasts.filter((t) => t.id !== id);
    this.cdr.markForCheck();
  }

  pauseToast(toast: ActiveToast): void {
    toast.paused = true;
  }

  resumeToast(toast: ActiveToast): void {
    toast.paused = false;
  }

  private startTimer(toast: ActiveToast): void {
    if (!toast.duration || toast.duration <= 0) return;

    const interval = 50; // update every 50ms
    const step = (interval / toast.duration) * 100;

    const timer = setInterval(() => {
      if (!toast.paused) {
        toast.progress = (toast.progress ?? 100) - step;
        if (toast.progress <= 0) {
          this.dismiss(toast.id);
        } else {
          this.cdr.markForCheck();
        }
      }
    }, interval);

    this.timers.set(toast.id, timer);
  }

  getToastBorderClass(type: NotificationType): string {
    switch (type) {
      case 'success': return 'border-emerald-500/30 dark:border-emerald-500/40';
      case 'error':   return 'border-red-500/30 dark:border-red-500/40';
      case 'warning': return 'border-amber-500/30 dark:border-amber-500/40';
      case 'info':    return 'border-sky-500/30 dark:border-sky-500/40';
    }
  }

  getIconBoxClass(type: NotificationType): string {
    switch (type) {
      case 'success': return 'bg-emerald-500 text-white dark:bg-emerald-600';
      case 'error':   return 'bg-red-500 text-white dark:bg-red-600';
      case 'warning': return 'bg-amber-500 text-white dark:bg-amber-600';
      case 'info':    return 'bg-sky-500 text-white dark:bg-sky-600';
    }
  }

  getProgressBarClass(type: NotificationType): string {
    switch (type) {
      case 'success': return 'bg-emerald-500';
      case 'error':   return 'bg-red-500';
      case 'warning': return 'bg-amber-500';
      case 'info':    return 'bg-sky-500';
    }
  }
}
