# Evidence: Organization (Phase 2, sector 3 of 11) — Domain, Application, Infrastructure

Date: 2026-08-19
Environment: local dev machine, .NET SDK 8.0.424, EF Core 8.0.11, SQL Server
LocalDB `mssqllocaldb`.

Scope and decisions are recorded in
`docs/adr/ADR-017-organization-phase2-scope-and-persistence.md`. This report
is the real proof: twenty-five of thirty-seven atlas tables modeled in
Domain (the passport-provider spine plus the atlas's own three named
verification queries), EF Core persistence against a new, genuinely
separate physical database (`OrganizationDb`), the six Application-layer
operations the atlas names as real behavior, composed into
`Nexus1.ModularRuntime`, `Nexus1.ArchitectureTests` still green, and a real
host startup with a working `organization-db` health check — verified
independently after the implementation pass, not just taken on report.

## Automated regression: 406/406 passing (was 294/294 before this step)

```
Nexus1.ReactorFleet.UnitTests                     12/12 passed
Nexus1.RootCause.UnitTests                        22/22 passed
Nexus1.AlarmManagement.UnitTests                  16/16 passed
Nexus1.Audit.UnitTests                             3/3  passed
Nexus1.Compliance.UnitTests                        2/2  passed
Nexus1.Reporting.UnitTests                         4/4  passed
Nexus1.CorePlatform.UnitTests                     46/46 passed
Nexus1.Security.UnitTests                         31/31 passed
Nexus1.Organization.UnitTests                     97/97 passed  (new)
Nexus1.BuildingBlocks.Observability.UnitTests     40/40 passed
Nexus1.ReactorFleet.ComponentTests                 3/3  passed
Nexus1.RootCause.ComponentTests                   24/24 passed
Nexus1.AlarmManagement.ComponentTests              19/19 passed
Nexus1.Audit.ComponentTests                       13/13 passed
Nexus1.Compliance.ComponentTests                  13/13 passed
Nexus1.Reporting.ComponentTests                   16/16 passed
Nexus1.CorePlatform.ComponentTests                 9/9  passed
Nexus1.Security.ComponentTests                    14/14 passed
Nexus1.Organization.ComponentTests                15/15 passed  (new)
Nexus1.ArchitectureTests                            7/7  passed
```

Full solution build: 0 warnings, 0 errors, run cleanly with `-m:1` (serial
per-project execution) to get an unambiguous per-project breakdown rather
than the interleaved/mislabeled console output `dotnet test` on the whole
solution produces under its default parallel scheduling — a presentation
artifact, not a test failure; every project passed either way, this just
made the counts trustworthy to transcribe.

`Nexus1.Contracts.ContractTests` and `Nexus1.DistributedSlice.EndToEndTests`
report "No test is available" — a pre-existing condition unrelated to this
step (confirmed by re-checking the raw VSTest output directly), not a new
regression.

## What was verified against the source material before writing code

- `From_Schema_to_System` Appendix C.3: **thirty-seven tables** (ten lookup,
  twenty-seven substantive). C.3.1 names Organization as one of the main
  passport providers for `ReactorFleet`, `Maintenance`,
  `RadiationMonitoring`, `EmergencyPreparedness`, `Compliance`, and
  `Reporting`.
- `From_Domain_to_Twin`'s Chapter 14 classification tables list neither
  Organization as core, supporting, nor generic — but Chapter 9's own
  flagship bounded-context teaching example (`Organization.Team`/
  `Department`, the "Unit" disambiguation) treats it as real, meaningful
  domain language, the opposite signal from Security's explicit "generic,
  no aggregate design anywhere" classification.
- The atlas's own C.3.7.4 ("Incoming passports from later sectors") names,
  table by table, exactly what `ReactorFleet`, `Maintenance`,
  `RadiationMonitoring`, `EmergencyPreparedness`, `Compliance`, and
  `Reporting` will reference — the strongest per-table signal available for
  any sector's scope decision so far, used directly to draw the built/not
  -built line.
- The atlas's own three "useful verification queries" (C.3.8) — site/plant
  hierarchy, login-account-to-person/department/team resolution, latest
  staffing gaps — named the Application layer's real operations directly.

Full reasoning for what was built vs. deliberately not built, and the
persistence decision, is in ADR-017; not repeated here.

## Domain layer — twenty-five entities, real invariants where the atlas actually describes them

`Nexus1.Organization.Domain`: 8 lookups (`LegalEntityType`, `SiteType`,
`PlantType`, `DepartmentType`, `TeamType`, `PersonType`,
`EmploymentStatus`, `QualificationStatus`) and 17 substantive entities
(`LegalEntity`, `Site`, `Plant`, `Building`, `Department`, `Team`,
`Position`, `Person`, `DepartmentAssignment`, `TeamMembership`,
`Qualification`, `PersonQualification`, `PersonnelRequirement`,
`StaffingScenario`, `StaffingScenarioRequirement`,
`StaffingScenarioResult`, `StaffingScenarioGap`), each with a `Create`
factory enforcing the atlas's real `CHECK` constraints. Real behavior
beyond validation, matching the "flagship bounded-context, not boring
plumbing" treatment ADR-017's research established:

- `DepartmentAssignment.End(DateOnly)` / `TeamMembership.End(DateOnly)` —
  re-validates `EndDate >= StartDate` on close-out, matching the atlas's
  own "time-bounded... avoids overwriting history" prose.
- `PersonQualification.Verify(int verifiedByUserId, DateTime verifiedAtUtc)`
  plus the issued/expiry `CHECK` (`ExpiresAtUtc > IssuedAtUtc`) enforced at
  `Create`.
- `StaffingScenarioGap.Create(...)` **computes `GapCount` itself**
  (`RequiredCount > AvailableCount ? RequiredCount - AvailableCount : 0`)
  — verified directly by reading the file, not taken on report: the
  constructor has no `gapCount` parameter, only `Create`'s internal
  computation feeds it. This is the one place in the sector where the
  database's own SQL computed column and the domain's invariant must agree
  by construction, and they do.

Audit columns not modeled in Domain (no attached behavior), same restraint
as CorePlatform/Security. 97 unit tests: creation validation for all 25
entities plus every real behavior (date-range accept/reject on both
assignment types, the qualification expiry check, and `StaffingScenarioGap`
for both the gap-exists and no-gap cases).

## EF Core Infrastructure — own physical database, twenty-five configurations, one reviewed migration

`Nexus1.Organization.Infrastructure`: `OrganizationDbContext` targeting a
**new, genuinely separate physical database (`OrganizationDb`)** — a
data-sensitivity decision (real PII in `Person`: `GivenName`, `FamilyName`,
`WorkEmail`, `WorkPhone`), independent of and re-derived from, not copied
from, ADR-016's Security reasoning. One `IEntityTypeConfiguration` per
entity, matching the atlas's real column types/lengths/named constraints.
Migration inspected directly: 25 `CreateTable` calls, 36 internal foreign
keys all mapped `Restrict` (zero `Cascade` — the atlas specifies no
`ON DELETE` clause anywhere in this sector, same discrepancy-catch already
applied to CorePlatform/Security), 12 `CHECK` constraints, 19 unique
indexes, and `StaffingScenarioGap.GapCount` mapped as a real SQL Server
`PERSISTED` computed column
(`(CASE WHEN [RequiredCount] > [AvailableCount] THEN [RequiredCount] -
[AvailableCount] ELSE 0 END)`) matching the atlas SQL verbatim.

**Cross-database FKs correctly downgraded, confirmed by reading the
migration, not assumed**: no `principalSchema: "CorePlatform"` or
`principalSchema: "Security"` foreign key exists anywhere in the generated
migration. `Person.ApplicationUserId`, `PersonQualification.
VerifiedByUserId`, `StaffingScenario.CreatedByUserId`,
`StaffingScenarioResult.EvaluatedByUserId` (→ `Security.ApplicationUser`
in the atlas DDL) and `LegalEntity.CountryId`, `Site.CountryId`/
`RegionId`/`TimeZoneId` (→ `CorePlatform.Country`/`Region`/`TimeZone` in
the atlas DDL) are all plain passport ints, exactly as ADR-017 required —
the same correction ADR-016 had to make for `UserPreference`, caught again
here before the migration was generated rather than after, because the
atlas's real DDL puts genuine `FOREIGN KEY` constraints on all of them and
would tempt the same mistake twice without an explicit check.

Migration: `20260816144034_InitialOrganizationSchema`, `Organization`
schema, own migration-history table (`__EFMigrationsHistory_Organization`),
generated via
`dotnet ef migrations add InitialOrganizationSchema --project src/Contexts/Organization/Nexus1.Organization.Infrastructure --startup-project src/Contexts/Organization/Nexus1.Organization.Infrastructure --output-dir Persistence/Migrations`
(the explicit `--output-dir` was needed to land the migration under
`Persistence/Migrations/` matching every other context's convention,
rather than EF's project-root default).

## Application layer — the atlas's own three named verification queries plus real per-table behaviors

Six operations, matched to what the atlas actually names as real behavior:

- `GetSitePlantHierarchyQuery` — atlas C.3.8 query 1, verbatim.
- `ResolvePersonOrganizationContextQuery` — atlas C.3.8 query 2, verbatim
  (login account → person → current department → current team).
- `AssignPersonToDepartmentCommand` / `AssignPersonToTeamCommand` —
  `DepartmentAssignment`/`TeamMembership`'s defining behavior.
- `RecordStaffingScenarioResultCommand` — writes one
  `StaffingScenarioResult` plus its `StaffingScenarioGap` rows in a single
  operation.
- `GetLatestStaffingGapsQuery` — atlas C.3.8 query 3, verbatim.

15 component tests against real LocalDB, including a dedicated proof that
`GetLatestStaffingGapsQuery` picks the most recent `StaffingScenarioResult`
by `EvaluatedAtUtc` when multiple results exist for the same scenario
(mirrors the atlas's own correlated-subquery shape), and a round-trip proof
that the database's computed `GapCount` matches the domain's own value.

## Composed into Nexus1.ModularRuntime

`AddOrganizationApplication()`/`AddOrganizationInfrastructure(organizationConnectionString)`
wired into `Program.cs` with a new `OrganizationDb` connection string (own
entry in `appsettings.json`) and an `organization-db` entry in the health
check chain — confirmed directly by reading `Program.cs`, not taken on
report: `grep` shows the connection-string resolution, the ADR-017-
referencing comment, and `.AddCheck<DbContextHealthCheck<OrganizationDbContext>>("organization-db")`,
bringing the total registered health checks to 8.

`Nexus1.ArchitectureTests` needed zero code changes — run standalone,
7/7 passing, confirming `Nexus1.Organization.Domain`/`.Application`/
`.Infrastructure` were classified and enforced automatically and that no
illegal cross-context references exist.

**Real host startup, independently re-verified, not taken from the
implementation pass's own report**: applied the migration
(`dotnet ef database update`) against a real `OrganizationDb`, confirmed
via direct `sqlcmd` query that all 25 `Organization.*` tables and the
`__EFMigrationsHistory_Organization` row exist *before* ever asking the
health check about it (avoiding the exact false-positive trap named
below), then built and ran the actual host
(`Nexus1.ModularRuntime.dll`), confirmed `GET /health/ready` returns
`200 Healthy` with `organization-db` genuinely present among the 8
registered checks.

## Owned

- **A real, pre-existing gap found and fixed during this step's
  verification pass, not part of Organization's own build**: while
  independently re-checking the foundation this sector builds on (prompted
  by a direct question about SSMS's table list), `sqlcmd` against
  `AlarmManagementDb` showed **zero `CorePlatform.*` tables and no
  `__EFMigrationsHistory_CorePlatform` row at all** — the CorePlatform
  migration (`20260816114650_InitialCorePlatformSchema`, present on disk,
  correctly targeting `AlarmManagementDb` in its design-time factory) had
  never actually been applied to the live database on this machine, despite
  the original CorePlatform evidence report's own claim of a verified
  `200 Healthy` host at the time. This was **strictly worse than the
  SecurityDb gap** caught in the previous sector: `DbContextHealthCheck<T>`
  only calls `CanConnectAsync()`, which checks that the *database* is
  reachable, not that *this context's own tables exist inside it* — since
  `AlarmManagementDb` itself was reachable (`AlarmManagement`/`ReactorFleet`
  tables live there), `coreplatform-db` was silently reporting `Healthy`
  the whole time, a false positive rather than the honest 503 the missing
  `SecurityDb` produced. Fixed by running `dotnet ef database update`
  against `Nexus1.CorePlatform.Infrastructure`; reverified with `sqlcmd`
  (11 `CorePlatform.*` tables now present) and a real host restart
  (`/health/ready` still `200 Healthy`, now correctly so). Swept every
  other context database (`RootCauseDb`, `AuditDb`, `ComplianceDb`,
  `ReportingDb`, `SecurityDb`) the same way before treating the foundation
  as solid — CorePlatform was the only casualty; the other five all have
  their expected tables and migration-history rows.
- Two genuine atlas discrepancies, confirmed directly against the DDL
  text: `StaffingScenario` has `CreatedAtUtc`/`ModifiedAtUtc`/`IsDeleted`/
  `RowVersion` but no `CreatedBy`/`ModifiedBy` (inconsistent with every
  other structural table in the sector, which has both); `StaffingScenarioRequirement`,
  `StaffingScenarioResult`, and `StaffingScenarioGap` have **no audit
  columns at all**, not even `CreatedAtUtc`. Zero implementation impact —
  audit columns aren't modeled in Domain regardless — but recorded as a
  genuine schema-authoring inconsistency in the source material, not
  silently normalized away.
- Two EF Core SQL Server LINQ-translation limits hit and fixed during
  component testing (harness-only, not product bugs, same category as the
  `.Distinct()` limit Security's evidence report already named): an
  explicit `StringComparer.Ordinal` in an `OrderBy` didn't translate
  (removed, default ordinal comparison is what SQL Server does anyway);
  ordering by a value-converted strongly-typed id's `.Value` didn't
  translate (ordered by the converted property itself instead).
- No `src/` files outside the new `Nexus1.Organization.*` projects and
  `Nexus1.ModularRuntime`'s composition root (csproj, `Program.cs`,
  `appsettings.json`) were touched — confirmed via `git status`.
- `OrganizationDb` was left in place after evidence capture — harmless
  local dev state, same reasoning as every prior step. Two orphaned
  `SecurityComponentTests_*` throwaway databases from an interrupted test
  run were also noticed during the database sweep above; left in place as
  harmless residue, not cleaned up as part of this step's scope.

## Scope explicitly not covered by this step

Per ADR-017, twelve of the atlas's thirty-seven Organization tables remain
unbuilt, in five named groups: `PersonContact`/`PersonAddress` (no
consumer anywhere in C.3.7.4's forward-reference list; `PersonAddress` in
particular is the sector's most personally sensitive table with zero
named consumer); `Employment`/`ContractorEngagement` (the atlas's own
"honest boundary" explicitly disclaims payroll/HR data, and no later
sector's FK touches either table); `ShiftPattern`/`Shift`/
`ShiftAssignment`/`OnCallRoster` (real, well-specified invariants exist,
but no C.3.7.4 incoming passport touches the roster layer);
`Certification`/`PersonCertification` (structurally identical to
`Qualification`/`PersonQualification`, but the atlas gives `Qualification`
the actual through-line into staffing, making `Certification` a more
natural pickup when `Compliance` is built); plus the `ShiftType`/
`ContactMethodType` lookups that back only the excluded groups. None are
silently dropped — each is named in ADR-017 with the specific reason it
was deferred.

The ADR-004 `SiteId`/`LineId` reversal for `AlarmFloodDetectedV1` is now
technically possible (`Organization.Site`/`Plant` exist) but was
explicitly **not** performed here, per instruction — recorded as an open
door in ADR-017, left closed. `ReactorFleet.Unit.PlantId →
Organization.Plant` (C.3.7.4) is likewise not wired in this step.

This closes Organization, sector 3 of 11 in Phase 2, and the CorePlatform
migration gap discovered during its verification. Instrumentation is next
per CLAUDE.md §9's ordering — awaiting the next checkpoint instruction.
