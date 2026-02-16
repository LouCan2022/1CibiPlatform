namespace Auth.Data.EntityConfiguration;

public class AuthAttemptsConfiguration : IEntityTypeConfiguration<AuthAttempts>
{
	public void Configure(EntityTypeBuilder<AuthAttempts> builder)
	{
		builder.HasKey(x => x.UserId);

		builder.Property(x => x.UserId)
			.IsRequired();

		builder.Property(x => x.Email)
			.IsRequired();

		builder.Property(x => x.Attempts)
			.IsRequired();

		builder.Property(x => x.Message)
			.IsRequired()
			.HasMaxLength(500);

		builder.Property(x => x.CreatedAt)
			.IsRequired()
			.HasDefaultValueSql("CURRENT_TIMESTAMP");
	}
}
