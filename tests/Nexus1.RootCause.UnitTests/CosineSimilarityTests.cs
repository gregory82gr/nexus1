using Nexus1.RootCause.Application.Diagnosis;

namespace Nexus1.RootCause.UnitTests;

/// <summary>
/// Proves the C#-cosine math (ADR-032) directly with synthetic vectors -- the
/// path that will rank real corpus embeddings once the embedding model lands.
/// The math is right today; it simply has no real inputs yet.
/// </summary>
public class CosineSimilarityTests
{
    [Fact]
    public void Identical_vectors_have_cosine_one()
    {
        var v = new[] { 1.0, 2.0, 3.0 };
        Assert.Equal(1.0, CosineSimilarity.Between(v, v), precision: 12);
    }

    [Fact]
    public void Orthogonal_vectors_have_cosine_zero()
    {
        Assert.Equal(0.0, CosineSimilarity.Between([1.0, 0.0], [0.0, 1.0]), precision: 12);
    }

    [Fact]
    public void Opposite_vectors_have_cosine_minus_one()
    {
        Assert.Equal(-1.0, CosineSimilarity.Between([1.0, 1.0], [-1.0, -1.0]), precision: 12);
    }

    [Fact]
    public void Scale_invariant_a_longer_parallel_vector_is_still_one()
    {
        Assert.Equal(1.0, CosineSimilarity.Between([1.0, 2.0, 3.0], [2.0, 4.0, 6.0]), precision: 12);
    }

    [Fact]
    public void Zero_vector_yields_zero_not_nan()
    {
        var result = CosineSimilarity.Between([0.0, 0.0], [1.0, 1.0]);
        Assert.False(double.IsNaN(result));
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void Length_mismatch_yields_zero()
    {
        Assert.Equal(0.0, CosineSimilarity.Between([1.0, 2.0], [1.0, 2.0, 3.0]));
    }

    [Fact]
    public void Empty_vectors_yield_zero()
    {
        Assert.Equal(0.0, CosineSimilarity.Between([], []));
    }
}
