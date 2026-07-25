import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Patient } from '../../../core/models';

/**
 * Patient Header — sticky cinematic header, no left-border patterns
 */
@Component({
  selector: 'app-patient-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      *ngIf="patient"
      class="sticky top-0 z-30
             bg-white/95 dark:bg-surface-900/95 backdrop-blur-md
             border-b border-surface-200 dark:border-surface-800
             shadow-sm"
    >
      <div class="px-6 py-4">
        <div class="flex items-center justify-between gap-6 flex-wrap">

          <!-- Avatar + info -->
          <div class="flex items-center gap-4 min-w-0">
            <div class="relative shrink-0">
              <div class="w-12 h-12 rounded-2xl bg-gradient-to-br from-primary-400 to-primary-600
                          flex items-center justify-center text-white text-base font-bold shadow-sm">
                {{ getInitials() }}
              </div>
              <!-- Status indicator -->
              <span class="absolute -bottom-0.5 -right-0.5 w-3.5 h-3.5 rounded-full
                           bg-primary-500 ring-2 ring-white dark:ring-surface-900"></span>
            </div>

            <div class="min-w-0">
              <h3 class="text-base font-bold text-gray-900 dark:text-white truncate">
                {{ patient.firstName }} {{ patient.lastName }}
              </h3>
              <div class="flex items-center gap-3 mt-0.5 flex-wrap">
                <span class="text-xs text-gray-500 dark:text-gray-400">
                  MRN <span class="font-semibold text-gray-700 dark:text-gray-300">{{ patient.mrn }}</span>
                </span>
                <span class="w-px h-3 bg-surface-300 dark:bg-surface-600"></span>
                <span class="text-xs text-gray-500 dark:text-gray-400">
                  DOB <span class="font-semibold text-gray-700 dark:text-gray-300">{{ patient.dateOfBirth | date:'MMM d, yyyy' }}</span>
                </span>
                <span class="w-px h-3 bg-surface-300 dark:bg-surface-600"></span>
                <span class="text-xs text-gray-500 dark:text-gray-400">
                  Age <span class="font-semibold text-gray-700 dark:text-gray-300">{{ getAge() }}</span>
                </span>
                <span *ngIf="patient.gender"
                  class="badge badge-neutral capitalize">
                  {{ patient.gender }}
                </span>
              </div>
            </div>
          </div>

          <!-- Quick metrics -->
          <div class="hidden lg:flex items-center gap-6">
            <div class="text-center">
              <div class="text-xl font-bold text-red-600 dark:text-red-400 tabular-nums">
                {{ patient.allergies?.length || 0 }}
              </div>
              <div class="text-2xs text-gray-500 uppercase tracking-wide font-medium">Allergies</div>
            </div>
            <div class="w-px h-8 bg-surface-200 dark:bg-surface-700"></div>
            <div class="text-center">
              <div class="text-xl font-bold text-yellow-600 dark:text-yellow-400 tabular-nums">
                {{ patient.chronicConditions?.length || 0 }}
              </div>
              <div class="text-2xs text-gray-500 uppercase tracking-wide font-medium">Conditions</div>
            </div>
            <div class="w-px h-8 bg-surface-200 dark:bg-surface-700"></div>
            <div class="text-center">
              <a *ngIf="patient.phone" [href]="'tel:' + patient.phone"
                class="text-xl font-bold text-primary-600 hover:text-primary-700 dark:text-primary-400
                       transition-colors tabular-nums">
                📞
              </a>
              <div class="text-2xs text-gray-500 uppercase tracking-wide font-medium">
                {{ patient.phone || '—' }}
              </div>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex items-center gap-2 shrink-0">
            <button class="px-4 py-2 text-sm font-semibold text-white
                           bg-primary-600 hover:bg-primary-700
                           rounded-xl shadow-sm hover:shadow-md
                           transition-all duration-200">
              Edit patient
            </button>
            <button class="flex items-center justify-center w-9 h-9
                           rounded-xl text-gray-500 hover:text-gray-700
                           hover:bg-surface-100 dark:hover:bg-surface-800
                           border border-surface-200 dark:border-surface-700
                           transition-all duration-200">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M12 5v.01M12 12v.01M12 19v.01M12 6a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2zm0 7a1 1 0 110-2 1 1 0 010 2z"/>
              </svg>
            </button>
          </div>
        </div>

        <!-- Alert strip -->
        <div *ngIf="hasAlerts()" class="flex flex-wrap gap-2 mt-3 pt-3 border-t border-surface-100 dark:border-surface-800">
          <div *ngIf="patient.allergies?.length"
            class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl
                   bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-300
                   text-xs font-medium">
            <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd"/>
            </svg>
            {{ patient.allergies!.length }} Allerg{{ patient.allergies!.length === 1 ? 'y' : 'ies' }}
          </div>
          <div *ngIf="patient.chronicConditions?.length"
            class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl
                   bg-yellow-50 dark:bg-yellow-900/20 text-yellow-700 dark:text-yellow-300
                   text-xs font-medium">
            <svg class="w-3.5 h-3.5" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clip-rule="evenodd"/>
            </svg>
            {{ patient.chronicConditions!.length }} Chronic Condition{{ patient.chronicConditions!.length === 1 ? '' : 's' }}
          </div>
        </div>
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientHeaderComponent {
  @Input() patient!: Patient;

  getInitials(): string {
    return [this.patient?.firstName, this.patient?.lastName]
      .filter(Boolean)
      .map(n => n[0].toUpperCase())
      .join('');
  }

  getAge(): string {
    if (!this.patient?.dateOfBirth) return '—';
    const dob = new Date(this.patient.dateOfBirth);
    const today = new Date();
    let age = today.getFullYear() - dob.getFullYear();
    if (today.getMonth() < dob.getMonth() ||
       (today.getMonth() === dob.getMonth() && today.getDate() < dob.getDate())) {
      age--;
    }
    return `${age}y`;
  }

  hasAlerts(): boolean {
    return !!(this.patient?.allergies?.length || this.patient?.chronicConditions?.length);
  }
}
