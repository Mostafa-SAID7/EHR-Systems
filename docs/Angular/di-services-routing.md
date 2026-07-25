# Angular Dependency Injection, Services & HTTP Client

## 1. Dependency Injection Fundamentals
```typescript
// providedIn: 'root' → singleton across entire app
@Injectable({ providedIn: 'root' })
export class PatientService { }

// providedIn: 'any' → fresh instance per lazy-loaded module
@Injectable({ providedIn: 'any' })
export class ScopedFormService { }

// Component-level provider → fresh instance per component tree
@Component({
  providers: [LocalFormStateService] // new instance for this component + children
})
export class VisitFormComponent { }
```

### Hierarchical Injector Chain
```
Root Injector (providedIn: 'root')
  └── Module Injector (lazy-loaded modules)
        └── Component Injector (providers: [...])
              └── Element Injector (directive providers)
```

## 2. HTTP Client — CRUD Operations
```typescript
@Injectable({ providedIn: 'root' })
export class CodingApiService {
  private readonly baseUrl = '/api/v1/coding';

  constructor(private http: HttpClient) {}

  // GET with typed response
  getSuggestions(visitId: number): Observable<CodeSuggestion[]> {
    return this.http.get<CodeSuggestion[]>(`${this.baseUrl}/suggestions/${visitId}`);
  }

  // POST with body
  confirmCode(visitId: number, code: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/confirm`, { visitId, code });
  }

  // DELETE
  removeCode(visitId: number, codeId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${visitId}/codes/${codeId}`);
  }

  // GET with query params
  searchPatients(term: string, page: number): Observable<PagedResult<Patient>> {
    const params = new HttpParams().set('q', term).set('page', page);
    return this.http.get<PagedResult<Patient>>('/api/v1/patients', { params });
  }
}
```

## 3. HTTP Interceptors (Auth Token + Error Handling)
```typescript
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.logout();
        inject(Router).navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};

// Register in app.config.ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
};
```

## 4. Routing — Guards, Lazy Loading & Resolvers
```typescript
export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },

  // Lazy-loaded feature module
  {
    path: 'patients',
    canActivate: [authGuard],
    loadChildren: () => import('./features/patients/patients.routes')
      .then(m => m.PATIENT_ROUTES)
  },

  // Route with resolver (pre-fetch data before component loads)
  {
    path: 'visits/:id',
    resolve: { visit: visitResolver },
    component: VisitDetailComponent
  }
];

// Functional guard (Angular 15+)
export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) return true;

  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};

// Functional resolver
export const visitResolver: ResolveFn<Visit> = (route) => {
  return inject(VisitService).getById(+route.paramMap.get('id')!);
};
```
