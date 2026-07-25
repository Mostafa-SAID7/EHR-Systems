# RxJS: Observables, Subjects, BehaviorSubject

## Observable (Lazy)

```typescript
import { Observable } from 'rxjs';

// Observable - lazy, nothing happens until subscribe
const obs$ = new Observable(subscriber => {
    console.log('Observable created');
    subscriber.next('Value 1');
    subscriber.next('Value 2');
    subscriber.complete();
});

// Nothing logged yet!

// Subscribe - NOW it runs
obs$.subscribe(value => console.log(value));
// Logs: "Observable created", "Value 1", "Value 2"
```

---

## Subject (Hot, Multicast)

```typescript
import { Subject } from 'rxjs';

const subject = new Subject<string>();

// Subscriber 1
subject.subscribe(value => console.log('Sub1:', value));

// Emit value
subject.next('Hello');

// Subscriber 2 (misses 'Hello')
subject.subscribe(value => console.log('Sub2:', value));

// Emit again
subject.next('World');

// Output:
// Sub1: Hello
// Sub1: World
// Sub2: World  ← Missed 'Hello'
```

---

## BehaviorSubject (Current Value)

```typescript
import { BehaviorSubject } from 'rxjs';

// Requires initial value
const subject = new BehaviorSubject<string>('Initial');

// Subscriber 1 gets initial value
subject.subscribe(value => console.log('Sub1:', value));

subject.next('Value 1');

// Subscriber 2 gets LAST value (Value 1)
subject.subscribe(value => console.log('Sub2:', value));

subject.next('Value 2');

// Output:
// Sub1: Initial
// Sub1: Value 1
// Sub2: Value 1  ← Gets last value!
// Sub1: Value 2
// Sub2: Value 2
```

---

## Real-World Use Cases

### User State Management

```typescript
export class UserService {
    private userSubject = new BehaviorSubject<User | null>(null);
    public user$ = this.userSubject.asObservable();
    
    public async login(email: string, password: string) {
        const user = await this.api.login(email, password).toPromise();
        this.userSubject.next(user); // Notify all subscribers
    }
    
    public logout() {
        this.userSubject.next(null);
    }
}

// In component
export class ProfileComponent implements OnInit {
    user$: Observable<User | null>;
    
    constructor(private userService: UserService) {
        this.user$ = this.userService.user$;
    }
}

// In template
<div *ngIf="user$ | async as user">
    Welcome {{ user.name }}
</div>
```

---

## ReplaySubject (Cache Values)

```typescript
import { ReplaySubject } from 'rxjs';

// Keep last 2 values
const subject = new ReplaySubject<string>(2);

subject.next('Value 1');
subject.next('Value 2');
subject.next('Value 3');

// Subscriber gets last 2 values
subject.subscribe(value => console.log(value));

// Output: Value 2, Value 3
```

---

## Common Operators

```typescript
import { Observable } from 'rxjs';
import { map, filter, debounceTime, switchMap } from 'rxjs/operators';

// map - Transform values
source$.pipe(
    map(user => user.name)
).subscribe(); // Logs names only

// filter - Keep only matching
source$.pipe(
    filter(user => user.age > 18)
).subscribe(); // Only adults

// debounceTime - Wait before emit
searchInput$.pipe(
    debounceTime(300), // Wait 300ms after typing stops
    switchMap(query => this.api.search(query))
).subscribe();
```

---

## Interview Q&A

**Q: Observable vs Promise?**

A:
- Observable: Lazy, cancellable, multiple values, powerful operators
- Promise: Eager, fires immediately, single value

**Q: Subject vs Observable?**

A:
- Observable: Lazy, cold
- Subject: Hot, multicast, starts emitting immediately

**Q: When to use BehaviorSubject?**

A: When you need current state (user, settings, theme)
