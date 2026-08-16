using System.Diagnostics;
using Nexus1.BuildingBlocks.Observability;

namespace Nexus1.BuildingBlocks.Observability.UnitTests;

public sealed class SafeTagsTests
{
    [Fact]
    public void ForOwnerOperation_projects_only_the_bounded_fields()
    {
        var messageId = Guid.NewGuid();

        var tags = SafeTags.ForOwnerOperation(messageId, "COMMITTED");

        Assert.Equal(messageId.ToString("D"), tags["nexus1.message.id"]);
        Assert.Equal("COMMITTED", tags["nexus1.outcome.code"]);
        Assert.Equal(2, tags.Count);
    }

    [Fact]
    public void ForOwnerOperation_omits_message_id_when_absent_rather_than_faking_one()
    {
        var tags = SafeTags.ForOwnerOperation(messageId: null, "REJECTED");

        Assert.False(tags.ContainsKey("nexus1.message.id"));
        Assert.Single(tags);
    }

    [Fact]
    public void ForMessagePublish_and_ForMessageProcess_never_include_forbidden_keys()
    {
        var publish = SafeTags.ForMessagePublish(Guid.NewGuid(), "nexus1.root-cause.root-cause-case-opened.v1", "root-cause.root-cause-case-opened.v1");
        var process = SafeTags.ForMessageProcess(Guid.NewGuid(), "nexus1.alarm-management.alarm-flood-detected.v1");

        string[] forbidden = ["trace.id", "traceparent", "authorization", "payload", "exception.message"];
        Assert.All(forbidden, key => Assert.False(publish.ContainsKey(key)));
        Assert.All(forbidden, key => Assert.False(process.ContainsKey(key)));
    }

    [Fact]
    public void SafeError_ignores_a_null_activity()
    {
        SafeError.Record(null, new InvalidOperationException("boom"));
    }

    [Fact]
    public void SafeError_records_only_the_classified_error_type_and_sets_error_status()
    {
        using var source = new ActivitySource("test-source-safe-error");
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);

        using var activity = source.StartActivity("test-op");
        SafeError.Record(activity, new InvalidOperationException("a secret exception message that must never be tagged"));

        Assert.NotNull(activity);
        Assert.Equal(ActivityStatusCode.Error, activity!.Status);
        Assert.Equal("contract_invalid", activity.GetTagItem("error.type"));
        Assert.DoesNotContain(activity.Tags, t => t.Value != null && t.Value.Contains("secret exception message"));
        Assert.DoesNotContain(activity.Tags, t => t.Value != null && t.Value.Contains(nameof(InvalidOperationException)));
    }
}
