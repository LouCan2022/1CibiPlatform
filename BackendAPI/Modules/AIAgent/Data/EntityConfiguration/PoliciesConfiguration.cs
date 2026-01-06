namespace AIAgent.Data.EntityConfiguration;

public class PoliciesConfiguration : IEntityTypeConfiguration<PolicyEntity>
{
	public void Configure(EntityTypeBuilder<PolicyEntity> builder)
	{
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

		builder.Property(e => e.Embedding)
			.HasConversion(
				v => v.ToArray(),
				v => new Vector(v))
			.HasColumnType("varbinary(max)");
	}
}
