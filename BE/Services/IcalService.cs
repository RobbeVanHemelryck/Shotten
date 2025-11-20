using System.Globalization;

namespace BE.Services;

public class IcalService
{
    public class IcalEvent
    {
        public DateTime StartDate { get; set; }
        public string Location { get; set; }
        public string Summary { get; set; }
    }

    public async Task<List<IcalEvent>> GetIcalEvents(string icalUrl)
    {
        var events = new List<IcalEvent>();
        using (var httpClient = new HttpClient())
        {
            var icalContent = await httpClient.GetStringAsync(icalUrl);
                
            var lines = icalContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            IcalEvent currentEvent = null;
            foreach (var line in lines)
            {
                if (line == "BEGIN:VEVENT")
                {
                    currentEvent = new IcalEvent();
                }
                else if (line == "END:VEVENT")
                {
                    if (currentEvent != null)
                    {
                        events.Add(currentEvent);
                        currentEvent = null;
                    }
                }
                else if (currentEvent != null)
                {
                    if (line.StartsWith("DTSTART:"))
                    {
                        var dateString = line.Substring("DTSTART:".Length);
                        currentEvent.StartDate = DateTime.ParseExact(dateString, "yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
                    }
                    else if (line.StartsWith("LOCATION:"))
                    {
                        currentEvent.Location = line.Substring("LOCATION:".Length);
                    }
                    else if (line.StartsWith("SUMMARY:"))
                    {
                        currentEvent.Summary = line.Substring("SUMMARY:".Length);
                    }
                }
            }
        }
        return events;
    }
}