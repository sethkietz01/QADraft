using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QADraft.Data;
using QADraft.Models;
using QADraft.Services;
using QADraft.Utilities;
using System;
using System.Globalization;
using System.Linq;

namespace QADraft.Controllers
{
    public class ChartController : Controller
    {
        private readonly GeekQAService _geekQAService;
        private readonly ApplicationDbContext _context;

        public ChartController(ApplicationDbContext context, GeekQAService geekQAService)
        {
            _context = context;
            _geekQAService = geekQAService;
        }

        
        public IActionResult Charts()
        {
            ViewBag.categoryDict = GetQADict("category");
            ViewBag.natureDict = GetQADict("nature");

            return View();
        }

        // Test connected scatterplot.

        // Helper method to get ISO 8601 week number
        private int GetIso8601WeekOfYear(DateTime date)
        {
            // Calculate the start of the week (Monday) for the given date
            DateTime startDateOfWeek = date.AddDays(DayOfWeek.Monday - date.DayOfWeek);

            // Return the ISO 8601 week number
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(startDateOfWeek, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public IActionResult ConnectedScatterPlot()
        {
            // Get all GeekQAs ordered by ErrorDate
            var qas = _context.GeekQAs.OrderBy(qa => qa.ErrorDate).ToList();

            // Group by week and count occurrences
            var occurrencesByWeek = qas.GroupBy(qa => GetIso8601WeekOfYear(qa.ErrorDate))
                                       .Select(g => new { Week = g.Key, Count = g.Count() })
                                       .OrderBy(g => g.Week)
                                       .ToList();

            // Print the results (you can modify this to suit your needs)
            foreach (var occurrence in occurrencesByWeek)
            {
                Console.WriteLine($"Week {occurrence.Week}: {occurrence.Count} occurrences");
            }

            return View();
        }


        // Create the dictionary needed for making Donut chart
        public Dictionary<string, int> GetQADict(string type)
        {
            // Get all QA categories and natures from db
            var qas = _context.GeekQAs
                .Select(qa => new { qa.CategoryOfError, qa.NatureOfError })
                .ToList();
            // Initilize empty string:int dictionary
            var Dict = new Dictionary<string, int>();
            // if-else to retrieve target QA attribute
            if (type == "category")
            {
                // Get each category using.GroupBy, then convert it to a dictionary
                Dict = qas
                    .GroupBy(qa => qa.CategoryOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            else if (type == "nature")
            {
                // Get each nature using.GroupBy, then convert it to a dictionary
                Dict = qas
                    .GroupBy(qa => qa.NatureOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }

            // Return the fetched dictionary
            return Dict;
        }

    }
}
