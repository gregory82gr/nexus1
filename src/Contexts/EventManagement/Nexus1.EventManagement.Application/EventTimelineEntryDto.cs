namespace Nexus1.EventManagement.Application;

/// <summary>Atlas C.8.5.2 query 2 projection, verbatim: EntryAtUtc, entry-type code, Title, Body.</summary>
public sealed record EventTimelineEntryDto(DateTime EntryAtUtc, string EntryType, string Title, string? Body);
