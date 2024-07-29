using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QADraft.Data;
using QADraft.Models;
using QADraft.Utilities;
using QADraft.ViewModels;
using System;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

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
            Console.WriteLine("\n\n\nId = " + newEvent.Id + "\nEventTime " + newEvent.EventTime + "\nEventInformation " + newEvent.EventInformation + "\nColor = " + newEvent.Color + "\n\n\n");

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

        [HttpPost]
        public IActionResult EditEvent(Events model)
        {
            Console.WriteLine("\nId " + model.Id + "\nEventTime " + model.EventTime + "\nEventInformation " + model.EventInformation + "\nColor " + model.Color);

            if (ModelState.IsValid)
            {
                Console.WriteLine("EditEvent valid model");

                var existingEvent = _context.Events.SingleOrDefault(e => e.Id == model.Id);

                if (existingEvent != null)
                {
                    // Update only the event with the matching Id
                    existingEvent.EventTime = model.EventTime;
                    existingEvent.EventInformation = model.EventInformation;

                    // Convert the hex code to lowercase
                    string normalizedColor = model.Color?.ToLowerInvariant();

                    if (normalizedColor == "#f5f5f5" || normalizedColor == "#4a4a4a")
                    {
                        existingEvent.Color = null;
                    }
                    else
                    {
                        existingEvent.Color = model.Color;
                    }

                    _context.SaveChanges();
                }
                else
                {
                    Console.WriteLine($"Event with Id {model.Id} not found.");
                }
            }
            else
            {
                Console.WriteLine("EditEvent not valid model");
            }

            return RedirectToAction("Index", "Home");
        }


        /*
         * Depricated
         * 
        // Test action for edit event info
        [HttpPost]
        public IActionResult EditEventInfo(int id, string text)
        {
            // Get the Event that matches the passed id
            var events = _context.Events.SingleOrDefault(e => e.Id == id);
            // Verify that the Event was found and exists
            if (events != null)
            {
                // Check if the new text is different from the old text
                if (events.EventInformation != text)
                {
                    // Update the event to match the new text
                    events.EventInformation = text;
                    _context.SaveChanges();
                }
            }
            // Return to the same page whether a change was made or not
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult EditEventTime(int id, DateTime time)
        {
            // Get the Event that matches the passed id
            var events = _context.Events.SingleOrDefault(e => e.Id == id);
            // Verify that the Event was found and exists
            if (events != null)
            {
                // Update the event to match the new time
                events.EventTime = time;
                _context.SaveChanges();
            }
            // Return to the same page whether a change was made or not
            return RedirectToAction("Index", "Home");
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
        */

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
