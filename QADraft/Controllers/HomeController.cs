using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using QADraft.Data;
using QADraft.Models;
using QADraft.Utilities;
using QADraft.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QADraft.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SnipeItApiClient _snipeItApiClient;

        // HomeController constructor
        public HomeController(ApplicationDbContext context)
        {
            // Assigned the context to local variable
            _context = context;
            _snipeItApiClient = new SnipeItApiClient();
        }

        //TEST----------------------------------------------------------
        public async Task<IActionResult> Test()
        {
            try
            {
                DateTime StartDate = new DateTime(2024, 01, 01);
                DateTime EndDate = new DateTime(2024, 08, 31);


                int totalCheckedout = await _snipeItApiClient.ActivityReportBetween("checkout", StartDate, EndDate);
                Console.WriteLine($"Total Checked Out: {totalCheckedout}");

                int currentCheckedout = await _snipeItApiClient.GetStatusCount("Checked Out");
                Console.WriteLine($"Current Checked Out: {currentCheckedout}");

                int currentReChecked = await _snipeItApiClient.GetStatusCount("Re-Checked Out");
                Console.WriteLine($"Current Re-Checked Out: {currentReChecked}");

                int currentReminderEmail = await _snipeItApiClient.GetStatusCount("Reminder Email");
                Console.WriteLine($"Current Reminder Email: {currentReminderEmail}");

                int currentCourtesyCallCompleted = await _snipeItApiClient.GetStatusCount("Courtesy Call Completed");
                Console.WriteLine($"Current Courtesy Call Completed: {currentCourtesyCallCompleted}");

                int current1stLateFee = await _snipeItApiClient.GetStatusCount("1st Late Fee");
                Console.WriteLine($"Current 1st Late Fee: {current1stLateFee}");

                int current2ndLateFee = await _snipeItApiClient.GetStatusCount("2nd Late Fee");
                Console.WriteLine($"Current 2nd Late Fee: {current2ndLateFee}");

                int currentMaintenance = await _snipeItApiClient.GetStatusCount("Maintenance/Diagnose");
                Console.WriteLine($"Current Maintenance/Diagnose: {currentMaintenance}");

                Console.WriteLine("----------------------------------------");

                int currentCirculation = await _snipeItApiClient.GetStatusCount("In Circulation");
                Console.WriteLine($"Current Available for Checkout: {currentCirculation}");

                int availableWin = await _snipeItApiClient.GetCountInCirculation("WIN");
                Console.WriteLine($"Windows Laptops: {availableWin}");

                int availableAir = await _snipeItApiClient.GetCountInCirculation("AIR");
                Console.WriteLine($"Macbook Airs: {availableAir}");

                int availableMac = await _snipeItApiClient.GetCountInCirculation("MAC");
                Console.WriteLine($"Macbook Pros: {availableMac}");

                int availablGcal = await _snipeItApiClient.GetCountInCirculation("GCAL");
                Console.WriteLine($"Graphing Calculators: {availablGcal}");

                int availablProj = await _snipeItApiClient.GetCountInCirculation("PROJ");
                Console.WriteLine($"Projectors: {availablProj}");

                int availablCam = await _snipeItApiClient.GetCountInCirculation("CAM");
                Console.WriteLine($"Cameras: {availablCam}");


                // Process output as needed
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);  
                // Handle exception appropriately
                return RedirectToAction("PermissionsDenied");
            }

        }


        // Display the index / home page
        public IActionResult Index()
        {
            // Verify that the user is logged in
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Geek", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

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

            // Do not check for permissions on the initial login page

            // Get the target layout for the user role
            ViewBag.Layout = SessionUtil.GetLayout(HttpContext);

            // If the user is not logged in (most likely case), return the login page
            return View();
        }

        // Verify if the user has entered valid credentials
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // Do not check for permissions on the initial login page

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
                    HttpContext.Session.SetString("Theme", user.theme);

                    // Set the current DateTime as LastLogin for user in database
                    user.LastLogin = DateTime.Now;
                    _context.SaveChanges();

                    // Direct the user to the home page
                    return RedirectToAction("Index");
                }
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt");
            // If the login attempt fails (username or password don't match database) resend the login screen
            return View();
        }

        // Log the user out of the session
        [HttpGet]
        public IActionResult Logout()
        {
            // Do not check permissions on the logout page

            // Clear all session data
            HttpContext.Session.SetString("IsAuthenticated", "false");
            HttpContext.Session.SetString("Username", "");
            HttpContext.Session.SetString("FirstName", "");
            HttpContext.Session.SetString("LastName", "");
            HttpContext.Session.SetInt32("Id", 0);
            HttpContext.Session.SetString("Role", "");
            HttpContext.Session.SetString("Theme", "");

            // Direct the user back to the initial Login page
            return RedirectToAction("Login");
        }

        // Display the extern-site outages page
        [HttpGet]
        public IActionResult Outages()
        {
            // Verify that the user is logged in
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Geek", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            /*
            ZoomStatus.Get();

            // Get the app services health from google and display
            string[] outage = GoogleStatus.Get();
            ViewBag.GooglePercentOutage = outage[0];
            ViewBag.GoogleVitalOutage = outage[1];
            */

            // Return the outages view
            return View();
        }

        // Display the permissions denied screen
        [HttpGet]
        public IActionResult PermissionsDenied()
        {
            // Verify that the user is logged in
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Geek", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // If the user tries to access a page they don't have access to,
            // they should be immediatly redirected to the permissions denied page.
            return View();
        }

    }
}
