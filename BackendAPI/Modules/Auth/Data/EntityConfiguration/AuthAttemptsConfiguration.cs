namespace Auth.Data.EntityConfiguration;

public class AuthAttemptsConfiguration : IEntityTypeConfiguration<AuthAttempts>
{
	public void Configure(EntityTypeBuilder<AuthAttempts> builder)
	{
		builder.HasKey(x => x.userId);

		builder.Property(x => x.userId)
			.IsRequired();

		builder.Property(x => x.attempts)
			.IsRequired();

		builder.Property(x => x.message)
			.IsRequired()
			.HasMaxLength(500);

		builder.Property(x => x.createAt)
			.IsRequired()
			.HasDefaultValueSql("CURRENT_TIMESTAMP");
	}
}
