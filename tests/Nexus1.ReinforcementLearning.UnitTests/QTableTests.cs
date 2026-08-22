using Nexus1.ReinforcementLearning.Domain;

namespace Nexus1.ReinforcementLearning.UnitTests;

public class QTableTests
{
    private static readonly DateTime NowUtc = new(2026, 8, 22, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_with_valid_fields_succeeds()
    {
        var qTable = QTable.Create(new QTableId(1), new TrainingRunId(1), new StateSpaceId(1), new ActionSpaceId(1), "QT-001", NowUtc, 175);

        Assert.Equal("QT-001", qTable.Code);
        Assert.Equal(175, qTable.EntryCount);
        Assert.False(qTable.IsFinal);
        Assert.Equal(NowUtc, qTable.SnapshotAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_with_blank_code_throws(string code)
    {
        Assert.Throws<ArgumentException>(() => QTable.Create(new QTableId(1), new TrainingRunId(1), new StateSpaceId(1), new ActionSpaceId(1), code, NowUtc, 175));
    }

    [Fact]
    public void Create_with_non_positive_entry_count_throws()
    {
        Assert.Throws<ArgumentException>(() => QTable.Create(new QTableId(1), new TrainingRunId(1), new StateSpaceId(1), new ActionSpaceId(1), "QT-001", NowUtc, 0));
    }

    [Fact]
    public void Create_with_is_final_true_marks_it_final()
    {
        var qTable = QTable.Create(new QTableId(1), new TrainingRunId(1), new StateSpaceId(1), new ActionSpaceId(1), "QT-001", NowUtc, 175, isFinal: true);

        Assert.True(qTable.IsFinal);
    }
}
