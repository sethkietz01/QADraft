using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QADraft.Data;
using QADraft.Models;
using QADraft.ViewModels;
using System;

namespace QADraft.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventController> _logger;

        public EventController(ApplicationDbContext context, ILogger<EventController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public IActionResult AddEvent(Events newEvent)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Events.Add(newEvent);
                    _context.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving Event");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the Event. Please try again.");
                }
            }

            // If ModelState is not valid, return to Index with the current CombinedEventsViewModel
            var viewModel = new CombinedEventsViewModel
            {
                NewEvent = newEvent,
                EventsViewModel = new EventsViewModel() // Replace with your actual EventsViewModel initialization
            };

            return View("Index", viewModel);
        }
    }
}
