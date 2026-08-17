using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.UnitTests;

public class ExerciseObservationTests
{
    private static readonly DateTime ObservedAtUtc = new(2026, 9, 1, 10, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_their_defaults()
    {
        var observation = ExerciseObservation.Create(
            new ExerciseObservationId(1), new ExerciseId(1), new ObservationSeverityId(1), observedByUserId: 501,
            ObservedAtUtc, "Muster count did not match roster within five minutes.");

        Assert.Equal(new ExerciseId(1), observation.ExerciseId);
        Assert.Equal(501, observation.ObservedByUserId);
        Assert.False(observation.CorrectiveActionRequired);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_finding_text_throws(string findingText)
    {
        Assert.Throws<ArgumentException>(() => ExerciseObservation.Create(
            new ExerciseObservationId(1), new ExerciseId(1), new ObservationSeverityId(1), observedByUserId: 501,
            ObservedAtUtc, findingText));
    }

    [Fact]
    public void Create_with_corrective_action_required_true_sets_it()
    {
        var observation = ExerciseObservation.Create(
            new ExerciseObservationId(1), new ExerciseId(1), new ObservationSeverityId(1), observedByUserId: 501,
            ObservedAtUtc, "Muster count did not match roster within five minutes.", correctiveActionRequired: true);

        Assert.True(observation.CorrectiveActionRequired);
    }
}
