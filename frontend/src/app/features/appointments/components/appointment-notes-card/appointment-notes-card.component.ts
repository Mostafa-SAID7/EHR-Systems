import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-appointment-notes-card',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './appointment-notes-card.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentNotesCardComponent {
  @Input() notes = '';
  @Output() notesChange = new EventEmitter<string>();

  editingNotes = false;

  toggleEditing(): void {
    this.editingNotes = !this.editingNotes;
  }
}
