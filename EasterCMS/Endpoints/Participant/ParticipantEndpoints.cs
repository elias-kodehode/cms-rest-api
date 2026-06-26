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
        app.MapGet("/participants/{id:guid}", GetParticipantById);
        app.MapPost("/participants", CreateParticipant);
        app.MapPut("/participants/{id:guid}", UpdateParticipant);
        app.MapDelete("/participants/{id:guid}", DeleteParticipant);
        app.MapGet("/participants/{id:guid}/prizes", GetParticipantPrizes);
    }

    public record CreateParticipantRequest(
        string FullName,
        int Age,
        string City
    );

    public record UpdateParticipantRequest(
        string? FullName = null,
        int? Age = null,
        string? City = null,
        DateTime? CreatedAt = null
    );

    async Task<IResult> GetParticipants(AppDbContext ctx)
    {
        return Ok(new
        {
            participants = await ctx
                .Participants
                .AsNoTracking()
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
        var participant = await db
            .Participants
            .AsNoTracking()
            .Include(x => x.Prizes)
            .Where(x => x.Id == id)
            .Select(participant => new ParticipantDto
            {
                Id = participant.Id,
                FullName = participant.FullName,
                Age = participant.Age,
                City = participant.City,
                Prizes = participant.Prizes.Select(prize => new PrizeDto
                {
                    Collected = prize.Collected,
                    Id = prize.Id,
                    InStock = prize.InStock,
                    Name = prize.Name,
                    Value = prize.Value
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return participant is not null ? Ok(new { participant }) : NotFound("Participant not found");
    }
    async Task<IResult> CreateParticipant(CreateParticipantRequest request, AppDbContext db, ILogger<ParticipantEndpoints> logger)
    {
        var validator = new CreateParticipantRequestValidator();
        var validationResult = await validator.ValidateAsync(request);


        if (!validationResult.IsValid)
        {
            logger.LogError("Validation Error when attempting to create participant [{errors}]",
                validationResult.ToDictionary().Keys);
            return ValidationProblem(validationResult.ToDictionary());
        }

        var p = new Participant
        {
            FullName = request.FullName,
            Age = request.Age,
            City = request.City,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await db.Participants.AddAsync(p);
        await db.SaveChangesAsync();

        logger.LogInformation("Created new particiant {participant}", p);

        return Created($"/participants/{p.Id}", p);
    }
    async Task<IResult> UpdateParticipant(Guid id, UpdateParticipantRequest request, AppDbContext db, ILogger<ParticipantEndpoints> logger)
    {
        var participant = await db.Participants.FindAsync(id);

        if (participant is null)
        {
            return NotFound();
        }

        if (request.City is null && request.FullName is null && request.Age is null && request.CreatedAt is null)
        {
            return BadRequest();
        }


        if (request.City is string city && !city.Equals(participant.City))
        {
            logger.LogInformation("Updating City of {participant} from {previous} to {new}", participant, participant.City, city);
            participant.City = city;

        }

        if (request.FullName is string fullName && !fullName.Equals(participant.FullName))
        {
            logger.LogInformation("Updating FullName of {participant} from {previous} to {new}",  participant, participant.FullName, fullName);
            participant.FullName = fullName;
        }

        if (request.Age is int age && !age.Equals(participant.Age))
        {
            logger.LogInformation("Updating Age of {participant} from {previous} to {new}",  participant, participant.Age, age);
            participant.Age = age;
        }

        if (request.CreatedAt is DateTime createdAt && !createdAt.Equals(participant.CreatedAt))
        {
            logger.LogInformation("Updating CreatedAt date of {participant} from {previous} to {new}",  participant, participant.CreatedAt, createdAt);
            participant.CreatedAt =createdAt;
        }

        if (db.ChangeTracker.HasChanges())
        {
            participant.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            logger.LogInformation("Updates of {participant} has been saved", participant.Id);

        }
        return Ok();


    }
    async Task<IResult> DeleteParticipant(Guid id, AppDbContext ctx, ILogger<ParticipantEndpoints> logger)
    {
        var p = ctx
            .Participants
            .Include(x => x.Prizes)
            .FirstOrDefault(x => x.Id == id);

        if (p is null)
        {
            return NotFound();
        }

        if (p.Prizes.Any(x => !x.Collected))
        {
            return BadRequest("Cannot delete a user with an unclaimed prize");
        }

        ctx.Participants.Remove(p);
        await ctx.SaveChangesAsync();


        logger.LogInformation("Successfully deleted {p}", p.Id);
        return NoContent();
    }
    async Task<IResult> GetParticipantPrizes(Guid id, AppDbContext db)
    {

        var participant = await 
            db
            .Participants
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(participant => new
            {
                Prizes = participant.Prizes.Select(prize => new PrizeDto
                {
                    Id = prize.Id,
                    Collected = prize.Collected,
                    InStock = prize.InStock,
                    Name = prize.Name,
                    Value = prize.Value
                }).ToList()
            }).FirstOrDefaultAsync();

        if(participant is null)
        {
            return NotFound("Participant not found");
        }

        return Ok(new { 
            prizes = participant.Prizes
        });
    }
}
