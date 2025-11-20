
namespace BE.Models;

public class Attendance
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public Match Match { get; set; }
    public int PlayerId { get; set; }
    public Player Player { get; set; }
    public AttendanceStatus Status { get; set; }
}