namespace CNX.Data.EntityConfiguration;

public class CNXTalkpushConfiguration : IEntityTypeConfiguration<CNXTalkpush>
{
    public void Configure(EntityTypeBuilder<CNXTalkpush> builder)
    {
        builder.HasKey(c => c.CandidateId);
    }
}
