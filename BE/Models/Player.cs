
namespace BE.Models;

public class Player
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ICollection<Team>? Teams { get; set; }
}