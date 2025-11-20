
namespace BE.DTOs;

public class PlayerDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public List<int>? TeamIds { get; set; }
}