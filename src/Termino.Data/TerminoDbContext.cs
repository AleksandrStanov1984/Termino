using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Termino.Domain.Entities;

namespace Termino.Data;
public class TerminoDbContext : DbContext
{
    public DbSet<TermItem> Terms => Set<TermItem>();

    public TerminoDbContext(DbContextOptions<TerminoDbContext> options) : base(options){ Database.EnsureCreated(); }

    protected override void OnModelCreating(ModelBuilder b)
    {
        var dto = new ValueConverter<DateTimeOffset,long>(v=>v.ToUnixTimeSeconds(), v=>DateTimeOffset.FromUnixTimeSeconds(v));
        var dtoN = new ValueConverter<DateTimeOffset?,long?>(v=>v.HasValue? v.Value.ToUnixTimeSeconds(): (long?)null, v=> v.HasValue? DateTimeOffset.FromUnixTimeSeconds(v.Value): (DateTimeOffset?)null);

        b.Entity<TermItem>(e => {
            e.ToTable("Terms");
            e.HasKey(x=>x.Id);
            e.Property(x=>x.Id).ValueGeneratedNever();
            e.Property(x=>x.Title).IsRequired().HasMaxLength(200);
            e.Property(x=>x.Description).HasMaxLength(4000);
            e.Property(x=>x.DueAt).HasConversion(dto).IsRequired();
            e.Property(x=>x.CreatedAt).HasConversion(dto);
            e.Property(x=>x.UpdatedAt).HasConversion(dtoN);
            e.Property(x=>x.CompletedAt).HasConversion(dtoN);
            e.Property(x=>x.LastReminderSentAt).HasConversion(dtoN);
            e.Property(x=>x.Reminder24hSentAt).HasConversion(dtoN);
            e.Property(x=>x.Reminder12hSentAt).HasConversion(dtoN);
            e.Property(x=>x.Reminder1hSentAt).HasConversion(dtoN);
            e.Property(x=>x.Status).HasConversion<int>();
        });
    }
}