# ASP.NET Core Migration Manifest (Side?by?Side Parallel Track)

Purpose
- Run a new ASP.NET Core 8 solution slice beside the existing ASP.NET MVC 5 / Identity 2 / OWIN application.
- Preserve uninterrupted development + production stability while incrementally porting features.
- Allow isolated iteration (own ports + own dev database) with an eventual single cutover.

Parallel Structure (Proposed)
- Existing: FlightJournal.Web (MVC5, EF6, Identity 2)
- New (add):
  - FlightJournal.Domain (net8.0 class lib) – pure domain models + services (no System.Web).
  - FlightJournal.Data (net8.0 class lib) – EF Core DbContext + migrations.
  - FlightJournal.WebCore (net8.0 ASP.NET Core web app – MVC + Razor + SignalR + Identity Core).
  - (Optional) FlightJournal.Shared (netstandard2.0 if needed for temporary sharing while legacy still net48).

Ports & Hosting (Dev)
- Legacy IIS Express: http://localhost:5000/ (unchanged)
- New IIS Express: http://localhost:6000/
- New Kestrel (if launched directly): http://localhost:5100 / https://localhost:5101

Databases (Dev)
- Legacy DB: FlightJournal_Dev
- New DB (initial isolated): FlightJournalCore_Dev
- Optional convergence phase: Migrate to shared schema FlightJournal_Dev after schema parity & data migration/passive read tests.

Environment / Settings
- ASP.NET Core user-secrets or appsettings.Development.json for new connection string: "FlightJournalCore".
- DataProtection key ring persisted to %LOCALAPPDATA%\FlightJournal.Keys (later can unify for cookie continuity if sharing auth domain).

High-Level Phases
1. Baseline & Test Safety
2. Domain Extraction / Duplication
3. New Core Shell + Identity & Auth Compatibility
4. EF Core Model + Migrations
5. Basic MVC + Layout + Static Assets Pipeline
6. Key Feature Strangling (Controllers / APIs / SignalR)
7. Cross-Cutting Concerns (Logging, Telemetry, Security Headers)
8. Data Validation & Coexistence (Optional shared DB switch)
9. Performance / Parity Verification
10. Cutover & Decommission Legacy

Detailed Steps / Deliverables

Phase 1 – Baseline
- Add/expand automated tests around auth, critical controllers, SignalR hubs in legacy.
- Capture current DB schema (script) & sample anonymized data set.
- Acceptance: Test suite green; schema script stored under /db/legacy-schema.sql.

Phase 2 – Domain Layer
- Identify domain classes free of System.Web references; move/port to FlightJournal.Domain.
- Introduce interfaces (e.g., IFlightLogService) decoupled from MVC specifics.
- Acceptance: Legacy builds referencing new domain lib via multi-target (if needed net48, net8.0) OR duplicate temporarily then reconcile.

Phase 3 – New WebCore Bootstrap
- dotnet new mvc -au Individual -o FlightJournal.WebCore
- Remove scaffolded Identity UI if replacing with custom UI later; keep infrastructure.
- Configure Identity password compatibility: builder.Services.Configure<PasswordHasherOptions>(o => o.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2);
- Acceptance: App runs on :6000 with registration/login using new isolated DB.

Phase 4 – EF Core Migration
- Scaffold DbContext + entities to match (or evolve) schema.
- Add initial EF Core migration (BaselineCreate). Use Fluent API for table names matching existing (AspNetUsers etc.) if future shared DB desired.
- Acceptance: New DB created & migrations apply cleanly; CRUD smoke tests pass.

Phase 5 – UI & Assets
- Copy shared layout, partials, and styles into WebCore (wwwroot/css, js). Replace System.Web.Optimization with static or modern bundler (esbuild/Vite optional later).
- Replace @Styles.Render/@Scripts.Render with <link>/<script> tags or bundler output.
- Acceptance: Key pages visually acceptable; no 404 static resources.

Phase 6 – Feature Strangling
Priority order:
  1. Authentication / Account management
  2. Core flight logging & retrieval endpoints
  3. Reporting views / exports
  4. SignalR live updates (migrate to ASP.NET Core SignalR @microsoft/signalr client)
  5. Ancillary / admin features
Port pattern per feature:
  - Create parallel controller in WebCore (namespace preserved, route versioning optional /v2 prefix initially).
  - Write integration tests hitting new endpoints.
  - (Optional) Add reverse proxy entries (YARP or IIS rewrite) to route selected paths from legacy to Core for early dogfooding.
- Acceptance: Each migrated feature has parity test or validation script.

Phase 7 – Cross-Cutting
- Logging: Use Microsoft.Extensions.Logging (AI provider) + optionally bridge log4net if needed.
- Telemetry: Add Application Insights SDK (connection string) + custom telemetry initializers cloning legacy context properties.
- Security: Add middleware for HTTPS redirection, HSTS (dev relaxed), anti-forgery, same-site cookies, security headers (Content-Security-Policy staged in report-only).
- Acceptance: AI shows correlated traces; security headers present (scan script).

Phase 8 – Optional Shared DB Convergence
- Add legacy-missing Identity columns to legacy DB (EF6 migration) so both apps can share.
- Point WebCore to legacy DB; validate logins, role resolution, claims.
- Run read-only soak tests, then enable writes.
- Acceptance: Dual-run mode stable for agreed window (e.g., 1 week dev).

Phase 9 – Performance & Parity
- Measure controller latency (BenchmarkDotNet micro for services + ApacheBench / oha for HTTP).
- Fix regressions >15% slower than legacy.
- Acceptance: Performance parity report committed.

Phase 10 – Cutover & Decommission
- Switch primary site binding to WebCore (keep legacy behind admin-only route temporarily).
- Freeze legacy repo path; remove System.Web-specific packages gradually.
- Archive instructions in /docs/migration-final.md.
- Acceptance: Legacy not serving user traffic; monitoring stable.

Identity & Auth Notes
- Keep normalized email/user columns identical.
- Use same password requirements initially; enforce stronger after rehash cycle completes.
- External providers: swap to Microsoft.AspNetCore.Authentication.* packages.
- DataProtection persistence required only when sharing cookie decryption across instances/domains.

SignalR Migration
- Replace hubs inheriting Hub (Core) from Microsoft.AspNetCore.SignalR.
- Use strongly typed hub interfaces for compile-time safety.
- JS client: import { HubConnectionBuilder } from '@microsoft/signalr'.
- Auth: Ensure cookie auth or JWT configured before app.MapHub<FlightHub>("/hubs/flight").

Configuration Mapping (Legacy -> Core)
- web.config appSettings -> appsettings.json.
- connectionStrings -> ConnectionStrings section.
- log4net.config (optional) -> AI + Serilog (if adopted) or keep minimal log4net via adapter.

Recommended NuGet (Core Side)
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore (SqlServer + Tools + Design)
- Microsoft.ApplicationInsights.AspNetCore
- Microsoft.AspNetCore.SignalR
- (Optional) Serilog.AspNetCore + sinks

Checklist (Mark as you progress)
[ ] Create new solution folders + projects
[ ] Configure new ports (launchSettings.json)
[ ] Implement Identity compatibility
[ ] EF Core baseline migration
[ ] Copy layout & static assets
[ ] First feature migrated & tested
[ ] SignalR hub migrated
[ ] Logging/Telemetry parity
[ ] Shared DB convergence (if chosen)
[ ] Performance validation
[ ] Cutover executed

launchSettings.json (WebCore example)
{
  "profiles": {
    "FlightJournal.WebCore": {
      "commandName": "Project",
      "launchBrowser": true,
      "applicationUrl": "http://localhost:6000;https://localhost:6100",
      "environmentVariables": { "ASPNETCORE_ENVIRONMENT": "Development" }
    }
  }
}

Password Hasher Configuration (Program.cs)
builder.Services.Configure<PasswordHasherOptions>(o =>
{
    o.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV2;
});

EF Core DbContext Table Mapping (Example)
protected override void OnModelCreating(ModelBuilder b)
{
    base.OnModelCreating(b);
    b.Entity<ApplicationUser>().ToTable("AspNetUsers");
    b.Entity<IdentityRole>().ToTable("AspNetRoles");
    // etc. for other Identity tables if customizing names
}

Risk Mitigations
- Keep migrations idempotent; never destructive until cutover.
- Feature toggles (simple config flags) to fall back to legacy paths.
- Continuous integration builds both legacy + new to detect divergence early.

Exit Criteria
- 100% of user-facing endpoints served by WebCore.
- No auth fallbacks to legacy observed in logs for 7 consecutive days.
- Performance & error budget within agreed SLO.

Post-Cutover Hardening
- Flip password hasher to latest; enforce improved password policy.
- Enable strict CSP (disable report-only).
- Remove legacy projects/directories after archival tag.

This document is the authoritative migration manifest. Keep it updated as decisions change.
