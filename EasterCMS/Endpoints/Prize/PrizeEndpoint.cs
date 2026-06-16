using EasterCMS.Data;
using EasterCMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasterCMS.Endpoints;

public class PrizeEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder app)
	{
		var group = app.MapGroup("/api");

		group.MapGet("/prizes", GetPrizes);
		group.MapPost("/prizes", CreatePrize);
		group.MapPost("/prizes/{id}/assign", AssignPrize);
	}





	async Task<IResult> GetPrizes(AppDbContext db)
	{
		return Results.Ok(new { prizes = await db.Prizes.ToListAsync() });
	}

	async Task<IResult> CreatePrize(AppDbContext db, CreatePrizeRequest request)
	{

		if(request.ParticipantId is not null)
		{
			var exists = await db.Participants.AnyAsync(x => x.Id == request.ParticipantId);

			if(!exists)
			{
				return Results.BadRequest("Participant not found");
			}
		}


		var prize = db.Prizes.Add(new Prize
		{
			InStock = true,
			Collected = false,
			ParticipantId = request.ParticipantId
		});

		prize.State = EntityState.Added;
		await db.SaveChangesAsync();

		return Results.Ok(new { prize.Entity.Id });
	}


	async Task<IResult> AssignPrize([FromRoute] Guid id, AssignPrizeRequest request, AppDbContext db)
	{

		var user = db.Participants.FirstOrDefault(x => request.ParticipantId == x.Id);

		if(user is null)
		{
			return Results.NotFound();
		}

		var prize = db.Prizes.FirstOrDefault(x => x.Id == id);

		if(prize is null)
		{
			return Results.NotFound();
		}
		db.Update(user);

		user.Prizes.Add(prize);

		await db.SaveChangesAsync();
		return Results.Ok(new { user.Prizes });
	}
	public record AssignPrizeRequest(
		Guid ParticipantId
	);


	public record CreatePrizeRequest(
		Guid? ParticipantId
	);
}
