using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nandel.Kafka.Outbox.Data;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        // Table
        builder.ToTable("outbox_messages");
        
        // Key
        builder.HasKey(message => message.Uid);
        
        // Indexes
        builder.HasIndex(message => message.CreatedAt);
        builder.HasIndex(message => message.SentAt);

        // Columns
        builder.Property(message => message.Uid).HasColumnName("uid");
        builder.Property(message => message.CreatedAt).HasColumnName("created_at");
        builder.Property(message => message.SentAt).HasColumnName("sent_at");
        
        builder.Property(message => message.Topic)
            .HasColumnName("topic")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(message => message.Key)
            .HasColumnName("key")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(message => message.Value)
            .HasColumnName("value")
            .IsRequired();
    }
}