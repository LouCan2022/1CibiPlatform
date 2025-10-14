namespace Auth.Data.EntityConfiguration;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
	public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
	{
		builder.HasKey(t => t.Id);

		builder.Property(t => t.Id).ValueGeneratedOnAdd();

		builder.Property(t => t.UserId)
			   .IsRequired();

		builder.Property(t => t.TokenHash)
			   .IsRequired();

		builder.Property(t => t.IsUsed)
			   .HasDefaultValue(false);

		builder.Property(t => t.CreatedAt)
			   .HasDefaultValueSql("GETUTCDATE()")
			   .IsRequired();

		builder.Property(t => t.ExpiresAt)
			   .IsRequired();

		builder.HasOne<Authusers>()
			   .WithMany()
			   .HasForeignKey(t => t.UserId)
			   .OnDelete(DeleteBehavior.Cascade);
	}
}
