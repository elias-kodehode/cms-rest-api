using EasterCMS.Data;
using EasterCMS.Models;
using EasterCMS.Services;
using EasterCMS.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.Results;

namespace EasterCMS.Endpoints;

public class ParticipantEndpoints : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder app)
	{
		app.MapGet("/participants", GetParticipants);
		app.MapGet("/participants/{id}", GetParticipantById);
		app.MapPost("/participants", CreateParticipant);
		app.MapPut("/participants/{id}", UpdateParticipant);
		app.MapDelete("/participants/{id}", DeleteParticipant);
		app.MapGet("/participants/{id}/prizes", GetParticipantPrizes);
	}

	public record CreateParticipantRequest(
		string FullName,
		int Age,
		string City
	);

	public record UpdateParticipantRequest();

	async Task<IResult> GetParticipants(AppDbContext ctx)
	{
		return Ok(new
		{
			participants = await ctx
				.Participants
				.Include(x => x.Prizes)
				.Select(x => new ParticipantDto
				{
					Age = x.Age,
					City = x.City,
					FullName = x.FullName,
					Id = x.Id,
					Prizes = x.Prizes.Select(p => new PrizeDto
					{
						Collected = p.Collected,
						Id = p.Id,
						InStock = p.InStock,
						Name = p.Name,
						Value = p.Value
					}).ToList()
				})
				.ToListAsync()
		});
	}
	async Task<IResult> GetParticipantById([FromRoute] Guid id, AppDbContext db)
	{
		var p = await db
			.Participants
			.Include(x => x.Prizes)
			.Where(x => x.Id == id)
			.Select(x => new ParticipantDto
			{
				Id = x.Id,
				FullName = x.FullName,
				Age = x.Age,
				City = x.City,
				Prizes = x.Prizes.Select(p => new PrizeDto
				{
					Collected = p.Collected,
					Id = p.Id,
					InStock = p.InStock,
					Name = p.Name,
					Value = p.Value
				}).ToList()
			})
			.FirstOrDefaultAsync();

		return p is not null ? Ok(p) : NotFound();
	}
	async Task<IResult> CreateParticipant(CreateParticipantRequest request, AppDbContext db)
	{
		var validator = new CreateParticipantRequestValidator();
		var validationResult = await validator.ValidateAsync(request);


		if(!validationResult.IsValid)
		{
			return ValidationProblem(validationResult.ToDictionary());
		}

		var p = await db.Participants.AddAsync(new Participant
		{
			FullName = request.FullName,
			Age = request.Age,
			City = request.City,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		});
		await db.SaveChangesAsync();


		return Created($"/{p.Entity.Id}", p.Entity);
	}
	async Task<IResult> UpdateParticipant(Guid id, UpdateParticipantRequest request)
	{
		return Ok();
	}
	async Task<IResult> DeleteParticipant(Guid id, AppDbContext ctx)
	{
		var p = ctx
			.Participants
			.Include(x => x.Prizes)
			.FirstOrDefault(x => x.Id == id);

		if(p is null)
		{
			return NotFound();
		}

		if(p.Prizes.Any(x => !x.Collected))
		{
			return BadRequest("Cannot delete a user with an unclaimed prize");
		}
		ctx.Participants.Remove(p);
		await ctx.SaveChangesAsync();

		return Ok();
	}
	async Task<IResult> GetParticipantPrizes(Guid id, AppDbContext db)
	{
		var p = db.Participants.FirstOrDefault(x => x.Id == id);
		if(p is null)
		{
			return NotFound();
		}

		return Ok(new { prizes = p.Prizes.ToList() });

	}

}
