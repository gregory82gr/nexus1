using System.Data.Common;
using System.Text.Json;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.BuildingBlocks.Observability.UnitTests;

public sealed class ErrorClassifierTests
{
    [Theory]
    [InlineData(typeof(TimeoutException), "timeout")]
    [InlineData(typeof(InvalidOperationException), "contract_invalid")]
    [InlineData(typeof(ArgumentException), "unclassified")]
    public void Classify_maps_known_exception_kinds_to_the_reviewed_vocabulary(Type exceptionType, string expected)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "boom")!;

        Assert.Equal(expected, ErrorClassifier.Classify(exception));
    }

    [Fact]
    public void Classify_maps_OperationCanceledException_to_shutdown_cancelled()
    {
        Assert.Equal("shutdown_cancelled", ErrorClassifier.Classify(new OperationCanceledException()));
    }

    [Fact]
    public void Classify_maps_JsonException_to_contract_invalid()
    {
        Assert.Equal("contract_invalid", ErrorClassifier.Classify(new JsonException("bad json")));
    }

    [Fact]
    public void Classify_maps_DbException_to_dependency_unavailable()
    {
        Assert.Equal("dependency_unavailable", ErrorClassifier.Classify(new FakeDbException()));
    }

    [Fact]
    public void Classify_never_returns_a_value_outside_the_declared_vocabulary()
    {
        var exceptions = new Exception[]
        {
            new TimeoutException(), new InvalidOperationException(), new ArgumentException(),
            new OperationCanceledException(), new JsonException(), new FakeDbException(),
            new NotSupportedException(), new StackOverflowException(),
        };

        Assert.All(exceptions, ex => Assert.Contains(ErrorClassifier.Classify(ex), ErrorClassifier.Vocabulary));
    }

    private sealed class FakeDbException : DbException;
}
