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
  template: `
    <div
      aria-live="polite"
      aria-atomic="true"
      class="fixed top-5 right-5 z-50 flex flex-col gap-3 max-w-sm w-full pointer-events-none"
    >
      <div
        *ngFor="let toast of toasts; trackBy: trackById"
        (mouseenter)="pauseToast(toast)"
        (mouseleave)="resumeToast(toast)"
        [ngClass]="getToastBorderClass(toast.type)"
        class="pointer-events-auto relative overflow-hidden rounded-2xl bg-white/95 dark:bg-surface-800/95 backdrop-blur-md shadow-2xl border p-4 transition-all duration-300 transform animate-slide-in-right hover:scale-[1.02]"
      >
        <div class="flex items-start gap-3">
          <!-- Icon -->
          <div [ngClass]="getIconBoxClass(toast.type)" class="w-9 h-9 rounded-xl flex items-center justify-center shrink-0 shadow-sm">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path *ngIf="toast.type === 'success'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"/>
              <path *ngIf="toast.type === 'error'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
              <path *ngIf="toast.type === 'warning'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/>
              <path *ngIf="toast.type === 'info'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
          </div>

          <!-- Content -->
          <div class="flex-1 min-w-0 pr-2">
            <h4 class="text-xs font-bold text-gray-900 dark:text-white uppercase tracking-wider">
              {{ toast.title }}
            </h4>
            <p class="text-xs text-gray-600 dark:text-gray-300 mt-0.5 leading-relaxed break-words">
              {{ toast.message }}
            </p>
          </div>

          <!-- Close button -->
          <button
            (click)="dismiss(toast.id)"
            class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors p-1 rounded-lg hover:bg-gray-100 dark:hover:bg-surface-700 shrink-0"
            title="Dismiss notification"
          >
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <!-- Progress bar timer -->
        <div
          *ngIf="toast.duration && toast.duration > 0"
          class="absolute bottom-0 left-0 right-0 h-1 bg-surface-100 dark:bg-surface-700/50"
        >
          <div
            [ngClass]="getProgressBarClass(toast.type)"
            [style.width.%]="toast.progress ?? 100"
            class="h-full transition-all duration-100 ease-linear"
          ></div>
        </div>
      </div>
    </div>
  `,
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
