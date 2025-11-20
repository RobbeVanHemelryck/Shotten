
using BE.Data;
using BE.DTOs;
using BE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlayersController : ControllerBase
{
    private readonly ShottenContext _context;

    public PlayersController(ShottenContext context)
    {
        _context = context;
    }

    // GET: api/Players
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayers()
    {
        return await _context.Players
            .Include(p => p.Teams)
            .Select(p => new PlayerDto
            {
                Id = p.Id,
                Name = p.Name,
                TeamIds = p.Teams.Select(t => t.Id).ToList()
            })
            .ToListAsync();
    }

    // GET: api/Players/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Player>> GetPlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);

        if (player == null)
        {
            return NotFound();
        }

        return player;
    }

    // PUT: api/Players/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlayer(int id, PlayerDto playerDto)
    {
        if (id != playerDto.Id)
        {
            return BadRequest();
        }

        var player = await _context.Players.Include(p => p.Teams).FirstOrDefaultAsync(p => p.Id == id);

        if (player == null)
        {
            return NotFound();
        }

        player.Name = playerDto.Name;

        // Update teams
        player.Teams?.Clear();
        if (playerDto.TeamIds != null && playerDto.TeamIds.Any())
        {
            var teams = await _context.Teams.Where(t => playerDto.TeamIds.Contains(t.Id)).ToListAsync();
            player.Teams = teams;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PlayerExists(id))
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

    // POST: api/Players
    [HttpPost]
    public async Task<ActionResult<PlayerDto>> PostPlayer(PlayerDto playerDto)
    {
        var player = new Player
        {
            Name = playerDto.Name,
            Teams = new List<Team>()
        };

        if (playerDto.TeamIds != null && playerDto.TeamIds.Any())
        {
            var teams = await _context.Teams.Where(t => playerDto.TeamIds.Contains(t.Id)).ToListAsync();
            player.Teams = teams;
        }

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        playerDto.Id = player.Id;
        playerDto.TeamIds = player.Teams.Select(t => t.Id).ToList();

        return CreatedAtAction("GetPlayer", new { id = player.Id }, playerDto);
    }

    // DELETE: api/Players/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player == null)
        {
            return NotFound();
        }

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PlayerExists(int id)
    {
        return _context.Players.Any(e => e.Id == id);
    }
}