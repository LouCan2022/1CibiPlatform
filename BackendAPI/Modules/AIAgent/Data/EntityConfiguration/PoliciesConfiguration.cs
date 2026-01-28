namespace AIAgent.Data.EntityConfiguration;

public class PoliciesConfiguration : IEntityTypeConfiguration<AIPolicyEntity>
{
	public void Configure(EntityTypeBuilder<AIPolicyEntity> builder)
	{
		builder.ToTable("AIPolicy", "ai");

		builder.HasKey(e => e.Id);

		builder.Property(e => e.PolicyCode)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(e => e.SectionCode)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(e => e.DocumentName)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(e => e.Content)
			.IsRequired();

		builder.Property(e => e.ChunckId)
			.IsRequired();

		builder.Property(e => e.Embedding)
					.HasColumnType("vector(1536)");
	}
}
