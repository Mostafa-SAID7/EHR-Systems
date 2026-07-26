import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-features',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home-features.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeFeaturesComponent {}
