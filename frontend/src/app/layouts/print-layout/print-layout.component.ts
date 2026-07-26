import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

/**
 * Print Layout Component
 * Layout for printable documents (reports, prescriptions, etc.)
 */
@Component({
  selector: 'app-print-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './print-layout.component.html',
  styleUrl: './print-layout.component.scss'
})
export class PrintLayoutComponent {}
