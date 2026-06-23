using EasterCMS.Data;
using EasterCMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.Results;

namespace EasterCMS.Endpoints;

public class PrizeEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder app)
	{

		app.MapGet("/prizes", GetPrizes);
		app.MapPost("/prizes", CreatePrize);
		app.MapPost("/prizes/{id}/assign", AssignPrize);
	}



	public record AssignPrizeRequest(Guid? ParticipantId);
	public record CreatePrizeRequest(Guid? ParticipantId);

	async Task<IResult> GetPrizes(AppDbContext db)
	{
		return Ok(new
		{
			prizes = await db.Prizes.ToListAsync()
		});
	}
	async Task<IResult> CreatePrize(AppDbContext db, CreatePrizeRequest request)
	{

		if(request.ParticipantId is not null)
		{
			var exists = await db.Participants.AnyAsync(x => x.Id == request.ParticipantId);

			if(!exists)
			{
				return BadRequest("Participant not found");
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

		return Ok(new
		{
			prize.Entity.Id
		});
	}
	async Task<IResult> AssignPrize([FromRoute] Guid id, AssignPrizeRequest request, AppDbContext db)
	{
		var prize = db.Prizes.FirstOrDefault(x => x.Id == id);
		if(prize == null || !prize.InStock)
		{
			return BadRequest(
				prize == null ? "Prize not found":"Prize is not in stock"
			);
		}



		Participant? participant = null;

		if(request.ParticipantId is not null)
		{
			participant = db.Participants.FirstOrDefault(x => request.ParticipantId == x.Id);
            if (participant is null)
            {
                return NotFound();
            }
        }




		var entity = db.Update(prize);


		if(participant is null)
		{
			entity.Entity.Collected = false;
			entity.Entity.Participant = null;
			entity.Entity.ParticipantId = null;
		}
		else
		{
			entity.Entity.Collected = true;
			entity.Entity.ParticipantId = participant.Id;
		}
		await db.SaveChangesAsync();

		return Ok(new
		{

		});
	}


}
