using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PhilSys.Data.EntityConfiguration;

public class PhilSysTransactionResultConfiguration : IEntityTypeConfiguration<PhilSysTransactionResult>
{
	public void Configure(EntityTypeBuilder<PhilSysTransactionResult> builder)
	{
		builder.HasKey(ptr => ptr.Trid);

		builder.Property(ptr => ptr.Trid)
			   .IsRequired();

		builder.HasOne<PhilSysTransaction>()
			   .WithOne()
			   .HasForeignKey<PhilSysTransactionResult>(ptr => ptr.idv_session_id)
			   .OnDelete(DeleteBehavior.Cascade)
			   .IsRequired();

		builder.Property(ptr => ptr.verified)
			   .IsRequired();
	}
}
