using BE.Models;

namespace BE.DTOs;

public class MatchDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? Location { get; set; }
    public string? Name { get; set; } // Full event name from iCal
    public string? TeamName { get; set; }
    public ICollection<AttendanceDto>? Attendances { get; set; }
}