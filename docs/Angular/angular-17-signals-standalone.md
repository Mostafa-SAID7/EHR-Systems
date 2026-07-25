# Angular 17+: Signals & Standalone Components

## What are Signals?

```typescript
import { signal, computed, effect } from '@angular/core';

// Create signal (reactive value)
const count = signal(0);
const name = signal('Ahmed');

// Read value
console.log(count()); // 0

// Update value
count.set(5); // Replace
count.update(v => v + 1); // Modify
```

---

## Signals vs RxJS Observables

| Feature | Signal | Observable |
|---------|--------|-----------|
| Type | Primitive value | Stream |
| Syntax | `signal()` | `Subject` |
| Read | `signal()` | `.subscribe()` |
| Update | `.set()` | `.next()` |
| Learning | Easier | Steeper |
| Use | Local state | Complex flows |

---

## Computed Signals

```typescript
const count = signal(0);
const doubled = computed(() => count() * 2);

count.set(5);
console.log(doubled()); // 10 (auto-updated)
```

---

## Effects (Side Effects)

```typescript
const count = signal(0);
const status = signal('');

// Run whenever count changes
effect(() => {
    status.set(`Count is: ${count()}`);
    console.log(`Count changed to: ${count()}`);
});

count.set(5); // Runs effect
```

---

## Standalone Components

```typescript
import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-counter',
    standalone: true,  // ✅ Standalone
    imports: [CommonModule, FormsModule],
    template: `
        <div>
            <p>Count: {{ count() }}</p>
            <button (click)="count.set(count() + 1)">Increment</button>
        </div>
    `
})
export class CounterComponent {
    count = signal(0);
}
```

---

## Routing with Standalone

```typescript
import { Routes } from '@angular/router';
import { HomeComponent } from './home.component';
import { AboutComponent } from './about.component';

export const routes: Routes = [
    { path: '', component: HomeComponent },
    { path: 'about', component: AboutComponent },
    {
        path: 'admin',
        component: AdminComponent,
        canActivate: [authGuard],
        children: [
            { path: 'users', component: UsersComponent }
        ]
    }
];
```

---

## Interview Q&A

**Q: Signals vs Observable - which to use?**

A:
- Signals: Simple local state (component counter)
- Observables: Complex async flows (HTTP requests, events)

**Q: Why Standalone Components?**

A:
- No NgModule boilerplate
- Simpler mental model
- Easier to tree-shake
- Modern Angular best practice
