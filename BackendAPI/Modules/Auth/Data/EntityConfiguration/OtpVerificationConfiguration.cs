namespace Auth.Data.EntityConfiguration;

public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
	public void Configure(EntityTypeBuilder<OtpVerification> builder)
	{
		builder.HasKey(o => o.Id);

		builder.Property(o => o.Id).ValueGeneratedOnAdd();

		builder.Property(o => o.Email)
			   .IsRequired()
			   .HasMaxLength(255);

		builder.Property(u => u.PasswordHash)
			   .IsRequired();

		builder.Property(u => u.FirstName)
			   .IsRequired()
			   .HasMaxLength(100);

		builder.Property(u => u.LastName)
			   .IsRequired()
			   .HasMaxLength(100);

		builder.Property(u => u.MiddleName)
			   .HasMaxLength(100);

		builder.Property(o => o.OtpCodeHash)
			   .IsRequired();

		builder.Property(o => o.IsVerified)
			   .HasDefaultValue(false);

		builder.Property(o => o.IsUsed)
			   .HasDefaultValue(false);

		builder.Property(o => o.AttemptCount)
			   .HasDefaultValue(0);

		builder.Property(o => o.CreatedAt)
			   .HasDefaultValueSql("timezone('utc', now())")
			   .IsRequired();

		builder.Property(o => o.ExpiresAt)
			   .IsRequired();
	}
}
