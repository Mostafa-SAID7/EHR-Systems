import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home-hero',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home-hero.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeHeroComponent {
  @Output() quickLogin = new EventEmitter<string>();
}
