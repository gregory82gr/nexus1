using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nexus1.RootCause.Domain.Grounding;

namespace Nexus1.RootCause.Infrastructure.Persistence.Configurations.Grounding;

public sealed class CorpusChunkConfiguration : IEntityTypeConfiguration<CorpusChunk>
{
    public void Configure(EntityTypeBuilder<CorpusChunk> builder)
    {
        builder.ToTable("Corpus", "RootCause");
        builder.HasKey(x => x.ChunkId).HasName("PK_RootCause_Corpus");

        builder.Property(x => x.ChunkId).ValueGeneratedOnAdd();
        builder.Property(x => x.UnitId).IsRequired();
        builder.Property(x => x.DocId).IsRequired();

        // Retrieval scoping key (ADR-037): the retriever filters chunks by unit.
        builder.HasIndex(x => x.UnitId).HasDatabaseName("IX_RootCause_Corpus_UnitId");
        builder.Property(x => x.TrustTier).IsRequired();
        builder.Property(x => x.Body).IsRequired();
        builder.Property(x => x.SourceLabel).HasMaxLength(256).IsRequired();

        // Plain column holding a float[] as JSON; null until the embedding
        // model (nomic-embed-text via Ollama) is available. No native SQL
        // Server 2025 VECTOR type -- cosine is computed in C# (ADR-032).
        builder.Property(x => x.EmbeddingJson);
    }
}
