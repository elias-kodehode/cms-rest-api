using EasterCMS.Data;
using EasterCMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasterCMS.Endpoints;

public class ParticipantEndpoints : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder app)
	{
		var group = app.MapGroup("/api");

		group.MapGet("/participants", (AppDbContext ctx) =>
		{
			return Results.Ok(new
			{
				participants = ctx.Participants.ToList()
			});
		});


		group.MapGet("/participants/{id}", ([FromRoute] Guid id, AppDbContext db) =>
		{
			var p = db.Participants.FirstOrDefault(x => x.Id == id);

			return p is not null ? Results.Ok(p) : Results.NotFound();
		});

		group.MapPost("/participants", async (CreateParticipantRequest request, AppDbContext db) =>
		{
			var p = await db.Participants.AddAsync(new Participant
			{
				FullName = request.FullName,
				Age = request.Age,
				City = request.City,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			});
			await db.SaveChangesAsync();


			return Results.Ok(new
			{
				id = p.Entity.Id
			});
		});

		group.MapPut("/participants/{id}", () => { });

		group.MapDelete("/participants/{id}", (Guid id, AppDbContext ctx) =>
		{
			var p = ctx.Participants.FirstOrDefault(x => x.Id == id);

			if(p is null)
			{
				return Results.NotFound();
			}
			ctx.Participants.Remove(p);

			return Results.Ok();
		});

		group.MapGet("/participants/{id}/prizes", (Guid id, AppDbContext db) =>
		{
			var p = db.Participants.FirstOrDefault(x => x.Id == id);
			if(p is null)
			{
				return Results.NotFound();
			}

			return Results.Ok(new { prizes = p.Prizes.ToList() });

		});
	}

	record CreateParticipantRequest(
		string FullName,
		int Age,
		string City
	);
}
