# Angular Core Fundamentals — Components, Templates & Data Binding

## 1. Component Anatomy
```typescript
@Component({
  selector: 'app-patient-card',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './patient-card.component.html',
  styleUrl: './patient-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PatientCardComponent {
  @Input({ required: true }) patient!: Patient;
  @Output() codeSelected = new EventEmitter<string>();

  selectCode(code: string): void {
    this.codeSelected.emit(code);
  }
}
```

## 2. Component Lifecycle Hooks
| Hook | When Called | Common Use |
|:--- |:--- |:--- |
| `ngOnInit` | After first `ngOnChanges` | HTTP calls, init logic |
| `ngOnChanges` | When `@Input` value changes | React to parent input changes |
| `ngOnDestroy` | Before component removed | Unsubscribe observables, clear timers |
| `ngAfterViewInit` | After view & child views initialized | Access `@ViewChild` elements |
| `ngDoCheck` | Every change detection run | Custom change detection |

## 3. Template Syntax & Directives
```html
<!-- Interpolation -->
<h2>{{ patient.name | titlecase }}</h2>

<!-- Property Binding -->
<img [src]="patient.photoUrl" [alt]="patient.name">

<!-- Event Binding -->
<button (click)="selectCode('E11.9')">Assign Code</button>

<!-- Two-Way Binding -->
<input [(ngModel)]="searchTerm" placeholder="Search patients...">

<!-- Structural Directives -->
@if (patient.isActive) {
  <span class="badge active">Active</span>
} @else {
  <span class="badge inactive">Inactive</span>
}

@for (code of patient.codes; track code.id) {
  <li>{{ code.display }}</li>
} @empty {
  <li>No codes assigned.</li>
}
```

## 4. OnPush Change Detection Strategy
```typescript
// Default: Angular checks every component on every event
// OnPush: Angular only checks when:
//   1. @Input reference changes
//   2. Async pipe emits new value
//   3. Manual markForCheck() called
//   4. Component event fires

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PatientListComponent {
  patients$ = this.patientService.getAll$(); // async pipe triggers detection

  constructor(
    private patientService: PatientService,
    private cdr: ChangeDetectorRef
  ) {}

  // Manually trigger detection after imperative update
  refreshManually(): void {
    this.cdr.markForCheck();
  }
}
```

## 5. ViewChild & ContentChild
```typescript
@Component({ template: `<input #searchInput>` })
export class SearchComponent implements AfterViewInit {
  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;

  ngAfterViewInit(): void {
    this.searchInput.nativeElement.focus(); // Safe — view fully initialized
  }
}
```

## 6. Component Communication Patterns
```typescript
// Parent → Child: @Input
// Child → Parent: @Output EventEmitter
// Sibling → Sibling: Shared Service with BehaviorSubject
// Any → Any: NgRx Store / Signals

// Shared Service pattern
@Injectable({ providedIn: 'root' })
export class CodingStateService {
  private selectedCode = new BehaviorSubject<string | null>(null);
  selectedCode$ = this.selectedCode.asObservable();

  selectCode(code: string): void {
    this.selectedCode.next(code);
  }
}
```
