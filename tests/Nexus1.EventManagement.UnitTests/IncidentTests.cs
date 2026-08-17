using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.UnitTests;

public class IncidentTests
{
    private static readonly DateTime OpenedAtUtc = new(2026, 8, 17, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var incident = Incident.Create(
            new IncidentId(1), operationalEventId: 100L, new IncidentTypeId(1), new IncidentStatusId(1),
            "INC-2026-0007", OpenedAtUtc);

        Assert.Equal(new OperationalEventId(100L), incident.OperationalEventId);
        Assert.Equal("INC-2026-0007", incident.IncidentNumber);
        Assert.Null(incident.ClosedAtUtc);
        Assert.Null(incident.LeadInvestigatorUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_incident_number_throws(string incidentNumber)
    {
        Assert.Throws<ArgumentException>(() => Incident.Create(
            new IncidentId(1), 100L, new IncidentTypeId(1), new IncidentStatusId(1), incidentNumber, OpenedAtUtc));
    }

    [Fact]
    public void Create_with_investigation_summary_and_lead_investigator_sets_them()
    {
        var incident = Incident.Create(
            new IncidentId(1), 100L, new IncidentTypeId(1), new IncidentStatusId(1), "INC-2026-0007", OpenedAtUtc,
            investigationSummary: "Initial triage complete", leadInvestigatorUserId: 21);

        Assert.Equal("Initial triage complete", incident.InvestigationSummary);
        Assert.Equal(21, incident.LeadInvestigatorUserId);
    }
}
