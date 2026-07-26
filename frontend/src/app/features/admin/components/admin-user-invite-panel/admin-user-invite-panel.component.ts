import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

export interface UserInviteForm {
  name: string;
  email: string;
  role: string;
}

@Component({
  selector: 'app-admin-user-invite-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-user-invite-panel.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AdminUserInvitePanelComponent {
  @Input() show = false;
  @Input() invite: UserInviteForm = { name: '', email: '', role: '' };
  @Input() roles: string[] = [];
  @Input() inviteSent = false;

  @Output() send = new EventEmitter<void>();
  @Output() close = new EventEmitter<void>();
}
