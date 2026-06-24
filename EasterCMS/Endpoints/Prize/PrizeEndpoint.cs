using EasterCMS.Data;
using EasterCMS.Models;
using EasterCMS.Services;
using EasterCMS.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.Results;

namespace EasterCMS.Endpoints;

public class PrizeEndpoint : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder app)
	{
		app.MapGet("/prizes", GetPrizes);
		app.MapGet("/prizes/{id}", GetPrizeById);
		app.MapPost("/prizes", CreatePrize);
		app.MapPut("/prizes/{id}", UpdatePrize);
		app.MapDelete("/prizes/{id}", DeletePrize);
		app.MapPost("/prizes/{id}/assign", AssignPrize);
		app.MapGet("/prizes/statistics", GetStatistics);
	}



	public record AssignPrizeRequest(Guid? ParticipantId);
	public record CreatePrizeRequest(Guid? ParticipantId, string Name, double Value);
	public record UpdatePrizeRequest(
		string? Name = null,
		double? Value= null,
		bool? Collected = null
		);
	public record GetStatisticsResponse(
		int TotalPrizes,
		int InStock,
		int Assigned,
		int Collected,
		double TotalValue,
		double AverageValue
	);


    static async Task<IResult> GetPrizeById(Guid id, AppDbContext db)
	{
		var prize = await db.Prizes.FirstOrDefaultAsync(x => x.Id == id);

		if(prize is null)
		{
			return NotFound("prize not found");
		}

		return Ok(new {
			prize = new PrizeDto
			{
				Collected = prize.Collected,
				Id = prize.Id,
				InStock = prize.InStock,
				Name = prize.Name,
				Value = prize.Value,
			}
		});
	}
    static async Task<IResult> GetPrizes(AppDbContext db)
	{
		return Ok(new
		{
			prizes = await db.Prizes.ToListAsync()
		});
	}
    static async Task<IResult> CreatePrize(AppDbContext db, CreatePrizeRequest request)
	{
		var validator = new CreatePrizeRequestValidator();
		var validationResults = await validator.ValidateAsync(request);

		if (!validationResults.IsValid)
		{
			return ValidationProblem(validationResults.ToDictionary());
		}

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
			Name = request.Name,
			Value = request.Value,
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
    static async Task<IResult> UpdatePrize(
		[FromRoute] Guid id, 
		[FromBody]UpdatePrizeRequest request,
		AppDbContext db)
	{
		var prize = await db.Prizes.FirstOrDefaultAsync(x => x.Id == id);

		if(prize is null)
		{
			return NotFound("Prize not found");
		}


		if(request.Name is null && request.Collected is null && request.Value is null)
		{
			return BadRequest();
		}

		var entity = db.Update(prize);

		if(request.Name is not null)
		{
			entity.Entity.Name = request.Name;
		}

		if(request.Value is not null)
		{
			entity.Entity.Value = request.Value.Value;
		}

		if(request.Collected is not null)
		{
			entity.Entity.Collected = request.Collected.Value;
		}


		await db.SaveChangesAsync();
		return Ok();
	}
    static async Task<IResult> DeletePrize(Guid id, AppDbContext db)
	{
		var prize = await db.Prizes.FirstOrDefaultAsync(x => x.Id == id);

		if (prize is null) return
				NotFound();

		if( prize.Collected == false)
			return Conflict("prize not claimed");


		db.Prizes.Remove(prize);
		await db.SaveChangesAsync();

		return Ok();
		
	}
    static async Task<IResult> AssignPrize([FromRoute] Guid id, AssignPrizeRequest request, AppDbContext db)
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
		entity.Entity.ParticipantId = request.ParticipantId;


		//if(participant is null)
		//{
		//	entity.Entity.Collected = false;
		//	entity.Entity.Participant = null;
		//	entity.Entity.ParticipantId = null;
		//}
		//else
		//{
		//	entity.Entity.Collected = true;
		//	entity.Entity.ParticipantId = participant.Id;
		//}
		await db.SaveChangesAsync();

		return Ok($"Assigned prize to {request.ParticipantId}");
	}
	static async Task<IResult> GetStatistics(AppDbContext db)
	{
		var response = await db
			.Prizes
			.GroupBy(_ => 1)
			.Select(x => new GetStatisticsResponse(
				TotalPrizes: x.Count(),
				InStock: x.Count(x => x.InStock),
				Assigned: x.Count(x => x.ParticipantId != null),
				Collected: x.Count(x => x.Collected),
				TotalValue: x.Sum(x => x.Value),
				AverageValue: x.Average(x => x.Value)
			))
			.FirstOrDefaultAsync();

        return Ok(response);
	}

}
