using BE.Data;
using BE.DTOs;
using BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MatchesController : ControllerBase
{
    private readonly ShottenContext _context;

    public MatchesController(ShottenContext context)
    {
        _context = context;
    }

    // GET: api/Matches
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MatchDto>>> GetMatches([FromQuery] int? playerId)
    {
        var query = _context.Matches
            .Include(m => m.Attendances)
            .ThenInclude(a => a.Player)
            .AsQueryable();

        if (playerId.HasValue)
        {
            var playerTeamIds = await _context.Players
                .Where(p => p.Id == playerId)
                .SelectMany(p => p.Teams.Select(t => t.Id))
                .ToListAsync();

            query = query.Where(m => m.TeamId.HasValue && playerTeamIds.Contains(m.TeamId.Value));
        }

        return await query.OrderBy(m => m.Date)
            .Select(m => new MatchDto
            {
                Id = m.Id,
                Date = m.Date,
                Location = m.Location,
                Name = m.Name,
                TeamName = m.TeamName,
                TeamId = m.TeamId,
                Attendances = m.Attendances.Select(a => new AttendanceDto
                {
                    MatchId = a.MatchId,
                    PlayerId = a.PlayerId,
                    Player = new PlayerDto { Id = a.Player.Id, Name = a.Player.Name },
                    Status = a.Status
                }).ToList()
            }).ToListAsync();
    }

    // GET: api/Matches/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MatchDto>> GetMatch(int id)
    {
        var match = await _context.Matches.Include(m => m.Attendances).ThenInclude(a => a.Player).FirstOrDefaultAsync(m => m.Id == id);

        if (match == null)
        {
            return NotFound();
        }

        var matchDto = new MatchDto
        {
            Id = match.Id,
            Date = match.Date,
            Location = match.Location,
            Name = match.Name,
            TeamName = match.TeamName,
            TeamId = match.TeamId,
            Attendances = match.Attendances.Select(a => new AttendanceDto
            {
                MatchId = a.MatchId,
                PlayerId = a.PlayerId,
                Player = new PlayerDto { Id = a.Player.Id, Name = a.Player.Name },
                Status = a.Status
            }).ToList()
        };

        return matchDto;
    }

    // PUT: api/Matches/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutMatch(int id, Match match)
    {
        if (id != match.Id)
        {
            return BadRequest();
        }

        _context.Entry(match).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!MatchExists(id))
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

    // POST: api/Matches
    [HttpPost]
    public async Task<ActionResult<MatchDto>> PostMatch(Match match)
    {
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();

        var players = await _context.Players.ToListAsync();
        foreach (var player in players)
        {
            var attendance = new Attendance
            {
                MatchId = match.Id,
                PlayerId = player.Id,
                Status = AttendanceStatus.NotPresent
            };
            _context.Attendances.Add(attendance);
        }
        await _context.SaveChangesAsync();

        var matchDto = new MatchDto
        {
            Id = match.Id,
            Date = match.Date,
            Location = match.Location,
            Name = match.Name,
            TeamName = match.TeamName,
            TeamId = match.TeamId,
            Attendances = match.Attendances.Select(a => new AttendanceDto
            {
                MatchId = a.MatchId,
                PlayerId = a.PlayerId,
                Player = new PlayerDto { Id = a.Player.Id, Name = a.Player.Name },
                Status = a.Status
            }).ToList()
        };

        return CreatedAtAction("GetMatch", new { id = match.Id }, matchDto);
    }

    // DELETE: api/Matches/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMatch(int id)
    {
        var match = await _context.Matches.FindAsync(id);
        if (match == null)
        {
            return NotFound();
        }

        _context.Matches.Remove(match);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool MatchExists(int id)
    {
        return _context.Matches.Any(e => e.Id == id);
    }
}