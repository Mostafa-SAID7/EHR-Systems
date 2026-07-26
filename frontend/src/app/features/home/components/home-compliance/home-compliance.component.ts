import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-compliance',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home-compliance.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComplianceComponent {}
