import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-footer',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home-footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeFooterComponent {
  @Output() quickLogin = new EventEmitter<string>();
}
