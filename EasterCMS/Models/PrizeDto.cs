namespace EasterCMS.Models;

public  class PrizeDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; } 
    public required bool InStock { get; init; } 
    public required bool Collected { get; init; } 
    public required double Value { get; init; } 
}
