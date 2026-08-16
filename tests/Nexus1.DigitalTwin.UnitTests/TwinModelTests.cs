using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

public class TwinModelTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds_and_is_not_deleted()
    {
        var model = TwinModel.Create(
            new TwinModelId(1), unitId: 1, new TwinModelTypeId(1), new TwinModelStatusId(1), new TwinFidelityLevelId(1),
            "NX1-U1-CORE-TWIN", "Unit 1 Core Twin", NowUtc);

        Assert.Equal("NX1-U1-CORE-TWIN", model.Code);
        Assert.False(model.IsDeleted);
        Assert.False(model.IsAuthoritative);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => TwinModel.Create(
            new TwinModelId(1), 1, new TwinModelTypeId(1), new TwinModelStatusId(1), new TwinFidelityLevelId(1),
            code, "Unit 1 Core Twin", NowUtc));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_name_throws(string name)
    {
        Assert.Throws<ArgumentException>(() => TwinModel.Create(
            new TwinModelId(1), 1, new TwinModelTypeId(1), new TwinModelStatusId(1), new TwinFidelityLevelId(1),
            "NX1-U1-CORE-TWIN", name, NowUtc));
    }
}
