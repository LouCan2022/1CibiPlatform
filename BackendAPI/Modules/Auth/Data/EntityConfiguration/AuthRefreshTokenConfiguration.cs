
namespace Auth.Data.EntityConfiguration;

internal class AuthRefreshTokenConfiguration : IEntityTypeConfiguration<AuthRefreshToken>
{
	public void Configure(EntityTypeBuilder<AuthRefreshToken> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.CreatedAt).HasDefaultValue(DateTime.UtcNow);

		builder.Property(e => e.IsActive).HasDefaultValue(true);
	}
}
