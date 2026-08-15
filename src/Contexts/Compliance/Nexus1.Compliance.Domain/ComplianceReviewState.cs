namespace Nexus1.Compliance.Domain;

/// <summary>
/// Only Pending is assigned by anything built so far — review assignment,
/// findings, and decision (ch.34, 34-AL) are Compliance's own reserved
/// future authority, not part of this step's scope (ADR-011). Modeled as an
/// enum now, not a bare "Pending" constant, because the book is explicit
/// that more states are coming; this avoids a breaking rename later.
/// </summary>
public enum ComplianceReviewState
{
    Pending,
}
