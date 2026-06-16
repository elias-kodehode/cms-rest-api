namespace EasterCMS.Models;

public class Prize
{
    public Guid Id { get; set; }
    public bool InStock { get; set; }
    public bool Collected { get; set; }

    public double Value { get; set; }

    public Participant? Participant { get; set; } 
    public Guid? ParticipantId { get; set; }
}
