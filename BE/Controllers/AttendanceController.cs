using BE.Data;
using BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE.Controllers;

[Route("api/matches/{matchId}/players/{playerId}/attendance")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly ShottenContext _context;

    public AttendanceController(ShottenContext context)
    {
        _context = context;
    }

    // PUT: api/matches/5/players/5/attendance
    [HttpPut]
    public async Task<IActionResult> PutAttendance(int matchId, int playerId, AttendanceStatus status)
    {
        var attendance = await _context.Attendances.FindAsync(matchId, playerId);

        if (attendance == null)
        {
            attendance = new Attendance
            {
                MatchId = matchId,
                PlayerId = playerId,
                Status = status
            };
            _context.Attendances.Add(attendance);
        }
        else
        {
            attendance.Status = status;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AttendanceExists(matchId, playerId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    private bool AttendanceExists(int matchId, int playerId)
    {
        return _context.Attendances.Any(e => e.MatchId == matchId && e.PlayerId == playerId);
    }
}