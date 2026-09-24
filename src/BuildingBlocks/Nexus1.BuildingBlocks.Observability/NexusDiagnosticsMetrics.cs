using System.Diagnostics.Metrics;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Nexus1.BuildingBlocks.Observability;

/// <summary>
/// The diagnosis/RAG-pipeline instrument set (From Flood to Cause, Appendix I;
/// ADR-038) -- distinct from <see cref="NexusRuntimeMetrics"/> (the messaging/workflow
/// meter), because the synchronous diagnosis pipeline is a different surface: how long
/// a diagnosis took, whether retrieval found grounding, how often the engine abstained,
/// how often the validator rejected.
///
/// Appendix I's own names are nexus.abstentions / nexus.validator_rejections /
/// nexus.diagnosis_ms / nexus.retrieval_recall on a meter "Nexus1.Diagnostics"; this
/// keeps the book's meter name but this repo's nexus1.* instrument-name convention.
///
/// Emitted UNTAGGED, faithfully to Appendix I (its instruments carry no dimensions)
/// and to avoid expanding this repo's reviewed metric-label vocabulary (ch.52 52-F,
/// <see cref="MetricLabelPolicy"/>). The duration distribution already shows the
/// bimodal cold/warm split without any tag; a per-incident breakdown is deferred.
/// </summary>
public sealed class NexusDiagnosticsMetrics
{
    public const string MeterName = "Nexus1.Diagnostics";

    /// <summary>Runs that abstained (any gate). Appendix I: nexus.abstentions.</summary>
    public Counter<long> Abstentions { get; }

    /// <summary>Runs whose draft failed H3/H4 validation. Appendix I: nexus.validator_rejections. A subset of Abstentions -- both move on a validation failure.</summary>
    public Counter<long> ValidatorRejections { get; }

    /// <summary>End-to-end diagnosis latency, milliseconds. Appendix I: nexus.diagnosis_ms.</summary>
    public Histogram<double> Duration { get; }

    /// <summary>
    /// PROXY, NOT recall. Records 1.0 when retrieval returned at least one passage
    /// above the H2 relevance floor for a run that reached retrieval, else 0.0; the
    /// mean over runs is a grounding-hit fraction. This fills Appendix I's
    /// retrieval_recall SLOT honestly: true recall needs a labelled golden set
    /// (H9 / Appendix J) that does not exist yet, so a metric called "recall" would
    /// claim a measurement this project cannot make. Never labelled or described as
    /// "recall" anywhere.
    /// </summary>
    public Histogram<double> GroundingHit { get; }

    public NexusDiagnosticsMetrics(IMeterFactory factory)
    {
        var meter = factory.Create(MeterName);

        Abstentions = meter.CreateCounter<long>(MetricNames.DiagnosisAbstentions, "{abstention}");
        ValidatorRejections = meter.CreateCounter<long>(MetricNames.DiagnosisValidatorRejections, "{rejection}");
        Duration = meter.CreateHistogram<double>(MetricNames.DiagnosisDuration, "ms");
        GroundingHit = meter.CreateHistogram<double>(MetricNames.DiagnosisGroundingHit, "{hit}");
    }
}
