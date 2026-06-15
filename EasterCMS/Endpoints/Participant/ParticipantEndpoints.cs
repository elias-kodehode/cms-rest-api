using EasterCMS.Data;
using Microsoft.AspNetCore.Mvc;

namespace EasterCMS.Endpoints.Participant;

public class ParticipantEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api");

        group.MapGet("/participants", (AppDbContext ctx) => {
            return Results.Ok(new
            {
                participants = ctx.Participants.ToList()
            });
        });


        group.MapGet("/participants/{id}", ([FromRoute] Guid id, AppDbContext ctx) => {
            var p = ctx.Participants.FirstOrDefault(x => x.Id == id);

            return p is not null ? Results.Ok(p) : Results.NotFound();
        });

        group.MapPost("/participants", async (CreateParticipantRequest request, AppDbContext ctx) => {
            var p = await ctx.Participants.AddAsync(new EasterCMS.Models.Participant
            {
                FullName = request.FullName,
                Age = request.Age,
                City = request.City,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();


            return Results.Ok(new
            {
                id = p.Entity.Id
            });
        });

        group.MapPut("/participants/{id}", () => { });

        group.MapDelete("/participants/{id}", () => { });

        group.MapGet("/participants/{id}/prizes", () => { });
    }

    record CreateParticipantRequest(
        string FullName,
        int Age,
        string City
    );
}
