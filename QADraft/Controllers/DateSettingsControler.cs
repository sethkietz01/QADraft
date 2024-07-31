using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;

[ApiController]
[Route("api/[controller]")]
public class DateSettingsController : ControllerBase
{
    private readonly string _filePath;

    public DateSettingsController()
    {
        _filePath = Path.Combine(Directory.GetCurrentDirectory(), "datesettings.json");
    }

    [HttpGet("dates")]
    public IActionResult GetDates()
    {
        try
        {
            Console.WriteLine("\n\n\nGetDates");
            if (!System.IO.File.Exists(_filePath))
            {
                return NotFound("Configuration file not found.");
            }

            var json = System.IO.File.ReadAllText(_filePath);

            return Ok(json);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("update-dates")]
    public IActionResult UpdateDates([FromBody] DateRange dateRange)
    {
        try
        {
            if (dateRange == null)
            {
                return BadRequest("Invalid date range.");
            }

            var startDate = DateTime.Parse(dateRange.StartDate);
            var endDate = DateTime.Parse(dateRange.EndDate);

            var dates = new
            {
                StartDate = startDate.ToString("yyyy-MM-dd"),
                EndDate = endDate.ToString("yyyy-MM-dd")
            };

            var json = JsonConvert.SerializeObject(dates, Formatting.Indented);
            System.IO.File.WriteAllText(_filePath, json);

            return Ok("Dates updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

public class DateRange
{
    public string StartDate { get; set; }
    public string EndDate { get; set; }
}
