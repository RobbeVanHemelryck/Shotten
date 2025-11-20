using System.Collections.Generic;

namespace BE.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Player>? Players { get; set; }
    public ICollection<Match>? Matches { get; set; }
}
