# Evidence: hardening item 2 — CorePlatform's missing `EngineeringUnit.QuantityType` CHECK constraint

## Investigation

`EngineeringUnit.cs`'s own doc comment claims a DB-level CHECK constraint
from the Schema Atlas that the migration never implemented (named as a
tracked, not-yet-fixed gap in the CorePlatform slice's own evidence report,
after the `QuantityType` data-mismatch investigation).

Read the actual atlas text (`From_Schema_to_System`, section C.1.4.8)
directly rather than assumed:

```sql
CONSTRAINT CK_CorePlatform_EngineeringUnit_QuantityType
       CHECK (QuantityType IN (
              N'Power', N'ThermalPower', N'Temperature', N'Pressure', N'Reactivity',
              N'Flow', N'Frequency', N'Percentage', N'RadiationDoseRate',
              N'RadiationDose', N'CountRate', N'Mass', N'Time', N'Other'))
```

**This list is character-for-character identical, in the same order, to
`EngineeringQuantityType`'s own C# enum member names.** The existing EF
mapping (`HasConversion<string>()`) already serializes/deserializes using
exact enum-member spelling — so the atlas's constraint was never in
conflict with the current mapping; its absence was a pure doc-vs-
implementation gap, not a design tension to resolve.

**Data safety, checked before touching anything:** only two
`CorePlatform.EngineeringUnit` rows exist, and both were already corrected
in the earlier CorePlatform slice's own investigation
(`RadiationDoseRate`, `Percentage`) — both are in the atlas's valid list.
No other row exists that could violate the constraint.

## Fix applied

Added the constraint to `EngineeringUnitConfiguration.cs`, verbatim
against the atlas text, with a doc-comment note explaining why it was
always safe to add:

```csharp
builder.ToTable("EngineeringUnit", "CorePlatform", t => t.HasCheckConstraint(
    "CK_CorePlatform_EngineeringUnit_QuantityType",
    "[QuantityType] IN (N'Power', N'ThermalPower', N'Temperature', N'Pressure', N'Reactivity', " +
    "N'Flow', N'Frequency', N'Percentage', N'RadiationDoseRate', N'RadiationDose', N'CountRate', " +
    "N'Mass', N'Time', N'Other')"));
```

Generated migration `20260823152434_AddEngineeringUnitQuantityTypeCheckConstraint`
via `dotnet ef migrations add`, applied to LocalDB via
`dotnet ef database update` — a single additive `AddCheckConstraint` call,
no other schema change.

## Verification

```sql
SELECT name, definition FROM sys.check_constraints WHERE name = 'CK_CorePlatform_EngineeringUnit_QuantityType';
```
Confirmed live, matching the atlas's fourteen-value list exactly.

Tested enforcement directly (rolled back, no data touched):
```sql
BEGIN TRAN;
INSERT INTO CorePlatform.EngineeringUnit (EngineeringUnitId,Symbol,Name,QuantityType,IsDimensionless,IsActive,DisplayOrder,CreatedAtUtc)
VALUES (999,'XX','Bad Unit','BOGUS',0,1,99,SYSUTCDATETIME());
ROLLBACK;
```
```
Msg 547 ... The INSERT statement conflicted with the CHECK constraint
"CK_CorePlatform_EngineeringUnit_QuantityType" ...
```
Confirmed rejecting an invalid value live, not just by reading the
migration file.

Both existing real rows re-confirmed intact and valid after the migration:

| EngineeringUnitId | QuantityType |
|---|---|
| 1 | RadiationDoseRate |
| 2 | Percentage |

## Build and test suite

```
dotnet build Nexus1.Runtime.sln → 0 Warning(s), 0 Error(s)
dotnet test Nexus1.Runtime.sln  → 37/37 assemblies green, 869/869 total, 0 failed
```

Unchanged baseline — no regression from the constraint addition.

## Summary

The doc-vs-implementation gap named in the CorePlatform slice is now
closed: the constraint the atlas always specified is live in the real
database, matches the existing C# enum exactly (no application-layer
change needed), and was verified — not assumed — to reject invalid data
and leave the two corrected real rows untouched.
