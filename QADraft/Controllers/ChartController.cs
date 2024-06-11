using Microsoft.AspNetCore.Mvc;
using QADraft.Models;
using QADraft.Services;
using System;
using System.Collections.Generic;

namespace QADraft.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChartController : ControllerBase
    {
        private readonly GeekQAService _geekQAService;

        public ChartController(GeekQAService geekQAService)
        {
            _geekQAService = geekQAService;
        }

        [HttpGet]
        public IActionResult GetFilteredQAs([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string category)
        {
            var data = _geekQAService.GetFilteredQAs(startDate, endDate, category);
            return Ok(data);
        }
    }
}
