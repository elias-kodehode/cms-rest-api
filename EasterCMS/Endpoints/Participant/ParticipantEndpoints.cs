using EasterCMS.Data;
using EasterCMS.DTOs;
using EasterCMS.Entities;
using EasterCMS.Services;
using EasterCMS.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.AspNetCore.Http.Results;

namespace EasterCMS.Endpoints;

public class ParticipantEndpoints : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder builder)
    {
        var participants = builder
            .MapGroup("/participants")
            .WithGroupName("Participant")
            .WithTags("Participants");

        //get ALL participants
        participants.MapGet("/", GetParticipants);

        //get participant by ID
        participants.MapGet("/{id:guid}", GetParticipantById);

        //create new participant
        participants.MapPost("/", CreateParticipant);

        //update participant, specified by its ID
        participants.MapPut("/{id:guid}", UpdateParticipant);

        //delete participant specified by its ID
        participants.MapDelete("/{id:guid}", DeleteParticipant);

        //assign prize to participant
        participants.MapPost("/{id:guid}/prizes/assign", AssignPrize);

        //get all participant prizes
        participants.MapGet("/{id:guid}/prizes", GetParticipantPrizes);

        //builder.MapGet("/participants", GetParticipants);
        //builder.MapGet("/participants/{id:guid}", GetParticipantById);
        //builder.MapPost("/participants", CreateParticipant);
        //builder.MapPut("/participants/{id:guid}", UpdateParticipant);
        //builder.MapDelete("/participants/{id:guid}", DeleteParticipant);
        //builder.MapPost("/participants/{id:guid}/prizes/assign", AssignPrize);
        //builder.MapGet("/participants/{id:guid}/prizes", GetParticipantPrizes);
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

    public record AssignPrizeRequest(Guid PrizeId);

    static async Task<IResult> GetParticipants(AppDbContext ctx)
    {
        return Ok(new
        {
            participants = await ctx
                .Participants
                .AsNoTracking()
                //.Include(x => x.Prizes)
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
    static async Task<IResult> GetParticipantById([FromRoute] Guid id, AppDbContext db)
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
    static async Task<IResult> CreateParticipant(CreateParticipantRequest request, AppDbContext db, ILogger<ParticipantEndpoints> logger)
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
    static async Task<IResult> UpdateParticipant(Guid id, UpdateParticipantRequest request, AppDbContext db, ILogger<ParticipantEndpoints> logger)
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
    static async Task<IResult> DeleteParticipant(Guid id, AppDbContext ctx, ILogger<ParticipantEndpoints> logger)
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
    static async Task<IResult> GetParticipantPrizes(Guid id, AppDbContext db)
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
    static async Task<IResult> AssignPrize(Guid id, AppDbContext db, AssignPrizeRequest request, ILogger<ParticipantEndpoints> logger)
    {
        var p = await db.Participants.FindAsync(id);

        if(p is null)
        {
            return NotFound("Participant not found");
        }

        var prize = await db.Prizes.FindAsync(request.PrizeId);

        if(prize is null)
        {
            return NotFound("Prize not found");
        }


        if(prize.ParticipantId == p.Id)
        {
            return BadRequest("Participant already owns this prize");
        }

        prize.ParticipantId = p.Id;

        logger.LogInformation("Assigned {prize} to {participant}", prize.Id, p.Id);
        await db.SaveChangesAsync();
        return Ok();
    }
}
