using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QADraft.Data;
using QADraft.Models;
using QADraft.ViewModels;
using System;
using System.Linq;

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

        public IActionResult Index()
        {
            var combinedModel = new CombinedEventsViewModel
            {
                EventsViewModel = new EventsViewModel
                {
                    EventList = _context.Events.ToList()
                },
                NewEvent = new Events()
            };

            return View(combinedModel); // Ensure this view is located in /Views/Event/Index.cshtml
        }

        [HttpPost]
        public IActionResult AddEvent(CombinedEventsViewModel combinedModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Add the new event to the database
                    _context.Events.Add(combinedModel.NewEvent);
                    _context.SaveChanges();

                    // Redirect to Index action after successful addition
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving Event");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the Event. Please try again.");

                    // Log exception details
                    _logger.LogError(ex, "Exception occurred while saving the event: {Message}", ex.Message);
                }
            }
            else
            {
                // Log ModelState errors if ModelState is not valid
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        _logger.LogError(error.ErrorMessage);
                    }
                }
            }

            // If model state is not valid or there's an error, reload the view with the current combined model
            combinedModel.EventsViewModel = new EventsViewModel
            {
                EventList = _context.Events.ToList() // Refresh the event list
            };

            // Add a flag to indicate the modal should stay open
            ViewBag.ShowModal = true;

            // Return the view with the layout
            return View("Index", combinedModel);
        }

    }
}
