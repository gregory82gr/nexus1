using System.Xml.Linq;

namespace Nexus1.ArchitectureTests;

/// <summary>
/// Enforces ADR-002's dependency law by parsing every src/ project's own
/// &lt;ProjectReference&gt; graph — not by reflecting on compiled assemblies,
/// since an empty skeleton project can carry an unused ProjectReference that
/// never shows up in emitted IL. The .csproj graph is the actual contract.
/// </summary>
public class DependencyLawTests
{
    private sealed record ProjectNode(string Name, string Layer, string? Context, IReadOnlyList<string> References);

    private static string FindRepoRoot()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Nexus1.Runtime.sln")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException(
                "Could not locate repository root: no Nexus1.Runtime.sln found above " + AppContext.BaseDirectory);
        }

        return dir.FullName;
    }

    private static (string Layer, string? Context) Classify(string projectName)
    {
        if (projectName is "Nexus1.BuildingBlocks.Domain" or "Nexus1.BuildingBlocks.Application"
            or "Nexus1.BuildingBlocks.Messaging" or "Nexus1.BuildingBlocks.Observability" or "Nexus1.ServiceDefaults")
        {
            return ("SharedKernel", null);
        }

        if (projectName.StartsWith("Nexus1.Contracts.", StringComparison.Ordinal))
        {
            return ("Contracts", projectName["Nexus1.Contracts.".Length..]);
        }

        if (projectName is "Nexus1.ModularRuntime" or "Nexus1.Bff" || projectName.EndsWith(".Host", StringComparison.Ordinal))
        {
            return ("Host", null);
        }

        // The served-model project (ADR-033) is a RootCause adapter that owns the
        // Semantic Kernel + Ollama dependencies. It binds by the same dependency
        // law as Infrastructure (its own context's Application/Domain, shared
        // kernel, contracts) — and is the sole name the served-model-package rule
        // below exempts.
        if (projectName == "Nexus1.RootCause.Explain")
        {
            return ("Infrastructure", "RootCause");
        }

        var parts = projectName.Split('.');
        if (parts.Length == 3 && parts[0] == "Nexus1" && parts[2] is "Domain" or "Application" or "Infrastructure")
        {
            return (parts[2], parts[1]);
        }

        throw new InvalidOperationException(
            $"Unclassified project '{projectName}'. Teach {nameof(DependencyLawTests)}.{nameof(Classify)} " +
            "about it before adding it to the solution.");
    }

    private static IReadOnlyList<ProjectNode> LoadSrcProjects()
    {
        var srcRoot = Path.Combine(FindRepoRoot(), "src");
        var nodes = new List<ProjectNode>();

        foreach (var path in Directory.EnumerateFiles(srcRoot, "*.csproj", SearchOption.AllDirectories))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var references = XDocument.Load(path)
                .Descendants("ProjectReference")
                .Select(e => Path.GetFileNameWithoutExtension((string)e.Attribute("Include")!))
                .ToList();
            var (layer, context) = Classify(name);
            nodes.Add(new ProjectNode(name, layer, context, references));
        }

        return nodes;
    }

    [Fact]
    public void Domain_projects_depend_only_on_the_domain_shared_kernel()
    {
        var violations = new List<string>();

        foreach (var project in LoadSrcProjects().Where(p => p.Layer == "Domain"))
        {
            violations.AddRange(
                from reference in project.References
                where reference != "Nexus1.BuildingBlocks.Domain"
                select $"{project.Name} -> {reference}");
        }

        Assert.True(violations.Count == 0,
            "Domain projects must reference only Nexus1.BuildingBlocks.Domain (dependencies point inward; " +
            "Domain never references transport, persistence, Host, Application, Infrastructure, or Contracts). " +
            "Violations:\n" + string.Join('\n', violations));
    }

    [Fact]
    public void Application_projects_reference_only_their_own_domain_shared_kernel_and_contracts()
    {
        var violations = new List<string>();

        foreach (var project in LoadSrcProjects().Where(p => p.Layer == "Application"))
        {
            foreach (var reference in project.References)
            {
                var (refLayer, refContext) = Classify(reference);

                var allowed = refLayer == "SharedKernel"
                    || refLayer == "Contracts"
                    || (refLayer == "Domain" && refContext == project.Context);

                if (!allowed)
                {
                    violations.Add($"{project.Name} -> {reference} ({refLayer})");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "Application projects may reference only: their own context's Domain project, the BuildingBlocks " +
            "shared-kernel projects, and any context's Contracts project. Cross-context data must flow through " +
            "producer-owned Contracts, never another context's Domain/Application/Infrastructure directly. " +
            "Violations:\n" + string.Join('\n', violations));
    }

    [Fact]
    public void Infrastructure_projects_reference_only_their_own_application_domain_shared_kernel_and_contracts()
    {
        var violations = new List<string>();

        foreach (var project in LoadSrcProjects().Where(p => p.Layer == "Infrastructure"))
        {
            foreach (var reference in project.References)
            {
                var (refLayer, refContext) = Classify(reference);

                var allowed = refLayer == "SharedKernel"
                    || refLayer == "Contracts"
                    || (refLayer is "Domain" or "Application" && refContext == project.Context);

                if (!allowed)
                {
                    violations.Add($"{project.Name} -> {reference} ({refLayer})");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "Infrastructure projects may reference only their own context's Domain/Application, the " +
            "BuildingBlocks shared kernel, and any context's Contracts project. Violations:\n" +
            string.Join('\n', violations));
    }

    [Fact]
    public void Contracts_projects_reference_nothing_else_in_the_solution()
    {
        var violations = (
            from project in LoadSrcProjects()
            where project.Layer == "Contracts"
            from reference in project.References
            select $"{project.Name} -> {reference}").ToList();

        Assert.True(violations.Count == 0,
            "Contracts projects are leaf DTO projects: no ProjectReference of their own. This is what makes " +
            "them safely referenceable across context boundaries without pulling in another context's " +
            "internals. Violations:\n" + string.Join('\n', violations));
    }

    [Fact]
    public void Shared_kernel_projects_reference_only_the_shared_kernel()
    {
        var violations = new List<string>();

        foreach (var project in LoadSrcProjects().Where(p => p.Layer == "SharedKernel"))
        {
            foreach (var reference in project.References)
            {
                var (refLayer, _) = Classify(reference);
                if (refLayer != "SharedKernel")
                {
                    violations.Add($"{project.Name} -> {reference} ({refLayer})");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "BuildingBlocks shared-kernel projects must not reference any context, contracts, or host project — " +
            "dependencies flow from contexts into the shared kernel, never the reverse. Violations:\n" +
            string.Join('\n', violations));
    }

    [Fact]
    public void Every_project_under_src_is_classified()
    {
        var count = LoadSrcProjects().Count;
        Assert.True(count > 0, "No projects found under src/ — repository root detection is likely broken.");
    }

    /// <summary>
    /// The One-Truth Pipeline's engine side is deterministic and LLM-free by
    /// construction (ADR-032): the served model sits behind a single deferred
    /// generation stage, and every other seam — graph walk, telemetry
    /// corroboration, retrieval, H3/H4 validation, the audit seal — decides
    /// without it. This pins that guarantee as a build-breaking fact: no src
    /// project may carry a served-model / LLM package, with exactly one future
    /// exception — Nexus1.RootCause.Explain, the deferred project that will own
    /// the Semantic Kernel + Ollama wiring and nothing else. If that project is
    /// ever added, it is the only place these packages may appear; if a served-
    /// model package leaks into the engine projects, this fails.
    /// </summary>
    [Fact]
    public void No_project_except_the_deferred_Explain_project_references_a_served_model_package()
    {
        // Matched case-insensitively against each PackageReference's Include name.
        string[] servedModelPackageMarkers =
        [
            "SemanticKernel", "Ollama", "LlamaSharp", "OpenAI", "Azure.AI", "Onnx",
            "Microsoft.ML", "TorchSharp", "Anthropic",
        ];

        const string allowedProject = "Nexus1.RootCause.Explain";

        var srcRoot = Path.Combine(FindRepoRoot(), "src");
        var violations = new List<string>();

        foreach (var path in Directory.EnumerateFiles(srcRoot, "*.csproj", SearchOption.AllDirectories))
        {
            var project = Path.GetFileNameWithoutExtension(path);
            if (project == allowedProject)
            {
                continue;
            }

            var packages = XDocument.Load(path)
                .Descendants("PackageReference")
                .Select(e => (string?)e.Attribute("Include") ?? string.Empty);

            violations.AddRange(
                from package in packages
                where servedModelPackageMarkers.Any(marker => package.Contains(marker, StringComparison.OrdinalIgnoreCase))
                select $"{project} -> {package}");
        }

        Assert.True(violations.Count == 0,
            "The deterministic engine side must stay LLM-free: no src project may reference a served-model / LLM " +
            $"package except the deferred {allowedProject} project (ADR-032). Violations:\n" +
            string.Join('\n', violations));
    }

    /// <summary>
    /// ADR-001-amend's original "no Contracts.ReactorFleet needed, consumption is
    /// in-process" claim was wrong (ADR-004) — this pins the fix so it can't
    /// silently regress. Redundant with the general cross-context rules above
    /// (AlarmManagement.Application/.Infrastructure already can't reference another
    /// context's Domain/Application by the generic Application/Infrastructure
    /// checks), but named explicitly for this specific pair since it's the one the
    /// corrected ADR calls out by name.
    /// </summary>
    [Fact]
    public void AlarmManagement_projects_reference_ReactorFleet_only_through_its_Contracts_project()
    {
        var violations = (
            from project in LoadSrcProjects()
            where project.Context == "AlarmManagement"
            from reference in project.References
            where reference is "Nexus1.ReactorFleet.Domain" or "Nexus1.ReactorFleet.Application" or "Nexus1.ReactorFleet.Infrastructure"
            select $"{project.Name} -> {reference}").ToList();

        Assert.True(violations.Count == 0,
            "AlarmManagement projects must reference ReactorFleet only through Nexus1.Contracts.ReactorFleet, " +
            "never Nexus1.ReactorFleet.Domain/.Application/.Infrastructure directly — regardless of same-host, " +
            "in-process composition (ADR-001-amend correction, ADR-004). Violations:\n" +
            string.Join('\n', violations));
    }
}
