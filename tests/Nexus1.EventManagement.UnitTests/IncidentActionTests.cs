using Nexus1.EventManagement.Domain;

namespace Nexus1.EventManagement.UnitTests;

public class IncidentActionTests
{
    private static readonly DateTime DueAtUtc = new(2026, 8, 24, 17, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CompletedAtUtc = new(2026, 8, 20, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime VerifiedAtUtc = new(2026, 8, 21, 9, 0, 0, DateTimeKind.Utc);

    private static IncidentAction NewAction() => IncidentAction.Create(
        new IncidentActionId(1), incidentId: 5L, new IncidentActionTypeId(1), new IncidentActionStatusId(1),
        "Replace corroded valve", description: "Root cause: corrosion", dueAtUtc: DueAtUtc);

    [Fact]
    public void Create_with_valid_fields_starts_with_no_completed_or_verified_timestamps()
    {
        var action = NewAction();

        Assert.Equal(new IncidentId(5L), action.IncidentId);
        Assert.Equal("Replace corroded valve", action.Title);
        Assert.Equal(DueAtUtc, action.DueAtUtc);
        Assert.Null(action.CompletedAtUtc);
        Assert.Null(action.VerifiedAtUtc);
        Assert.Null(action.VerifiedByUserId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_title_throws(string title)
    {
        Assert.Throws<ArgumentException>(() => IncidentAction.Create(
            new IncidentActionId(1), 5L, new IncidentActionTypeId(1), new IncidentActionStatusId(1), title));
    }

    [Fact]
    public void Complete_records_the_completion_timestamp()
    {
        var action = NewAction();

        action.Complete(CompletedAtUtc);

        Assert.Equal(CompletedAtUtc, action.CompletedAtUtc);
    }

    [Fact]
    public void Complete_called_twice_throws()
    {
        var action = NewAction();
        action.Complete(CompletedAtUtc);

        Assert.Throws<InvalidOperationException>(() => action.Complete(CompletedAtUtc));
    }

    [Fact]
    public void Verify_before_complete_throws()
    {
        var action = NewAction();

        Assert.Throws<InvalidOperationException>(() => action.Verify(VerifiedAtUtc, verifiedByUserId: 9));
    }

    [Fact]
    public void Complete_then_Verify_records_both_timestamps_and_the_verifying_user()
    {
        var action = NewAction();

        action.Complete(CompletedAtUtc);
        action.Verify(VerifiedAtUtc, verifiedByUserId: 9);

        Assert.Equal(CompletedAtUtc, action.CompletedAtUtc);
        Assert.Equal(VerifiedAtUtc, action.VerifiedAtUtc);
        Assert.Equal(9, action.VerifiedByUserId);
    }

    [Fact]
    public void Verify_with_no_verifying_user_leaves_it_null()
    {
        var action = NewAction();
        action.Complete(CompletedAtUtc);

        action.Verify(VerifiedAtUtc, verifiedByUserId: null);

        Assert.Equal(VerifiedAtUtc, action.VerifiedAtUtc);
        Assert.Null(action.VerifiedByUserId);
    }

    [Fact]
    public void Verify_called_twice_throws()
    {
        var action = NewAction();
        action.Complete(CompletedAtUtc);
        action.Verify(VerifiedAtUtc, verifiedByUserId: 9);

        Assert.Throws<InvalidOperationException>(() => action.Verify(VerifiedAtUtc, verifiedByUserId: 9));
    }
}
