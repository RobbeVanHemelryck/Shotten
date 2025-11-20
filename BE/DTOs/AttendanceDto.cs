
using BE.Models;

namespace BE.DTOs;

public class AttendanceDto
{
    public int MatchId { get; set; }
    public int PlayerId { get; set; }
    public PlayerDto? Player { get; set; }
    public AttendanceStatus Status { get; set; }
}