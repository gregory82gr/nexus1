# ADR-017: Organization (Phase 2, sector 3) — scope, domain shape, and persistence

## Status

Accepted.

## Context

Phase 2's third sector. Verified directly against the source material before
writing any code:

- `From_Schema_to_System` Appendix C.3: **thirty-seven tables** (ten lookup,
  twenty-seven substantive) — legal/physical structure (`LegalEntity`,
  `Site`, `Plant`, `Building`), department/team structure (`Department`,
  `Team`, `Position`), personnel (`Person`, `PersonContact`,
  `PersonAddress`, `Employment`, `ContractorEngagement`), assignment
  (`DepartmentAssignment`, `TeamMembership`), shift/on-call
  (`ShiftPattern`, `Shift`, `ShiftAssignment`, `OnCallRoster`), competence
  (`Qualification`, `PersonQualification`, `Certification`,
  `PersonCertification`), and staffing/stress-test (`PersonnelRequirement`,
  `StaffingScenario`, `StaffingScenarioRequirement`,
  `StaffingScenarioResult`, `StaffingScenarioGap`). C.3.1 names Organization
  as *"one of the main passport providers for later sectors: ReactorFleet,
  Maintenance, RadiationMonitoring, EmergencyPreparedness, Compliance and
  Reporting."*
- `From_Domain_to_Twin`'s Chapter 14 ("Core, Supporting, and Generic
  Domains") does **not list Organization in any of its three
  classification tables** — not core, not the named supporting domains
  (Instrumentation, Alarm Management, Maintenance, Robotics, Emergency
  Preparedness), not the named generic domains (Security, Audit,
  Reporting, Core Platform). This is different from Security, which the
  book explicitly labeled generic with no aggregate design anywhere. The
  chapter appears to predate Organization's addition to project scope
  (Compliance is likewise absent), so it gives no direct classification —
  but the book is not silent on Organization elsewhere: Chapter 9's bounded-
  context teaching example is built entirely on `Organization.Team`/
  `Department` (the "Unit" disambiguation: *"Inside an organization chart,
  'unit' may mean a department"*), and Chapter 9's own worked table lists
  `Organization.Unit — A team or department in the organisation model` as
  one of exactly three canonical bounded-context examples in the whole
  book (alongside `ReactorFleet.Unit` and `CorePlatform.EngineeringUnit`).
  A domain the book reaches for as one of its three flagship teaching
  examples is not being treated as boring, pass-through plumbing — this is
  the opposite signal from Security's "no aggregate design exists" finding.
  **Named discrepancy, not acted on**: the book's own example table calls
  the type `Organization.Unit`; the atlas's real schema has no `Unit`
  table at all — the real tables are `Department` and `Team`. The book's
  example is pedagogical shorthand, not a literal schema reference; the
  atlas is the schema authority per CLAUDE.md, so `Department`/`Team` are
  modeled as the atlas actually names them, not renamed to match the book.
- The atlas's own "Design choice" callout (C.3.1): *"`Organization.Plant`
  is not a reactor unit. It is the organizational plant container under a
  physical site. `ReactorFleet.Unit` will later reference
  `Organization.Plant` through `PlantId`, preserving the distinction
  between institution, site, plant and machine."* This is real, described
  domain reasoning — not a generic lookup table — and it directly informs
  the scope decision below.
- The atlas's own C.3.7.4 ("Incoming passports from later sectors") is
  unusually explicit about exactly which Organization tables later
  sectors will reference: `ReactorFleet.Unit.PlantId → Organization.Plant`;
  `Maintenance.WorkOrder.AssignedTeamId/RequestedByPersonId →
  Organization.Team/Person`; `RadiationMonitoring.RadiationZone.BuildingId/
  SiteId → Organization.Building/Site`; `EmergencyPreparedness.
  DrillParticipant.PersonId/TeamId → Organization.Person/Team`;
  `Compliance.AuditScope.LegalEntityId/SiteId/DepartmentId → Organization`
  structure; `Reporting.ReportFilter.SiteId/DepartmentId/TeamId →
  Organization` structure. No incoming passport anywhere touches
  `PersonContact`, `PersonAddress`, `Employment`, `ContractorEngagement`,
  the shift/on-call tables, or `Certification`/`PersonCertification`.
- The atlas's own three "useful verification queries" (C.3.8) name real
  Application-layer operations: (1) plant hierarchy for a site
  (`Site`→`Plant`), (2) resolve a login account to person, department and
  team (`ApplicationUser`→`Person`→`DepartmentAssignment`→`Department`,
  `TeamMembership`→`Team`), (3) staffing gaps for the latest stress-test
  result (`StaffingScenario`→`StaffingScenarioResult`→
  `StaffingScenarioGap`→`Position`).
- The atlas's own closing "Honest boundary" (C.3.9): *"This schema is an
  enterprise demonstrator organization model. It is not a
  human-resources system, payroll system, medical fitness system, or
  official personnel-licensing register. It deliberately avoids highly
  personal data that the NEXUS-1 demonstrator does not need."* The atlas
  itself flags `Employment`/`ContractorEngagement` as the kind of
  HR/payroll-adjacent data this boundary is disclaiming.
- `Organization.Person`'s own DDL comment: *"The person row is
  intentionally separate from the security user. A person may or may not
  have a login account, and a service account may have no person behind
  it."* — the atlas's own reasoning for keeping `Person.ApplicationUserId`
  nullable and separate, matching this project's own person/identity
  separation instinct independently.

Two additional, explicit constraints from the user's own instruction, not
derived from the atlas:

- **ADR-004's deferred `SiteId`/`LineId`** (the `AlarmFloodDetectedV1`
  amendment) is **not** retrofitted by this ADR. Organization's existence
  makes that reversal theoretically possible — see Consequences — but it
  is a separate, deliberate future decision, left closed here.
- Same restraint/trim and persistence-decision discipline as CorePlatform
  (ADR-015) and Security (ADR-016), reasoned per this sector's own facts,
  not copied from either precedent.

## Decision

### Scope: twenty-five of thirty-seven tables — the passport-provider spine plus the atlas's own three named operations

**Built** (8 lookups + 17 substantive):

- Lookups: `LegalEntityType`, `SiteType`, `PlantType`, `DepartmentType`,
  `TeamType`, `PersonType`, `EmploymentStatus`, `QualificationStatus`.
- Structure: `LegalEntity`, `Site`, `Plant`, `Building` — the passport
  spine C.3.7.4 names verbatim as what `ReactorFleet`, `RadiationMonitoring`,
  `Compliance`, and `Reporting` will reference.
- Department/team: `Department`, `Team`, `Position` — the org tree;
  `Team`/`Department` are also the book's own flagship bounded-context
  example (see Context), and C.3.7.4 names `Team` as a direct
  `Maintenance`/`EmergencyPreparedness` passport target.
- Personnel: `Person` — the login-account/person split the atlas itself
  calls out as intentional; C.3.7.4 names `Person` as a direct
  `Maintenance`/`EmergencyPreparedness` passport target and C.3.8's query 2
  is built entirely around it.
- Assignment: `DepartmentAssignment`, `TeamMembership` — time-bounded, real
  `EndDate >= StartDate` invariants, and exactly the join path C.3.8's
  query 2 names.
- Competence: `Qualification`, `PersonQualification` — real expiry
  (`ExpiresAtUtc > IssuedAtUtc`) and status invariants, and the table that
  `PersonnelRequirement`/`StaffingScenarioRequirement` actually reference.
- Staffing: `PersonnelRequirement`, `StaffingScenario`,
  `StaffingScenarioRequirement`, `StaffingScenarioResult`,
  `StaffingScenarioGap` — C.3.8's query 3 names this exact join path
  verbatim, and `StaffingScenarioGap.GapCount` is a real computed
  invariant (`RequiredCount > AvailableCount ? RequiredCount -
  AvailableCount : 0`) worth modeling as domain behavior, not a passthrough
  column.

**Not built, with reasoning per group** (2 lookups + 10 substantive):

- **`ShiftType`, `ContactMethodType`** lookups — each backs only an
  excluded group below; no other consumer.
- **`PersonContact`, `PersonAddress`** — pure storage with no described
  behavior beyond a `CHECK` constraint on `AddressType`, and no C.3.7.4
  incoming passport references either table. `PersonAddress` in particular
  is the single most personally sensitive table in the sector (home/
  mailing addresses) with zero named consumer — building it now would be
  exactly the kind of data collection C.3.9's own "honest boundary"
  disclaims, for a table nothing in this project reads.
- **`Employment`, `ContractorEngagement`** — genuinely HR/payroll-adjacent
  (`EmployeeNumber`, `FtePercent`, `ManagerPersonId`, contract references),
  which C.3.9 explicitly names as the kind of thing this schema is *not*
  ("not a human-resources system, payroll system... or official
  personnel-licensing register"). No C.3.7.4 incoming passport references
  either table — `DepartmentAssignment`/`TeamMembership` already carry the
  "who belongs where, right now" answer every named later-sector consumer
  actually needs.
- **`ShiftPattern`, `Shift`, `ShiftAssignment`, `OnCallRoster`** — real,
  well-specified invariants exist here (time-range checks, a confirmation
  workflow), but no C.3.7.4 incoming passport touches any of the four;
  `EmergencyPreparedness.DrillParticipant` references `Person`/`Team`
  directly, not the roster layer. Rostering is a genuine, coherent future
  slice, not speculative plumbing — but nothing in this project's
  confirmed dependency graph needs it yet.
- **`Certification`, `PersonCertification`** — structurally identical to
  `Qualification`/`PersonCertification` (same expiry/verification shape),
  but the atlas gives `Qualification` — not `Certification` — the actual
  through-line into `PersonnelRequirement`/`StaffingScenarioRequirement`.
  `Certification`'s own column (`RegulatoryBody`) makes it the more
  natural pickup when the `Compliance` sector is built and can reference
  it meaningfully, not a table this sector's own named operations need.

This mirrors ADR-016's discipline (reason per group from the atlas's own
signals, not a blanket percentage) but lands at a materially larger
fraction (25/37 ≈ 68%, vs. Security's 9/29 ≈ 31%) for a different reason:
C.3.7.4 gives an unusually explicit, table-by-table list of exactly what
later sectors will reference, which is a stronger signal than existed for
Security (whose exclusions rested on "no HTTP surface exists yet," not
"no later sector's own FK list names this table").

### Domain shape: real invariants where the atlas actually describes them, not boring pass-through

Unlike Security (no aggregate design anywhere in the book, explicit
"generic" classification), Organization gets real behavior modeled,
matching the book's own treatment of it as a flagship bounded-context
example rather than plumbing:

- `LegalEntity`/`Site`/`Plant`/`Building` — structural anchors, `Create`
  factories enforcing atlas `CHECK`s (`Site.Latitude`/`Longitude` range,
  `Building.FloorCount >= 0`).
- `Department`/`Team`/`Position` — org-tree `Create` factories; `Team`
  keeps `IsShiftTeam`/`IsEmergencyTeam` as real flags (not modeled behavior
  beyond storage — no shift/emergency workflow exists in this scope to act
  on them, matching the "flag it, don't wire it" restraint already used
  for `ApplicationUser.IsServiceAccount` in ADR-016).
- `Person` — `Create` factory; `ApplicationUserId` modeled as an optional
  passport int (see Persistence below for why it cannot be a real FK).
- `DepartmentAssignment`/`TeamMembership` — real behavior:
  `EndDate >= StartDate` enforced at creation, plus an `End(DateOnly)`
  method enforcing the same invariant on close-out, matching the atlas's
  own prose ("time-bounded... avoids overwriting history").
- `Qualification`/`PersonQualification` — real behavior:
  `PersonQualification.Verify(...)`/expiry check
  (`ExpiresAtUtc > IssuedAtUtc` enforced at creation), matching the
  atlas's own "dated, verifiable rows" description.
- `PersonnelRequirement` — `Create` factory with `MinRequiredCount >= 0`
  and validity-window checks.
- `StaffingScenario`/`StaffingScenarioRequirement`/
  `StaffingScenarioResult`/`StaffingScenarioGap` — the sector's most
  distinctive real behavior: `StaffingScenarioGap` computes its own
  `GapCount` in the domain constructor (`RequiredCount > AvailableCount ?
  RequiredCount - AvailableCount : 0`), mirroring the atlas's own SQL
  computed-column definition exactly rather than trusting a caller-supplied
  value — the one place in this sector where the database's computed
  column and the domain's own invariant must agree by construction.

Same audit-column restraint as ADR-015/ADR-016: `CreatedBy`/
`ModifiedAtUtc`/`ModifiedBy`/`IsDeleted`/`RowVersion` are not modeled in
Domain (no attached behavior), one `CreatedAtUtc` kept per entity.
`DepartmentAssignment`/`TeamMembership`/`PersonQualification`'s
`VerifiedByUserId`/`ConfirmedByUserId`-style columns are out of this
built scope entirely (they live on `ShiftAssignment`, which is excluded)
except `PersonQualification.VerifiedByUserId`, kept as a passport-only
nullable int (see Persistence).

**ADR-004 reversal condition — recorded, not acted on.** `Organization.Site`
and `Organization.Plant` now exist as real, buildable concepts. ADR-004's
`AlarmFloodDetectedV1` amendment deliberately omitted `SiteId`/`LineId`
from its payload because Organization did not exist yet at that time. That
gap is now technically closable — a future amendment could add `SiteId`/
`PlantId` to `AlarmFloodDetectedV1`, resolved through
`ReactorFleet.Unit.PlantId → Organization.Plant` (itself not yet built,
since `ReactorFleet` predates this sector). **This ADR does not perform
that amendment.** Per explicit instruction, the door is noted as open; it
stays closed until a future session is explicitly asked to walk through
it.

### Application layer: the atlas's own three named operations

- `GetSitePlantHierarchyQuery` — atlas C.3.8 query 1, verbatim.
- `ResolvePersonOrganizationContextQuery` — atlas C.3.8 query 2, verbatim
  (login account → person → current department → current team).
- `AssignPersonToDepartmentCommand` / `AssignPersonToTeamCommand` —
  `DepartmentAssignment`/`TeamMembership`'s defining behavior.
- `RecordStaffingScenarioResultCommand` — writes a
  `StaffingScenarioResult` plus its `StaffingScenarioGap` rows in one
  operation, matching how a stress-test evaluation actually produces both
  together.
- `GetLatestStaffingGapsQuery` — atlas C.3.8 query 3, verbatim.

### Persistence: own physical database (`OrganizationDb`) — a data-sensitivity call, re-derived, not copied

Re-checking both ADR-015's (deployment-topology) and ADR-016's
(data-sensitivity) reasoning against this sector's own facts, not
transferring either mechanically:

- **Deployment topology** (ADR-015's axis): no independent deployment
  exists for Organization any more than for CorePlatform or Security — this
  alone would argue for sharing `AlarmManagementDb`.
- **Data sensitivity** (ADR-016's axis): even in this trimmed 25-table
  scope, `Person` carries real names (`GivenName`, `FamilyName`,
  `DisplayName`), `WorkEmail`, `WorkPhone` — genuine PII, distinct in kind
  from Security's credential-adjacent columns but the same *class* of
  reason ADR-016 used to isolate: sensitivity that exists independent of
  deployment topology. Because this axis points toward isolation here too
  (for a different reason than Security), Organization gets its **own
  physical database (`OrganizationDb`)**, joining `RootCauseDb`/`AuditDb`/
  `ComplianceDb`/`ReportingDb`/`SecurityDb` rather than
  `AlarmManagementDb`'s shared-foundation group.

**Cross-database FK consequence, caught before the migration was written,
not after** — the same check ADR-016 had to make, reapplied here because
the atlas's real DDL genuinely tempts the same mistake twice:

- The atlas's own DDL puts **real SQL `FOREIGN KEY`** constraints from
  `Organization.Person.ApplicationUserId`, `PersonQualification.
  VerifiedByUserId`, `StaffingScenario.CreatedByUserId`, and
  `StaffingScenarioResult.EvaluatedByUserId` to `Security.ApplicationUser`
  — written on the atlas's own assumption of one shared database. Since
  `OrganizationDb` and `SecurityDb` are two different physical databases
  (this section's own decision, following ADR-016's), none of these can be
  real FKs. All four are modeled as plain passport ints with no enforced
  constraint.
- Symmetrically, `LegalEntity.CountryId` and `Site.CountryId`/`RegionId`/
  `TimeZoneId` have real FKs to `CorePlatform.Country`/`Region`/`TimeZone`
  in the atlas DDL. `CorePlatform` lives in `AlarmManagementDb`
  (ADR-015); `OrganizationDb` is a third, different physical database from
  both `AlarmManagementDb` and `SecurityDb`. ADR-015's real-FK exception
  does not extend here either — these become passport-only ints too,
  unlike `ReactorFleet`'s (which shares `AlarmManagementDb` with
  `CorePlatform` and keeps the real FK exception).

Own `OrganizationDbContext`, own `Organization` SQL schema, own
migration-history table (`__EFMigrationsHistory_Organization`), same
caller-supplied-ID convention as every other context.

## Consequences

- `Nexus1.Organization.Domain`, `Nexus1.Organization.Application`,
  `Nexus1.Organization.Infrastructure` — composed into
  `Nexus1.ModularRuntime` only (no independent host), against a new,
  sixth physical database (`OrganizationDb`).
- Twelve tables remain unbuilt, named explicitly above in two lookup and
  five substantive groups — not a blanket cut.
- `Person.ApplicationUserId`, `PersonQualification.VerifiedByUserId`,
  `StaffingScenario.CreatedByUserId`, `StaffingScenarioResult.
  EvaluatedByUserId` reference `Security.ApplicationUser` by passport int
  only, matching ADR-016's own finding once two sectors live in different
  physical databases.
- `LegalEntity.CountryId`, `Site.CountryId`/`RegionId`/`TimeZoneId`
  reference `CorePlatform.Country`/`Region`/`TimeZone` by passport int
  only — a genuine downgrade from the atlas's own real-FK DDL, same
  reasoning as the Security-side correction above, caught before the
  migration was generated.
- The ADR-004 `SiteId`/`LineId` reversal is now technically possible
  (`Organization.Site`/`Plant` exist) but explicitly not performed here —
  flagged for a future, deliberate decision only.
- `ReactorFleet.Unit.PlantId → Organization.Plant` (C.3.7.4) is not wired
  in this step either — `ReactorFleet` was built in Phase 1 before
  Organization existed; retrofitting `ReactorFleet.Unit` with a real
  `PlantId` reference is the same category of deliberate-future-decision
  as the ADR-004 reversal, not performed here without being asked.

## Rejected alternatives

- **Reuse ADR-015's shared-`AlarmManagementDb` precedent because
  Organization, like CorePlatform, has no independent deployment.**
  Rejected: deployment topology is only one of the two axes; data
  sensitivity (real PII in `Person`) independently argues for isolation
  here, the same kind of divergence ADR-016 already found for a different
  reason. Copying the topology-only conclusion without re-checking
  sensitivity would repeat the exact mistake this project's verification
  convention warns against.
- **Build all thirty-seven tables for full atlas fidelity.** Rejected:
  twelve tables have no named consumer anywhere in C.3.7.4's own
  forward-reference list, and two of those twelve (`Employment`,
  `ContractorEngagement`) are the specific kind of data the atlas's own
  C.3.9 boundary disclaims.
- **Retrofit `AlarmFloodDetectedV1`/`ReactorFleet.Unit` now that
  `Organization.Site`/`Plant` exist.** Rejected per explicit instruction:
  the dependency being technically satisfiable is not itself a trigger:
  recorded as an open door, left closed.
- **Rename `Department`/`Team` to match the book's `Organization.Unit`
  teaching example.** Rejected: the atlas is the schema authority per
  CLAUDE.md; the book's `Unit` is illustrative shorthand for "a team or
  department," not a literal table name — building a table that doesn't
  exist in the atlas to satisfy a teaching analogy would invert the
  authority order.

## Evidence required

- Domain unit tests, no persistence, for all seventeen substantive
  entities' creation validation and the real behaviors (date-range
  enforcement on `DepartmentAssignment`/`TeamMembership`, expiry check on
  `PersonQualification`, `StaffingScenarioGap`'s computed `GapCount`).
- `dotnet ef migrations add` producing a readable migration under
  `Nexus1.Organization.Infrastructure`, targeting the `Organization` SQL
  schema against a new `OrganizationDb` physical database, independent
  migration history.
- Component tests against real LocalDB (`OrganizationDb`) for the six
  Application-layer operations, including both atlas verification queries
  proven against real seeded data (site/plant hierarchy, person→
  department→team resolution, latest staffing gaps).
- `Nexus1.ArchitectureTests` passing with `Nexus1.Organization.*` composed
  into `Nexus1.ModularRuntime` and correctly classified.
- Real host startup with an `organization-db` health check reaching the
  new physical database.
