using EasterCMS.Data;
using EasterCMS.Models;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace EasterCMS.Endpoints;

public class ParticipantEndpoints : IEndpoint
{
	public void MapEndpoint(RouteGroupBuilder app)
	{
		app.MapGet("/participants", GetParticipants);
		app.MapGet("/participants/{id}", GetParticipantByid);
		app.MapPost("/participants", CreateParticipant);
		app.MapPut("/participants/{id}", UpdateParticipant);
		app.MapDelete("/participants/{id}", DeleteParticipant);
		app.MapGet("/participants/{id}/prizes", GetParticipantPrizes);
	}

	record CreateParticipantRequest(
		string FullName,
		int Age,
		string City
	);

	record UpdateParticipantRequest();

	async Task<IResult> GetParticipants(AppDbContext ctx)
	{
		return Ok(new
		{
			participants = ctx.Participants.ToList()
		});
	}
	async Task<IResult> GetParticipantByid([FromRoute] Guid id, AppDbContext db)
	{
		var p = db.Participants.FirstOrDefault(x => x.Id == id);

		return p is not null ? Ok(p) : NotFound();
	}
	async Task<IResult> CreateParticipant(CreateParticipantRequest request, AppDbContext db)
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


		return Ok(new
		{
			id = p.Entity.Id
		});
	}
	async Task<IResult> UpdateParticipant(Guid id, UpdateParticipantRequest request)
	{
		return Ok();
	}
	async Task<IResult> DeleteParticipant(Guid id, AppDbContext ctx)
	{
		var p = ctx.Participants.FirstOrDefault(x => x.Id == id);

		if(p is null)
		{
			return NotFound();
		}
		ctx.Participants.Remove(p);

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
