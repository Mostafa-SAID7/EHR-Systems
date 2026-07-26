import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-solutions',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home-solutions.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeSolutionsComponent {}
