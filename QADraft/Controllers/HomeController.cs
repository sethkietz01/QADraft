using Microsoft.AspNetCore.Mvc;
using QADraft.Data;
using QADraft.Models;
using QADraft.Utilities;
using QADraft.ViewModels;

namespace QADraft.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        // HomeController constructor
        public HomeController(ApplicationDbContext context)
        {
            // Assigned the context to local variable
            _context = context;
        }

        // Display the index / home page
        public IActionResult Index()
        {
            // Verify that the user is logged in
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Get the target layout for the user
            ViewBag.Layout = SessionUtil.GetLayout(HttpContext);

            // Initialize CombinedEventsViewModel
            var calendarModel = new CombinedEventsViewModel
            {
                EventsViewModel = new EventsViewModel
                {
                    // Retrieve all events from the database
                    EventList = _context.Events.ToList() 
                },
                // Initialize a new event for the form
                NewEvent = new Events() 
            };

            // return the index page with the calendar model
            return View(calendarModel);
        }
    

        // Initial login referencer
        [HttpGet]
        public IActionResult Login()
        {
            // Verify that the user is logged in
            if (SessionUtil.IsAuthenticated(HttpContext))
            {
                // Direct the user to the home page
                return RedirectToAction("Index");
            }
            // Get the target layout for the user role
            ViewBag.Layout = SessionUtil.GetLayout(HttpContext);

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

    }
}
