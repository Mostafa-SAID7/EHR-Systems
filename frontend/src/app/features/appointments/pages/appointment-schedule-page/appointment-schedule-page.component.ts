import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import * as AppointmentActions from '../../store/appointment.actions';
import { selectLoading, selectError, selectScheduleInProgress } from '../../store/appointment.selectors';
import { AppointmentType, ScheduleAppointmentRequest } from '../../models/appointment.model';

@Component({
  selector: 'app-appointment-schedule-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  templateUrl: './appointment-schedule-page.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppointmentSchedulePageComponent implements OnInit {
  form!: FormGroup;
  submitted = false;
  error$: Observable<string | null>;
  loading$: Observable<boolean>;
  scheduling$: Observable<boolean>;

  patients = [
    { id: '550e8400-e29b-41d4-a716-446655440000', name: 'Sarah Johnson', mrn: '00-1234' },
    { id: '550e8400-e29b-41d4-a716-446655440001', name: 'Michael Chen', mrn: '00-2345' },
    { id: '550e8400-e29b-41d4-a716-446655440002', name: 'Emma Williams', mrn: '00-3456' },
    { id: '550e8400-e29b-41d4-a716-446655440003', name: 'Robert Davis', mrn: '00-4567' },
    { id: '550e8400-e29b-41d4-a716-446655440004', name: 'Linda Martinez', mrn: '00-5678' }
  ];

  providers = [
    { id: '550e8400-e29b-41d4-a716-446655440010', name: 'Dr. Patel' },
    { id: '550e8400-e29b-41d4-a716-446655440011', name: 'Dr. Smith' },
    { id: '550e8400-e29b-41d4-a716-446655440012', name: 'Dr. Garcia' },
    { id: '550e8400-e29b-41d4-a716-446655440013', name: 'Dr. Johnson' },
    { id: '550e8400-e29b-41d4-a716-446655440014', name: 'Dr. Lee' }
  ];

  appointmentTypes = Object.values(AppointmentType);
  durations = [15, 30, 45, 60];

  constructor(
    private fb: FormBuilder,
    private store: Store,
    private router: Router
  ) {
    this.error$ = this.store.select(selectError);
    this.loading$ = this.store.select(selectLoading);
    this.scheduling$ = this.store.select(selectScheduleInProgress);
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      patientId: ['', Validators.required],
      providerId: ['', Validators.required],
      appointmentType: [AppointmentType.Office, Validators.required],
      scheduledStart: ['', Validators.required],
      durationMinutes: [30, [Validators.required, Validators.min(15), Validators.max(480)]],
      reasonForVisit: ['', [Validators.required, Validators.minLength(3)]],
      notes: ['']
    });
  }

  submit(): void {
    if (this.form.invalid) return;

    const formValue = this.form.value;
    
    // Parse datetime-local format (YYYY-MM-DDTHH:mm)
    const scheduledStart = new Date(formValue.scheduledStart);

    const request: ScheduleAppointmentRequest = {
      patientId: formValue.patientId,
      providerId: formValue.providerId,
      scheduledStart,
      durationMinutes: parseInt(formValue.durationMinutes, 10),
      appointmentType: formValue.appointmentType,
      reasonForVisit: formValue.reasonForVisit,
      notes: formValue.notes || ''
    };

    this.store.dispatch(AppointmentActions.scheduleAppointment({ request }));
    this.submitted = true;

    setTimeout(() => {
      this.form.reset();
      this.submitted = false;
      this.router.navigate(['/appointments']);
    }, 2000);
  }

  getPatientName(): string {
    const patientId = this.form?.get('patientId')?.value;
    return this.patients.find(p => p.id === patientId)?.name || '';
  }

  isValid(): boolean {
    return this.form?.valid || false;
  }
}
