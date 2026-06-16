namespace EasterCMS.Models;

public class Participant
{
	public Guid Id { get; set; }
	public string FullName { get; set; } = string.Empty;
	public int Age { get; set; }
	public string City { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public ICollection<Prize> Prizes { get; set; } = [];
}
