import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AppointmentNotesCardComponent } from '../../components/appointment-notes-card/appointment-notes-card.component';
import { AppointmentVitalsCardComponent } from '../../components/appointment-vitals-card/appointment-vitals-card.component';

@Component({
  selector: 'app-appointment-detail-page',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    AppointmentNotesCardComponent,
    AppointmentVitalsCardComponent,
  ],
  templateUrl: './appointment-detail-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentDetailPageComponent implements OnInit {
  appt = {
    id: '1042',
    patientId: '1',
    patient: 'Sarah Johnson',
    initials: 'SJ',
    mrn: '00-1234',
    gender: 'Female',
    age: 39,
    phone: '(555) 010-1234',
    color: 'linear-gradient(135deg,#15803d,#16a34a,#4ade80)',
    type: 'Annual Physical Exam',
    doctor: 'Dr. Patel',
    date: new Date(2026, 6, 23, 10, 30),
    duration: 30,
    room: '101',
    status: 'Completed',
    priority: 'Routine',
    notes: 'Patient presents for annual physical. BP slightly elevated at 138/88. HbA1c 7.2% — continues metformin. Dietary counseling given. Return in 3 months.',
  };

  details = [
    { label: 'Visit Type',  value: 'Annual Physical Exam' },
    { label: 'Provider',    value: 'Dr. Patel' },
    { label: 'Time',        value: '10:30 AM' },
    { label: 'Duration',    value: '30 minutes' },
    { label: 'Room',        value: 'Room 101' },
    { label: 'Priority',    value: 'Routine' },
  ];

  vitals = [
    { label: 'Blood Pressure', value: '138/88', unit: 'mmHg' },
    { label: 'Heart Rate',     value: '72',     unit: 'bpm' },
    { label: 'Temperature',    value: '98.6',   unit: '°F' },
    { label: 'Weight',         value: '154',    unit: 'lbs' },
    { label: 'Height',         value: '5\'6"',  unit: '' },
    { label: 'O₂ Sat.',       value: '98',     unit: '%' },
  ];

  previousVisits = [
    { type: 'Diabetes Management Review', date: 'Apr 15, 2026', status: 'Completed' },
    { type: 'Hypertension Follow-up',     date: 'Jan 10, 2026', status: 'Completed' },
    { type: 'Annual Physical',            date: 'Jul 20, 2025', status: 'Completed' },
  ];

  statusClass(s: string): string {
    return s === 'Scheduled' ? 'badge-info' : s === 'In Progress' ? 'badge-primary' : s === 'Completed' ? 'badge-success' : s === 'Cancelled' ? 'badge-danger' : 'badge-neutral';
  }

  ngOnInit(): void {}
}
