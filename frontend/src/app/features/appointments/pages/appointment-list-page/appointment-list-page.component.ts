import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AppointmentScheduleTableComponent, AppointmentRow } from '../../components/appointment-schedule-table/appointment-schedule-table.component';

@Component({
  selector: 'app-appointment-list-page',
  standalone: true,
  imports: [CommonModule, RouterModule, AppointmentScheduleTableComponent],
  templateUrl: './appointment-list-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentListPageComponent implements OnInit {
  activeView = 'day';
  todayLabel = new Date().toLocaleDateString('en-US', { weekday: 'long', month: 'long', day: 'numeric' });
  todayCount = 8;

  views = [
    { key: 'day',   label: 'Day' },
    { key: 'week',  label: 'Week' },
    { key: 'month', label: 'Month' },
  ];

  statusSummary = [
    { label: 'Scheduled',   count: 5, dotClass: 'bg-blue-500' },
    { label: 'In Progress', count: 1, dotClass: 'bg-primary-500 animate-pulse-soft' },
    { label: 'Completed',   count: 2, dotClass: 'bg-green-500' },
    { label: 'No Show',     count: 0, dotClass: 'bg-gray-400' },
    { label: 'Cancelled',   count: 1, dotClass: 'bg-red-500' },
  ];

  appointments: AppointmentRow[] = [
    { id: '1', patient: 'Sarah Johnson',  initials: 'SJ', type: 'General Checkup',    doctor: 'Dr. Patel',  date: this.today(9,0),   duration: 30, status: 'Completed',  room: '101', color: '#16a34a' },
    { id: '2', patient: 'Michael Chen',   initials: 'MC', type: 'Follow-up Visit',    doctor: 'Dr. Smith',  date: this.today(10,30), duration: 20, status: 'In Progress', room: '102', color: '#2563eb' },
    { id: '3', patient: 'Emma Williams',  initials: 'EW', type: 'Lab Results Review', doctor: 'Dr. Patel',  date: this.today(11,0),  duration: 15, status: 'Scheduled',  room: '103', color: '#7c3aed' },
    { id: '4', patient: 'Robert Davis',   initials: 'RD', type: 'Cardiology Consult', doctor: 'Dr. Garcia', date: this.today(14,0),  duration: 45, status: 'Scheduled',  room: '201', color: '#dc2626' },
    { id: '5', patient: 'Linda Martinez', initials: 'LM', type: 'Annual Physical',    doctor: 'Dr. Patel',  date: this.today(15,30), duration: 60, status: 'Scheduled',  room: '101', color: '#0d9488' },
    { id: '6', patient: 'James Wilson',   initials: 'JW', type: 'Follow-up Visit',    doctor: 'Dr. Smith',  date: this.today(16,0),  duration: 20, status: 'Cancelled',  room: '104', color: '#d97706' },
  ];

  today(h: number, m: number): Date {
    const d = new Date(); d.setHours(h, m, 0); return d;
  }

  ngOnInit(): void {}
}
