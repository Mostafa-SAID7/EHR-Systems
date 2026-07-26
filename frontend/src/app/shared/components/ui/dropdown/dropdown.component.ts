import {
  Component, Input, Output, EventEmitter,
  ChangeDetectionStrategy, HostListener
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';

export interface DropdownOption {
  id: string | number;
  label: string;
  icon?: string;
  divider?: boolean;
  danger?: boolean;
}

/**
 * Dropdown Component — polished menu, no generic border outlines
 */
@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dropdown.component.html',
  animations: [
    trigger('dropdownAnim', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.96) translateY(-6px)' }),
        animate('200ms cubic-bezier(0.16, 1, 0.3, 1)',
          style({ opacity: 1, transform: 'scale(1) translateY(0)' })),
      ]),
      transition(':leave', [
        animate('130ms ease-in',
          style({ opacity: 0, transform: 'scale(0.96) translateY(-4px)' })),
      ]),
    ]),
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DropdownComponent {
  @Input() options: DropdownOption[] = [];
  @Output() select = new EventEmitter<DropdownOption>();

  isOpen = false;

  toggleOpen(): void { this.isOpen = !this.isOpen; }

  selectOption(option: DropdownOption): void {
    this.select.emit(option);
    this.isOpen = false;
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(e: MouseEvent): void {
    if (!(e.target as HTMLElement).closest('app-dropdown')) this.isOpen = false;
  }
}
