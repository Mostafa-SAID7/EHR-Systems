import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-workflows',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home-workflows.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeWorkflowsComponent {}
