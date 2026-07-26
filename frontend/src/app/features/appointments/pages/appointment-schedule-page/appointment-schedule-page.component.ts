import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-appointment-schedule-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './appointment-schedule-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentSchedulePageComponent implements OnInit {
  submitted = false;

  form = {
    patientId: '',
    mrn: '',
    type: '',
    provider: '',
    date: '',
    time: '',
    duration: '30',
    room: '',
    priority: 'routine',
    notes: '',
    sendReminder: true,
    sendEmail: true,
  };

  patients = [
    { id: '1', name: 'Sarah Johnson',  mrn: '00-1234' },
    { id: '2', name: 'Michael Chen',   mrn: '00-2345' },
    { id: '3', name: 'Emma Williams',  mrn: '00-3456' },
    { id: '4', name: 'Robert Davis',   mrn: '00-4567' },
    { id: '5', name: 'Linda Martinez', mrn: '00-5678' },
  ];

  visitTypes = ['General Checkup', 'Follow-up Visit', 'Lab Results Review', 'Cardiology Consult', 'Annual Physical', 'Urgent Care', 'Telehealth Visit', 'Vaccination', 'Mental Health'];
  doctors  = ['Dr. Patel', 'Dr. Smith', 'Dr. Garcia', 'Dr. Johnson', 'Dr. Lee'];
  rooms    = ['Room 101', 'Room 102', 'Room 103', 'Room 104', 'Room 201', 'Room 202', 'Telehealth'];
  timeSlots = ['08:00 AM', '08:30 AM', '09:00 AM', '09:30 AM', '10:00 AM', '10:30 AM', '11:00 AM', '11:30 AM', '01:00 PM', '01:30 PM', '02:00 PM', '02:30 PM', '03:00 PM', '03:30 PM', '04:00 PM', '04:30 PM'];

  priorities = [
    { key: 'routine',   label: '🟢 Routine',   active: 'border-primary-500 bg-primary-50 text-primary-700 dark:bg-primary-900/40 dark:text-primary-300',   inactive: 'border-surface-200 dark:border-surface-600 text-gray-600 dark:text-gray-400 hover:border-primary-300' },
    { key: 'urgent',    label: '🟡 Urgent',    active: 'border-amber-500 bg-amber-50 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300',               inactive: 'border-surface-200 dark:border-surface-600 text-gray-600 dark:text-gray-400 hover:border-amber-300' },
    { key: 'emergency', label: '🔴 Emergency', active: 'border-red-500 bg-red-50 text-red-700 dark:bg-red-900/40 dark:text-red-300',                          inactive: 'border-surface-200 dark:border-surface-600 text-gray-600 dark:text-gray-400 hover:border-red-300' },
  ];

  isValid(): boolean {
    return !!(this.form.patientId && this.form.type && this.form.provider && this.form.date && this.form.time);
  }

  getPatientName(): string {
    return this.patients.find(p => p.id === this.form.patientId)?.name || '';
  }

  submit(): void {
    if (!this.isValid()) return;
    this.submitted = true;
    setTimeout(() => this.submitted = false, 3500);
  }

  ngOnInit(): void {}
}
