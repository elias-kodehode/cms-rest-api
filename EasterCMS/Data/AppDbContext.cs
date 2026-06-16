using EasterCMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EasterCMS.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public DbSet<Participant> Participants { get; set; }
    public DbSet<Prize> Prizes{ get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Participant>()
            .HasMany(participant => participant.Prizes)
            .WithOne(prize => prize.Participant)
            .HasForeignKey(p => p.ParticipantId)
            .IsRequired(false);
    }
}
