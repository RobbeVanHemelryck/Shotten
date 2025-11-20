using System;
using System.Collections.Generic;

namespace BE.Models;

public class Match
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string? Location { get; set; }
    public string? Name { get; set; } // Full event name from iCal
    public string? TeamName { get; set; }
    public ICollection<Attendance>? Attendances { get; set; }
}