using Nexus1.DigitalTwin.Domain;

namespace Nexus1.DigitalTwin.UnitTests;

/// <summary>
/// The sector's most distinctive real behavior (ADR-020): DeltaValue is
/// computed by the domain factory itself, never accepted as a
/// caller-supplied value, mirroring the book's own worked TwinDivergence
/// example (Difference => MeasuredValue - ModelValue) even though the
/// atlas's own DDL does not mark DeltaValue as a SQL computed column.
/// </summary>
public class TwinDivergenceTests
{
    private static readonly DateTime DetectedAtUtc = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_computes_delta_value_as_measured_minus_modeled()
    {
        var divergence = TwinDivergence.Create(
            new TwinDivergenceId(1), new TwinSnapshotId(1), signalId: 1, new DivergenceSeverityId(1), new DivergenceStatusId(1),
            DetectedAtUtc, modeledValue: 100.0, measuredValue: 107.5);

        Assert.Equal(7.5, divergence.DeltaValue, precision: 10);
    }

    [Fact]
    public void Create_with_measured_below_modeled_computes_negative_delta()
    {
        var divergence = TwinDivergence.Create(
            new TwinDivergenceId(1), new TwinSnapshotId(1), signalId: 1, new DivergenceSeverityId(1), new DivergenceStatusId(1),
            DetectedAtUtc, modeledValue: 100.0, measuredValue: 92.0);

        Assert.Equal(-8.0, divergence.DeltaValue, precision: 10);
    }

    [Fact]
    public void Create_with_equal_values_computes_zero_delta()
    {
        var divergence = TwinDivergence.Create(
            new TwinDivergenceId(1), new TwinSnapshotId(1), signalId: 1, new DivergenceSeverityId(1), new DivergenceStatusId(1),
            DetectedAtUtc, modeledValue: 50.0, measuredValue: 50.0);

        Assert.Equal(0.0, divergence.DeltaValue, precision: 10);
    }

    /// <summary>
    /// There is no overload of Create that accepts a DeltaValue parameter at
    /// all — this test documents that constraint via reflection, so a future
    /// change that accidentally adds a caller-supplied DeltaValue parameter
    /// is caught here rather than only by code review.
    /// </summary>
    [Fact]
    public void Create_has_no_caller_supplied_delta_value_parameter()
    {
        var createMethod = typeof(TwinDivergence).GetMethod("Create");
        Assert.NotNull(createMethod);
        Assert.DoesNotContain(createMethod!.GetParameters(), p => p.Name == "deltaValue");
    }
}
