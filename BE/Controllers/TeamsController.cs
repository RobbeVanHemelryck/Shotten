using BE.Data;
using BE.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BE.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TeamsController : ControllerBase
{
    private readonly ShottenContext _context;

    public TeamsController(ShottenContext context)
    {
        _context = context;
    }

    // GET: api/Teams
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams()
    {
        return await _context.Teams
            .Select(t => new TeamDto
            {
                Id = t.Id,
                Name = t.Name
            })
            .ToListAsync();
    }
}
