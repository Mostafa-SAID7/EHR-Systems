import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home-stats',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home-stats.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeStatsComponent {}
