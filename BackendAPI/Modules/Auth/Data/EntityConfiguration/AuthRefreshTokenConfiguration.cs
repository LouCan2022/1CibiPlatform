
namespace Auth.Data.EntityConfiguration;

internal class AuthRefreshTokenConfiguration : IEntityTypeConfiguration<AuthRefreshToken>
{
	public void Configure(EntityTypeBuilder<AuthRefreshToken> builder)
	{
		builder.HasKey(e => e.Id);

		builder.Property(e => e.CreatedAt).HasDefaultValueSql("timezone('utc', now())");

		builder.Property(e => e.IsActive).HasDefaultValue(true);
	}
}
