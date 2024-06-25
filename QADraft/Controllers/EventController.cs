using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QADraft.Data;
using QADraft.Models;
using QADraft.Utilities;
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

        [HttpPost]
        public IActionResult DeleteEvent(int eventId)
        {
            Debug.Write("Rat");
            Debug.Write(eventId);
            try
            {
                // Fetch the matching Event by the eventId
                var eventToDelete = _context.Events.Find(eventId);

                // Verify that the Event exists and was found
                if (eventToDelete == null)
                {
                    Debug.Write("Hat");

                    return NotFound(); // Optionally handle case where event with given ID was not found
                }
                Debug.Write("Bat");
                // Delete the fetched Event from the database
                _context.Events.Remove(eventToDelete);
                _context.SaveChanges();
                Debug.Write("Sat");
                // Redirect to the Index action of the Event controller
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Optionally handle any exceptions that might occur during deletion
                _logger.LogError(ex, "Error deleting Event");
                ViewBag.ErrorMessage = "Failed to delete event. Please try again later.";
                return RedirectToAction("Index", "Home"); // Redirect to the event list page or handle the error appropriately
            }
        }
    }
}
