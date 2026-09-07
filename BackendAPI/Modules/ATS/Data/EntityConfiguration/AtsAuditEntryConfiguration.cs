namespace ATS.Data.EntityConfiguration;

public class AtsAuditEntryConfiguration : IEntityTypeConfiguration<AtsAuditEntry>
{
	public void Configure(EntityTypeBuilder<AtsAuditEntry> builder)
	{
		builder.ToTable("AuditTrail", "ats");

		builder.HasKey(x => x.AuditEntryId);

		// Version 7 ids are minted by the behaviour so an entry keeps the order it was
		// recorded in even though the drain writes it later.
		builder.Property(x => x.AuditEntryId)
			   .ValueGeneratedNever();

		builder.Property(x => x.OccurredAt)
			   .IsRequired();

		builder.Property(x => x.Action)
			   .HasMaxLength(120)
			   .IsRequired();

		builder.Property(x => x.Area)
			   .HasMaxLength(80)
			   .IsRequired();

		builder.Property(x => x.Outcome)
			   .HasMaxLength(20)
			   .IsRequired();

		builder.Property(x => x.FailureReason)
			   .HasMaxLength(500);

		builder.Property(x => x.UserEmail)
			   .HasMaxLength(255);

		builder.Property(x => x.UserFullName)
			   .HasMaxLength(255);

		// Matches UserDetailsConfiguration's width for the column it is copied from.
		builder.Property(x => x.Site)
			   .HasMaxLength(100);

		builder.Property(x => x.IpAddress)
			   .HasMaxLength(64);

		builder.Property(x => x.TraceId)
			   .HasMaxLength(64);

		// jsonb rather than text so the payload can be queried directly when someone asks
		// "which action set this field", without a second migration later.
		builder.Property(x => x.Payload)
			   .HasColumnType("jsonb")
			   .IsRequired();

		// Same reasoning, and nullable: only tracked writes produce a diff.
		builder.Property(x => x.Changes)
			   .HasColumnType("jsonb");

		// Mirrors the screen's fixed (OccurredAt DESC, AuditEntryId DESC) ordering, so the
		// keyset page is an index scan rather than a sort.
		builder.HasIndex(x => new { x.OccurredAt, x.AuditEntryId })
			   .IsDescending(true, true);

		// The retention sweep deletes the oldest rows first and needs the ascending order.
		builder.HasIndex(x => x.OccurredAt);
	}
}
