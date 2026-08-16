using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

public class TwinRuntimeSessionTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime StartedAtUtc = new(2026, 8, 16, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_opens_the_session_with_no_end_date()
    {
        var session = TwinRuntimeSession.Create(
            new TwinRuntimeSessionId(1), new TwinModelVersionId(1), "SESSION-1", "SHADOW", StartedAtUtc, NowUtc);

        Assert.Null(session.EndedAtUtc);
        Assert.True(session.IsReadOnly);
    }

    [Fact]
    public void End_with_valid_end_date_closes_the_session()
    {
        var session = TwinRuntimeSession.Create(
            new TwinRuntimeSessionId(1), new TwinModelVersionId(1), "SESSION-1", "SHADOW", StartedAtUtc, NowUtc);

        session.End(StartedAtUtc.AddHours(2));

        Assert.Equal(StartedAtUtc.AddHours(2), session.EndedAtUtc);
    }

    [Fact]
    public void End_with_end_date_equal_to_start_date_throws()
    {
        var session = TwinRuntimeSession.Create(
            new TwinRuntimeSessionId(1), new TwinModelVersionId(1), "SESSION-1", "SHADOW", StartedAtUtc, NowUtc);

        Assert.Throws<ArgumentException>(() => session.End(StartedAtUtc));
    }

    [Fact]
    public void End_with_end_date_before_start_date_throws()
    {
        var session = TwinRuntimeSession.Create(
            new TwinRuntimeSessionId(1), new TwinModelVersionId(1), "SESSION-1", "SHADOW", StartedAtUtc, NowUtc);

        Assert.Throws<ArgumentException>(() => session.End(StartedAtUtc.AddHours(-1)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_session_code_throws(string sessionCode)
    {
        Assert.Throws<ArgumentException>(() => TwinRuntimeSession.Create(
            new TwinRuntimeSessionId(1), new TwinModelVersionId(1), sessionCode, "SHADOW", StartedAtUtc, NowUtc));
    }
}
