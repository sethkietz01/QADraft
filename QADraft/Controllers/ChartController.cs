using Microsoft.AspNetCore.Mvc;
using QADraft.Models;
using QADraft.Services;
using System;
using System.Linq;

namespace QADraft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChartController : Controller
    {
        private readonly GeekQAService _geekQAService;

        public ChartController(GeekQAService geekQAService)
        {
            _geekQAService = geekQAService;
        }

        [HttpGet("Charts")]
        public IActionResult Charts()
        {
            if (IsAuthenticated())
            {
                return View();
            }
            return RedirectToAction("Login", "Home");
        }

        [HttpGet("GetFilteredQAs")]
        public IActionResult GetFilteredQAs([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string categoryOfError)
        {
            var data = _geekQAService.GetFilteredQAs(startDate, endDate, categoryOfError);
            var chartData = data.Select(qa => new
            {
                ErrorDate = qa.ErrorDate.ToString("yyyy-MM-dd"),
                CategoryOfError = qa.CategoryOfError,
                Count = 1 // assuming each record represents one occurrence
            });

            return Ok(chartData);
        }

        private bool IsAuthenticated()
        {
            // Implement your authentication check logic here
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        private string GetLayout()
        {
            // Implement your logic to get the layout
            return "_Layout";
        }

    }
}
