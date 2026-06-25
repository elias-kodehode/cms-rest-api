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
        var p = await db
            .Participants
            .AsNoTracking()
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

        logger.LogInformation("Created new particiant {participant}", p);

        return Created($"/participants/{p.Id}", p);
    }
    async Task<IResult> UpdateParticipant(Guid id, UpdateParticipantRequest request, AppDbContext db, ILogger<ParticipantEndpoints> logger)
    {
        var p = await db.Participants.FirstOrDefaultAsync(x => x.Id == id);

        if (p is null)
        {
            return NotFound();
        }

        if (request.City is null && request.FullName is null && request.Age is null && request.CreatedAt is null)
        {
            return BadRequest();
        }


        if (request.City is not null && !request.City.Equals(p.City))
        {
            logger.LogInformation("Updating City of {participant} from {previous} to {new}", p, p.City, request.City);
            p.City = request.City;

        }

        if (request.FullName is not null && !request.FullName.Equals(p.FullName))
        {
            logger.LogInformation("Updating FullName of {participant} from {previous} to {new}",  p, p.FullName, request.FullName);
            p.FullName = request.FullName;
        }

        if (request.Age is not null && !request.Age.Equals(p.Age))
        {
            logger.LogInformation("Updating Age of {participant} from {previous} to {new}",  p, p.Age, request.Age);
            p.Age = request.Age.Value;
        }

        if (request.CreatedAt is not null && !request.CreatedAt.Equals(p.CreatedAt))
        {
            logger.LogInformation("Updating CreatedAt date of {participant} from {previous} to {new}",  p, p.CreatedAt, request.CreatedAt.Value);
            p.CreatedAt = request.CreatedAt.Value;
        }

        if (db.ChangeTracker.HasChanges())
        {
            p.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            logger.LogInformation("Updates of {participant} has been saved", p.Id);

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
        var participant = await db
            .Participants
            .AsNoTracking()
            .Include(x => x.Prizes)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (participant is null)
            return NotFound();

        var prizes = participant?.Prizes.Select(x => new PrizeDto { 
            Collected = x.Collected,
            Id = x.Id,
            InStock = x.InStock,
            Name = x.Name,
            Value = x.Value
        }).ToList();

        if (prizes is null || prizes.Count <= 0)
        {
            return NotFound();
        }

        return Ok(new {
            prizes
        });

    }

}
