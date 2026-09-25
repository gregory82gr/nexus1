namespace Nexus1.ArchitectureTests;

/// <summary>
/// ADR-044: no host configuration file under src/ may carry a password. Connection strings with
/// credentials live in each host's User Secrets store (Development) instead. Scans the source
/// tree (bin/obj excluded -- build outputs are copies, not the tracked files), so a password
/// pasted back into any appsettings*.json fails the build's test gate.
/// </summary>
public class NoSecretsInTrackedConfigTests
{
    [Fact]
    public void No_appsettings_file_under_src_contains_a_password()
    {
        var src = Path.Combine(FindRepoRoot(), "src");

        var configFiles = Directory.EnumerateFiles(src, "appsettings*.json", SearchOption.AllDirectories)
            .Where(f => !f.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Any(segment => segment is "bin" or "obj"))
            .ToList();

        Assert.NotEmpty(configFiles); // guard the guard: an empty scan must not pass silently

        var offenders = configFiles
            .Where(f => File.ReadAllText(f).Contains("Password=", StringComparison.OrdinalIgnoreCase))
            .Select(f => Path.GetRelativePath(src, f))
            .ToList();

        Assert.True(
            offenders.Count == 0,
            "Plaintext password(s) found in tracked configuration -- move the connection string to User Secrets (ADR-044): "
                + string.Join(", ", offenders));
    }

    private static string FindRepoRoot()
    {
        DirectoryInfo? dir = new(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Nexus1.Runtime.sln")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException("Could not locate repository root: no Nexus1.Runtime.sln found above " + AppContext.BaseDirectory);
    }
}
