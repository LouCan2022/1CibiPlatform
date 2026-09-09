namespace ATS.Data.EntityConfiguration;

public class AtsNotificationConfiguration : IEntityTypeConfiguration<AtsNotification>
{
	public void Configure(EntityTypeBuilder<AtsNotification> builder)
	{
		builder.ToTable("Notifications", "ats");

		builder.HasKey(x => x.NotificationId);

		// Version 7 ids are minted in code so a row keeps the order it was raised in, and
		// so the id can be used as the keyset tie-breaker below.
		builder.Property(x => x.NotificationId)
			   .ValueGeneratedNever();

		builder.Property(x => x.RecipientUserId)
			   .IsRequired();

		builder.Property(x => x.Type)
			   .HasMaxLength(60)
			   .IsRequired();

		builder.Property(x => x.Title)
			   .HasMaxLength(160)
			   .IsRequired();

		// Wide enough for the bulk summary line, which interpolates a file name and two
		// counts. Senders truncate rather than letting an over-long body fail the write.
		builder.Property(x => x.Body)
			   .HasMaxLength(500)
			   .IsRequired();

		// Holds an app-relative path with an encoded query string, so it needs room for a
		// long subject name.
		builder.Property(x => x.LinkUrl)
			   .HasMaxLength(500);

		builder.Property(x => x.CreatedAt)
			   .IsRequired();

		// Mirrors the inbox's fixed (CreatedAt DESC, NotificationId DESC) ordering within
		// one recipient, so a keyset page is an index scan rather than a sort.
		builder.HasIndex(x => new { x.RecipientUserId, x.CreatedAt, x.NotificationId })
			   .IsDescending(false, true, true);

		// The unread badge is read on every page load, so it gets its own narrow index
		// rather than riding the one above.
		builder.HasIndex(x => new { x.RecipientUserId, x.IsRead });

		// The retention sweep deletes the oldest rows first and needs ascending order.
		builder.HasIndex(x => x.CreatedAt);
	}
}
