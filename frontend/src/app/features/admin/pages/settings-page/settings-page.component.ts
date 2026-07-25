import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardComponent } from '../../../../shared/components/ui/card/card.component';

/**
 * settings-page Component
 * Page for settings-page
 */
@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule, CardComponent],
  template: `
    <app-card title="settings-page">
      <div class="text-center py-12">
        <p class="text-gray-600 dark:text-gray-400">
          settings-page page - Implementation in progress
        </p>
      </div>
    </app-card>
  `,
  styles: [],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsPageComponent implements OnInit {
  ngOnInit(): void {
    // Initialize component
  }
}
