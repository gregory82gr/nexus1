using System.Collections.Frozen;
using System.Data.Common;
using System.Text.Json;

namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// Bounded, reviewed error-type vocabulary (ch.52 52-Q's ERROR MAP), shared
/// by both signals rather than duplicated: <see cref="SafeError"/> uses it
/// for the trace `error.type` tag, and metrics use it for classified
/// failure counting. A raw <c>exception.GetType().Name</c> is technically
/// unbounded across every exception type any dependency could ever throw —
/// fine as a trace attribute (traces are not aggregated into low-cardinality
/// series), not fine as a metric label, so this classifier maps every
/// exception down to one of five reviewed buckets instead.
/// </summary>
public static class ErrorClassifier
{
    private static readonly FrozenSet<string> BrokerExceptionTypeNames = new[]
    {
        "BrokerUnreachableException", "OperationInterruptedException", "AlreadyClosedException", "ConnectFailureException",
    }.ToFrozenSet();

    public static readonly FrozenSet<string> Vocabulary = new[]
    {
        "timeout", "dependency_unavailable", "contract_invalid", "shutdown_cancelled", "unclassified",
    }.ToFrozenSet();

    public static string Classify(Exception exception) => exception switch
    {
        OperationCanceledException => "shutdown_cancelled",
        TimeoutException => "timeout",
        DbException => "dependency_unavailable",
        JsonException => "contract_invalid",
        InvalidOperationException => "contract_invalid",
        _ when BrokerExceptionTypeNames.Contains(exception.GetType().Name) => "dependency_unavailable",
        _ => "unclassified",
    };
}
