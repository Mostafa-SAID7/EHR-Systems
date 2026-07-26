# Modern EHR Platform

A production-ready, enterprise-grade Electronic Health Records system built with ASP.NET Core microservices and Angular 18.

## Stack

- **Frontend**: Angular 18, Tailwind CSS — runs on port **4200** via the `Frontend` workflow
- **Backend**: ASP.NET Core (.NET 8), EF Core, PostgreSQL, MediatR (CQRS), FluentValidation, Serilog, Mapster
- **Polyglot DB**: PostgreSQL (primary OLTP), Redis (caching), Elasticsearch (search), MongoDB (documents)
- **Messaging**: Kafka / MassTransit (inter-service events)

## How to Run on Replit

All 10 workflows start automatically. The full stack is live once every workflow shows **running**.

| Workflow | Port | Notes |
|---|---|---|
| Frontend | 4200 | Angular dev server — preview pane |
| API Gateway | 5000 | YARP reverse proxy — routes `/api/v1/*` to services |
| Identity Service | 5001 | Auth / JWT |
| Patient Service | 5002 | — |
| Clinical Service | 5003 | — |
| Appointment Service | 5004 | — |
| Audit Service | 5006 | — |
| Billing Service | 5007 | — |
| Prescription Service | 5008 | — |
| Analytics Service | 5009 | — |

**Database**: Replit's managed PostgreSQL is used automatically — each service reads `PGHOST`/`PGPORT`/`PGUSER`/`PGPASSWORD`/`PGDATABASE` from the environment and creates its own schema tables on first boot. No manual DB setup needed.

**Optional services** (Redis, Elasticsearch, MongoDB, Kafka, RabbitMQ) are not available in this environment; all services degrade gracefully — caching and search are disabled, messaging falls back to in-process loopback.

### Frontend install
If `node_modules` are missing (fresh clone), run:
```bash
cd frontend && npm install --legacy-peer-deps
```
The `--legacy-peer-deps` flag is required due to a peer dependency conflict between `ng2-charts@4` and `@angular/cdk@22`.

## Project Structure

```
frontend/                        # Angular 18 SPA
  src/
    app/
      core/                      # Auth guards, interceptors, services
      shared/components/         # UI library (card, button, sidebar, topbar, etc.)
      features/                  # Pages: dashboard, patients, appointments, billing, ...
      layouts/                   # main-layout, auth-layout
    styles/styles.scss           # ← SINGLE source of truth for all design tokens + component classes

backend/
  src/
    EHRPlatform.Common/          # Shared: CQRS, repo, domain events, Redis, Elasticsearch, MongoDB
    EHRPlatform.Services.*/      # Individual microservices (Identity, Patient, Clinical, ...)
  EHRPlatform.sln

devops/                          # Docker, Kubernetes, CI/CD, Terraform, monitoring
docs/                            # Architecture, API spec, DB schema, security
```

## Frontend Design System

All styles are centralized in `frontend/src/styles/styles.scss` and `frontend/tailwind.config.js`.

**Never add one-off Tailwind utilities inline when a design-system class exists.** Key classes:

| Class | Purpose |
|---|---|
| `.card`, `.card-hover`, `.card-glass`, `.card-green` | Card variants |
| `.card-header` | Card top bar (title + action) |
| `.btn-primary`, `.btn-secondary`, `.btn-ghost`, `.btn-danger` | Button variants |
| `.btn-icon`, `.btn-icon-sm` | Icon-only buttons |
| `.view-toggle`, `.view-toggle-btn`, `.view-toggle-btn-active` | Tab/view switcher |
| `.dropdown-item` | Dropdown menu rows |
| `.link-primary`, `.link-sm` | Green text links with chevron |
| `.stat-card`, `.mini-stat` | KPI / metric cards |
| `.grid-stats`, `.grid-2`, `.grid-3`, `.grid-4`, `.grid-3-stats` | Responsive grids |
| `.badge-*` | Status badges (primary/success/warning/danger/info/neutral) |
| `.icon-box-*` | Colored icon containers |
| `.avatar`, `.avatar-*`, `.avatar-custom-*` | Avatar variants |
| `.stagger` | Entrance animation on child elements |
| `.animate-fade-in-up`, `.animate-count-up`, etc. | Motion utilities |
| `.filter-pill`, `.filter-pill-active` | Filter tab buttons |
| `.progress-bar` / `.progress-fill` | Progress indicator |
| `.input-base`, `.input-icon`, `.input-error` | Form inputs |
| `.heading-xl` → `.heading-sm` | Typography scale |
| `.body-text`, `.caption`, `.label-text`, `.muted` | Text styles |

**Color palette**: Primary is emerald green (`primary-600` = `#16a34a`). Surface neutrals in `surface-*`. Full token list in `:root` CSS variables.

## User Preferences

- Keep styles centralized — extend `styles.scss` rather than adding inline Tailwind duplicates
- Green-first cinematic aesthetic: atmospheric gradients, no hard side-border accents
- All grids must be responsive (always start with `grid-cols-1` on mobile)
- Animations should feel natural, not jarring — use the existing easing variables
