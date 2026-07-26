import {
  Component, Input, Output, EventEmitter,
  ChangeDetectionStrategy, HostListener
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate, group } from '@angular/animations';

/**
 * Modal Component — cinematic backdrop + scale-in dialog
 */
@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './modal.component.html',
  animations: [
    trigger('backdrop', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms ease-out', style({ opacity: 1 })),
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ opacity: 0 })),
      ]),
    ]),
    trigger('dialog', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.94) translateY(8px)' }),
        animate('280ms cubic-bezier(0.34, 1.56, 0.64, 1)',
          style({ opacity: 1, transform: 'scale(1) translateY(0)' })),
      ]),
      transition(':leave', [
        animate('180ms cubic-bezier(0.4, 0, 1, 1)',
          style({ opacity: 0, transform: 'scale(0.96) translateY(4px)' })),
      ]),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ModalComponent {
  @Input() open = false;
  @Input() title = 'Modal';
  @Input() confirmLabel = 'Confirm';
  @Input() cancelLabel = 'Cancel';
  @Input() closeOnBackdrop = true;
  @Input() maxWidth = '28rem';

  @Output() openChange = new EventEmitter<boolean>();
  @Output() confirm    = new EventEmitter<void>();
  @Output() close      = new EventEmitter<void>();

  onConfirm(): void { this.confirm.emit(); this.setOpen(false); }
  onClose():   void { this.close.emit();   this.setOpen(false); }

  onBackdropClick(): void {
    if (this.closeOnBackdrop) this.onClose();
  }

  private setOpen(v: boolean): void {
    this.open = v;
    this.openChange.emit(v);
  }

  @HostListener('keydown.escape')
  onEscape(): void { if (this.closeOnBackdrop) this.onClose(); }
}
