using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QADraft.Data;
using QADraft.Models;
using QADraft.ViewModels;
using System;
using System.Diagnostics;

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

        // Process AddEvent POST request
        [HttpPost]
        public IActionResult AddEvent(Events newEvent)
        {
            // Verify that the event model stat is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // If it is, add the newEvent to the data base and save the changes made
                    _context.Events.Add(newEvent);
                    _context.SaveChanges();
                    // Direct to the Index page
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    // If an exception was caught, log and handle the error.
                    _logger.LogError(ex, "Error saving Event");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the Event. Please try again.");
                }
            }

            // If ModelState is not valid, return to Index with the current CombinedEventsViewModel
            var viewModel = new CombinedEventsViewModel
            {
                NewEvent = newEvent,
                EventsViewModel = new EventsViewModel()
            };
            // Return to the Index page with the view model
            return View("Index", viewModel);
        }


        // risky test, linked to JS method in Index.cshtml
        [HttpPost]
        public IActionResult UpdateEventInformation(Events model)
        {
            try
            {
                // Retrieve data from FormData
                var eventId = Request.Form["eventId"];
                var updatedEventInformation = Request.Form["updatedEventInformation"];


                var events = _context.Events.Find(int.Parse(eventId));
                if (events == null)
                {
                    return NotFound();
                }

                events.EventInformation = updatedEventInformation;
                _context.Update(events);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Handle exceptions if necessary
                return BadRequest(new { error = "An error occurred while updating the event." });
            }
        }

        [HttpPost]
        public IActionResult UpdateEventTime(Events model)
        {
            try
            {
                // Retrieve data from FormData
                var eventId = Request.Form["eventId"];
                var updatedEventTimeString = Request.Form["updatedEventTime"];

                if (!DateTime.TryParse(updatedEventTimeString, out DateTime updatedEventTime))
                {
                    // Handle invalid date format or other errors
                    return BadRequest(new { error = "Invalid date format or date value." });
                }

                var events = _context.Events.Find(int.Parse(eventId));
                if (events == null)
                {
                    return NotFound();
                }

                events.EventTime = updatedEventTime;
                _context.Update(events);
                _context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Handle exceptions if necessary
                return BadRequest(new { error = "An error occurred while updating the event." });
            }
        }
    }
}
