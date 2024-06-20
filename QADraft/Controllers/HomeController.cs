using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QADraft.Data;
using QADraft.Models;
using QADraft.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;
using System.Collections;
using QADraft.ViewModels;

namespace QADraft.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Dictionary<string, int> GetQADict(string type)
        {
            // Get all QA categories and natures from db
            var qas = _context.GeekQAs
                .Select(qa => new { qa.CategoryOfError, qa.NatureOfError })
                .ToList();
            // Initilize empty string:int dictionary
            var Dict = new Dictionary<string, int>();
            // if-else to retrieve target QA attribute
            if (type == "category") {
                // Get each category using.GroupBy, then convert it to a dictionary
                Dict = qas
                    .GroupBy(qa => qa.CategoryOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            else if (type == "nature") {
                // Get each nature using.GroupBy, then convert it to a dictionary
                Dict = qas
                    .GroupBy(qa => qa.NatureOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            
            // Return the fetched dictionary
            return Dict;
        }

        [HttpGet]
        public IActionResult PieChart()
        {
            return View();
        }


        public IActionResult Index()
        {
            // Verify that the user is logged in
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            // Get the target layout for the user
            ViewBag.Layout = GetLayout();

            // Initialize CombinedEventsViewModel
            var combinedModel = new CombinedEventsViewModel
            {
                EventsViewModel = new EventsViewModel
                {
                    // Retrieve all events from the database
                    EventList = _context.Events.ToList() 
                },
                // Initialize a new event for the form
                NewEvent = new Events() 
            };

            return View(combinedModel);
        }
    

        // Initial login referencer
        [HttpGet]
        public IActionResult Login()
        {
            // Verify that the user is logged in
            if (IsAuthenticated())
            {
                // Direct the user to the home page
                return RedirectToAction("Index");
            }
            // Get the target layout for the user role
            ViewBag.Layout = GetLayout();

            // If the user is not logged in (most likely case), return the login page
            return View();
        }

        // Verify if the user has entered valid credentials
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Fetch the account where the username is the same as the user-entered username
            var user = _context.Users.SingleOrDefault(u => u.Username == username);
            // If a user was found (not null), verify the password
            if (user != null)
            {
                // Call PasswordHasher utilitiy to check the password.
                // The user-entered password is hashed and then compared to the hashed password saved in the database.
                if (PasswordHasher.VerifyPassword(password, user.Password))
                {
                    // If the user has been verified, set/assign the session data
                    HttpContext.Session.SetString("IsAuthenticated", "true");
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    HttpContext.Session.SetString("LastName", user.LastName);
                    HttpContext.Session.SetInt32("Id", user.Id);
                    HttpContext.Session.SetString("Role", user.Role);

                    // Set the current DateTime as LastLogin for user in database
                    user.LastLogin = DateTime.Now;
                    _context.SaveChanges();

                    // Direct the user to the home page
                    return RedirectToAction("Index");
                }
            }
            // If the login attempt fails (username or password don't match database) resend the login screen
            return View();
        }

        // Log the user out of the session
        [HttpGet]
        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.SetString("IsAuthenticated", "false");
            HttpContext.Session.SetString("Username", "");
            HttpContext.Session.SetString("FirstName", "");
            HttpContext.Session.SetString("LastName", "");
            HttpContext.Session.SetInt32("Id", 0);
            HttpContext.Session.SetString("Role", "");

            // Direct the user back to the initial Login page
            return RedirectToAction("Login");
        }

        // Display the permissions denied screen
        [HttpGet]
        public IActionResult PermissionsDenied()
        {
            // If the user tries to access a page they don't have access to,
            // they should be immediatly redirected to the permissions denied page.
            return View();
        }

        // Display the extern-site outages page
        [HttpGet]
        public IActionResult Outages()
        {
            ZoomStatus.Get();

            // Get the app services health from google and display
            string[] outage = GoogleStatus.Get();
            ViewBag.GooglePercentOutage = outage[0];
            ViewBag.GoogleVitalOutage = outage[1];

            // Return the outages view
            return View();
        }

        // Helper function to check if user is currently logged in
        public bool IsAuthenticated()
        {
            // Check if the session string "IsAuthenticated" is true and return true/false
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        // Helper function to get the user's Id
        public int? GetId()
        {
            // Grab the session integer "Id" and return it
            return HttpContext.Session.GetInt32("Id");
        }

        // Helper function to determine the correct layout to use for the user's role
        public string GetLayout()
        {
            // Get the current user's role from the httpcontext session.
            string? role = HttpContext.Session.GetString("Role");

            // Assign the appropriate layout for each role.
            if (role == "Geek")
                return "~/Views/Shared/_LayoutGeek.cshtml";

            else
                return "~/Views/Shared/_Layout.cshtml";
        }
    }
}
