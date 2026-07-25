# Angular Performance, Pipes, Security & Testing

## 1. Performance Optimization

### Lazy Loading Routes
```typescript
// Split code by feature — only load when user navigates there
{
  path: 'admin',
  loadChildren: () => import('./features/admin/admin.routes').then(m => m.ADMIN_ROUTES)
}
```

### trackBy in @for loops (Prevent Full Re-Render)
```typescript
// Without trackBy: Angular destroys and recreates ALL DOM nodes on array change
// With trackBy: Angular only updates changed items

@Component({
  template: `
    @for (patient of patients; track patient.id) {
      <app-patient-row [patient]="patient" />
    }
  `
})
```

### Deferrable Views (Angular 17+)
```html
<!-- Load component only when it enters viewport -->
@defer (on viewport) {
  <app-audit-history [visitId]="visitId" />
} @placeholder {
  <div class="skeleton-loader"></div>
} @loading {
  <app-spinner />
}
```

---

## 2. Pipes — Data Transformation
```typescript
// Built-in
{{ patient.name | uppercase }}
{{ appointment.date | date:'dd/MM/yyyy' }}
{{ revenue | currency:'USD':'symbol':'1.2-2' }}
{{ isActive | async }}  // ← unwrap Observable/Promise

// Custom Pure Pipe (cached — runs only when input reference changes)
@Pipe({ name: 'icdDescription', pure: true, standalone: true })
export class IcdDescriptionPipe implements PipeTransform {
  constructor(private icdService: IcdService) {}

  transform(code: string): string {
    return this.icdService.getDescription(code) ?? code;
  }
}

// Usage in template
{{ 'E11.9' | icdDescription }}
```

---

## 3. Angular Security
```typescript
// ✅ Angular auto-escapes interpolation — XSS-safe
{{ userInput }}  // safe — Angular escapes HTML

// ❌ innerHTML — bypasses escaping — NEVER with user input
<div [innerHTML]="userContent"></div>

// ✅ Use DomSanitizer only for trusted content
constructor(private sanitizer: DomSanitizer) {}

get safeHtml() {
  return this.sanitizer.bypassSecurityTrustHtml(this.trustedTemplate);
}

// ✅ Route Guards block unauthorized navigation (server must ALSO validate)
// ✅ Store JWT in httpOnly Cookie (not localStorage) to prevent XSS theft
// ✅ Set CSP headers on server to restrict script execution sources
```

---

## 4. Angular Testing (TestBed)
```typescript
describe('PatientCardComponent', () => {
  let component: PatientCardComponent;
  let fixture: ComponentFixture<PatientCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientCardComponent],
      providers: [
        { provide: PatientService, useValue: { getById: () => of(mockPatient) } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PatientCardComponent);
    component = fixture.componentInstance;
    component.patient = mockPatient;
    fixture.detectChanges();
  });

  it('should display patient name', () => {
    const el = fixture.nativeElement.querySelector('h2');
    expect(el.textContent).toContain('John Doe');
  });

  it('should emit codeSelected when button clicked', () => {
    let emitted: string | undefined;
    component.codeSelected.subscribe(code => emitted = code);

    fixture.nativeElement.querySelector('button').click();
    expect(emitted).toBe('E11.9');
  });
});
```

---

## 5. Angular Decorators Reference
| Decorator | Purpose |
|:--- |:--- |
| `@Component` | Define component metadata (selector, template, styles) |
| `@Injectable` | Mark class for DI system |
| `@Input()` | Accept data from parent |
| `@Output()` | Emit events to parent |
| `@ViewChild` | Access first matching child in template |
| `@ViewChildren` | Access all matching children |
| `@HostListener` | Listen to host element events |
| `@HostBinding` | Bind to host element property |
| `@Pipe` | Define a pipe |
