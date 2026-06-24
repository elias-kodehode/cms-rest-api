using EasterCMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EasterCMS.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Participant> Participants { get; set; }
	public DbSet<Prize> Prizes { get; set; }
	protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.Entity<Participant>()
			.HasMany(participant => participant.Prizes)
			.WithOne(prize => prize.Participant)
			.HasForeignKey(p => p.ParticipantId)
			.IsRequired(false);

		builder.Entity<Participant>().HasIndex(x => x.FullName);

		builder.Entity<Participant>()
			.HasData(new Participant
			{
				FullName = "Elias Sørensen",
				Age = 29,
				City = "Ulsteinvik",
				CreatedAt = new DateTime(),
				UpdatedAt = new DateTime(),
				Id = new("880595b4-1b81-40dc-8d41-5744284d8864")
			});

		builder.Entity<Prize>()
			.HasData(new Prize
			{
				Name = "Vase",
				Value = 1500,
				Collected = false,
				Id = new("880595b4-1b81-40dc-8d41-5744284d1234"),
				InStock = true,
				ParticipantId = new("880595b4-1b81-40dc-8d41-5744284d8864")
			},
			new Prize
			{
				Name = "Chocolate",
				Value = 50,
				Collected = false,
				Id = new("880595b4-1b81-40dc-8d41-5744284d1235"),
				InStock = true,
				//ParticipantId = new("880595b4-1b81-40dc-8d41-5744284d8864")
			});
	}
}
