using System.Text.Json.Serialization;

namespace EasterCMS.Models;

public class Prize
{
	public Guid Id { get; set; }
	public string Name { get; set; } = "";
	public bool InStock { get; set; }
	public bool Collected { get; set; }
	public double Value { get; set; }

	[JsonIgnore]
	public Participant? Participant { get; set; } = null;
	public Guid? ParticipantId { get; set; } = null;
}
