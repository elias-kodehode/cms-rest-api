namespace EasterCMS.Models;

public  class ParticipantDto
{
    public required Guid Id { get; init; }
    public required string FullName { get; init; } 
    public required int Age { get; init; } 
    public required string City { get; init; } 
    public required List<PrizeDto> Prizes { get; init; } 
}