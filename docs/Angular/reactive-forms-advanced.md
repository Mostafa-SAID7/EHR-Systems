# Angular Reactive Forms Advanced

## FormGroup with Validators

```typescript
import { FormBuilder, Validators } from '@angular/forms';

export class UserFormComponent implements OnInit {
    userForm: FormGroup;
    
    constructor(private fb: FormBuilder) {
        this.userForm = this.fb.group({
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required, Validators.minLength(8)]],
            confirmPassword: ['', Validators.required],
            age: [null, [Validators.required, Validators.min(18)]]
        }, { validators: this.passwordMatchValidator });
    }
    
    // Custom validator - compare fields
    passwordMatchValidator(group: FormGroup): { [key: string]: any } | null {
        const password = group.get('password')?.value;
        const confirmPassword = group.get('confirmPassword')?.value;
        
        if (password !== confirmPassword) {
            return { passwordMismatch: true };
        }
        return null;
    }
    
    onSubmit() {
        if (this.userForm.invalid) return;
        
        const formData = this.userForm.value;
        console.log(formData);
    }
}
```

---

## Async Validators

```typescript
export class EmailValidatorService {
    constructor(private api: ApiService) {}
    
    validateEmailUnique(): AsyncValidatorFn {
        return (control: AbstractControl): Observable<{ [key: string]: any } | null> => {
            if (!control.value) return of(null);
            
            return this.api.checkEmailExists(control.value).pipe(
                map(exists => exists ? { emailExists: true } : null),
                catchError(() => of(null))
            );
        };
    }
}

// Usage
this.userForm = this.fb.group({
    email: ['', Validators.required, this.emailValidator.validateEmailUnique()]
});
```

---

## FormArray

```typescript
export class AddressListComponent {
    form: FormGroup;
    
    constructor(private fb: FormBuilder) {
        this.form = this.fb.group({
            addresses: this.fb.array([])
        });
    }
    
    get addresses(): FormArray {
        return this.form.get('addresses') as FormArray;
    }
    
    addAddress() {
        this.addresses.push(this.fb.group({
            street: ['', Validators.required],
            city: ['', Validators.required],
            zipCode: ['']
        }));
    }
    
    removeAddress(index: number) {
        this.addresses.removeAt(index);
    }
}
```

Template:
```html
<form [formGroup]="form">
    <div formArrayName="addresses">
        <div *ngFor="let address of addresses.controls; let i = index" [formGroupName]="i">
            <input formControlName="street" placeholder="Street">
            <input formControlName="city" placeholder="City">
        </div>
    </div>
    <button (click)="addAddress()">Add Address</button>
</form>
```

---

## Interview Q&A

**Q: Template-driven vs Reactive forms?**

A:
- Template-driven: Simple forms, less code
- Reactive: Complex forms, more control, better testing

**Q: How to track FormGroup status?**

A:
```typescript
this.form.statusChanges.subscribe(status => {
    console.log(status); // VALID, INVALID, PENDING
});
```
