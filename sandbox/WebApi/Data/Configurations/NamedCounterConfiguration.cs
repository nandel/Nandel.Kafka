using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace WebApi.Data.Configurations;

public class NamedCounterConfiguration : IEntityTypeConfiguration<NamedCounter>
{
    public void Configure(EntityTypeBuilder<NamedCounter> builder)
    {
        builder.ToTable("named_counters");
        builder.HasKey(counter => counter.Name);
    }
}