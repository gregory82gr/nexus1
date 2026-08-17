using Nexus1.EmergencyPreparedness.Domain;

namespace Nexus1.EmergencyPreparedness.UnitTests;

public class ExerciseTests
{
    private static readonly DateTime ScheduledStartUtc = new(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ScheduledEndUtc = new(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_leaves_optional_fields_at_their_defaults()
    {
        var exercise = Exercise.Create(
            new ExerciseId(1), "EX-001", "Site-Wide Fire Drill", new ExerciseTypeId(1), new ExerciseStatusId(1),
            siteId: 100, ScheduledStartUtc, ScheduledEndUtc, coordinatorUserId: 501);

        Assert.Equal("EX-001", exercise.Code);
        Assert.Equal("Site-Wide Fire Drill", exercise.Name);
        Assert.Equal(100, exercise.SiteId);
        Assert.Null(exercise.PlantId);
        Assert.Equal(501, exercise.CoordinatorUserId);
        Assert.Null(exercise.ActualStartUtc);
        Assert.Null(exercise.ActualEndUtc);
        Assert.Null(exercise.Summary);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => Exercise.Create(
            new ExerciseId(1), code, "Site-Wide Fire Drill", new ExerciseTypeId(1), new ExerciseStatusId(1),
            siteId: 100, ScheduledStartUtc, ScheduledEndUtc, coordinatorUserId: 501));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => Exercise.Create(
            new ExerciseId(1), "EX-001", name, new ExerciseTypeId(1), new ExerciseStatusId(1),
            siteId: 100, ScheduledStartUtc, ScheduledEndUtc, coordinatorUserId: 501));
    }

    [Fact]
    public void Create_with_passport_only_plant_id_sets_it_with_no_enforced_fk()
    {
        var exercise = Exercise.Create(
            new ExerciseId(1), "EX-001", "Site-Wide Fire Drill", new ExerciseTypeId(1), new ExerciseStatusId(1),
            siteId: 100, ScheduledStartUtc, ScheduledEndUtc, coordinatorUserId: 501, plantId: 42);

        Assert.Equal(42, exercise.PlantId);
    }
}
