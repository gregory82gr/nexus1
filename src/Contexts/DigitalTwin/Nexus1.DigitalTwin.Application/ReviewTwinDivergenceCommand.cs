using Nexus1.BuildingBlocks.Application;

namespace Nexus1.DigitalTwin.Application;

/// <summary>TwinDivergenceReview's defining behavior (ADR-020) — closes the review loop C.6.1's design choice explicitly asks for.</summary>
public sealed record ReviewTwinDivergenceCommand(
    long TwinDivergenceId, int DivergenceStatusId, DateTime ReviewedAtUtc, int? ReviewedByUserId = null,
    string? ReviewNote = null, string? CorrectiveAction = null)
    : ICommand<long>;
