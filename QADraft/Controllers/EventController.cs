using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QADraft.Data;
using QADraft.Models;
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
        public IActionResult AddEvent(Events model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Save the event to the database
                    _context.Events.Add(model);
                    _context.SaveChanges();

                    // Redirect to a different view upon successful submission
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log any errors that occur during event saving
                    _logger.LogError(ex, "Error saving Event");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the Event. Please try again.");
                }
            }

            // If there are validation errors or other errors, return to the same view with the model
            return View(model);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
