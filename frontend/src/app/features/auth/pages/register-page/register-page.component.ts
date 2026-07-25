import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * register-page Component
 * Page for register-page
 */
@Component({
  selector: 'app-register-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="register-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          register-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
