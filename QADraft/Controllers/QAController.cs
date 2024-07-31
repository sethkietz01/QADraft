using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QADraft.Data;
using QADraft.Models;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using QADraft.Utilities;
using Microsoft.Extensions.Configuration.UserSecrets;
using NuGet.Packaging;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using Microsoft.Identity.Client;

namespace QADraft.Controllers
{
    public class QAController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly string _filePath;

        public QAController(ApplicationDbContext context,ICompositeViewEngine viewEngine)
        {
            _context = context;
            _viewEngine = viewEngine;
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "datesettings.json");
        }

        // Display the AddQA page
        [HttpGet]
        public IActionResult AddQA()
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Coordinator", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Get every user's id and name and place into a list
            var users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            // Get every coordinator id and name and place into a list
            var coordinators = _context.Users.Where(u => u.Role == "Coordinator" || u.Role == "Admin" || u.Role == "Super Admin")
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName}"
                })
                .ToList();

            // Create a new GeekQA instance and assign it the current DateTime and the combined list of users.
            GeekQA qa = new GeekQA
            {
                ErrorDate = DateTime.Now,
                FoundOn = DateTime.Now,
                Users = users,
                Coordinators = coordinators
            };

            // Return the AddQA view with the GeekQA instance model. This will auto-fill the related input fields.
            return View(qa);
        }

        // Process the AddQA POST request
        [HttpPost]
        public IActionResult AddQA(GeekQA model)
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Coordinator", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Verify that the submitted model state is valid (checks types and if all required model attributes are there)
            if (ModelState.IsValid)
            {
                Console.WriteLine("valid");
                try
                {
                    // Set the currently logged in User for the "SubmittedBy" field.
                    model.SubmittedBy = SessionUtil.GetFullName(HttpContext);
                    _context.GeekQAs.Add(model);
                    _context.SaveChanges();

                    // Automatically flag the user that commited an error (if it needs to be flagged).
                    var user = _context.Users.FirstOrDefault(u => u.Id == model.CommittedById);
                    if (user != null)
                    {
                        AutomaticFlag(user, model);
                    }

                    // Direct the user back to the inital AddQA page
                    return RedirectToAction("AddQA");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the QA. Please try again.");
                }
            }
            Console.WriteLine("notvalid");

            // Get every user's id and name and place into a list
            model.Users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            // Get every coordinator id and name and place into a list
            model.Coordinators = _context.Users.Where(u => u.Role == "Coordinator" || u.Role == "Admin" || u.Role == "Super Admin")
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName}"
                })
                .ToList();

            Console.WriteLine(model.CategoryOfError);

            // Direct the user back to the same page
            return View("AddQA", model);
        }

        // Display the Filter page
        [HttpGet]
        public async Task<IActionResult> Filter(string dateFilter, DateTime? startDate, DateTime? endDate, int? committedBy, int? loggedBy, string category, int? severity, string? customerName)
        {
            Debug.WriteLine("\n\n\ndateFilter on filter = " + dateFilter + "\n\n\n");

            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Coordinator", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Pass all users into the viewbag to populate drop-down selectors
            ViewBag.Users = _context.Users.ToList();
            // Pass all error categories into viewbag
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Snipe-It", Text = "Snipe-It" },
                new SelectListItem { Value = "DocuSign", Text = "DocuSign" },
                new SelectListItem { Value = "Processes", Text = "Processes" },
                new SelectListItem { Value = "Desk Conduct", Text = "Desk Conduct" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };

            // Pass the user's role and name into the ViewBag to compare
            // against the QA they are trying to edit
            ViewBag.UserName = SessionUtil.GetFullName(HttpContext);
            ViewBag.UserRole = SessionUtil.GetRole(HttpContext);

            // Fetch all QAs as queryable
            var qas = _context.GeekQAs.AsQueryable();

            // Check if the datefilter box is checked
            if (!string.IsNullOrEmpty(dateFilter))
            {
                // Check if there is both a start and end date entered
                if (startDate.HasValue && endDate.HasValue)
                {
                    // Both start date and end date are provided
                    qas = qas.Where(q => q.ErrorDate >= startDate.Value && q.ErrorDate <= endDate.Value);
                }
                // If not, check for a lone start date
                else if (startDate.HasValue)
                {
                    // Only start date is provided, filter from start date to current date
                    qas = qas.Where(q => q.ErrorDate >= startDate.Value && q.ErrorDate <= DateTime.Now);
                }
                // Finally, check for a lone end date
                else if (endDate.HasValue)
                {
                    // Only end date is provided, filter from the minimum date to end date
                    qas = qas.Where(q => q.ErrorDate >= DateTime.MinValue && q.ErrorDate <= endDate.Value);
                }
            }
            // Check if the Commited By field has been given a value
            if (committedBy.HasValue)
            {
                // Add QAs that were commited by the selected person
                qas = qas.Where(q => q.CommittedById == committedBy.Value);
            }

            // Check if the Logged By field has been given a value
            if (loggedBy.HasValue)
            {
                // Add QAs that were logged by the selected person
                qas = qas.Where(q => q.FoundById == loggedBy.Value);
            }

            // Check if the Category field was given a value
            if (!string.IsNullOrEmpty(category))
            {
                // Add QAs that are of the same category
                qas = qas.Where(q => q.CategoryOfError == category);
            }

            if (severity.HasValue)
            {
                qas = qas.Where(q => q.Severity == severity.Value);
            }    

            if (!string.IsNullOrEmpty(customerName))
            {
                qas = qas.Where(q => q.CustomerName.ToLower() == customerName.ToLower());
            }

            // Add the QAs to a list and await async
            var model = await qas.ToListAsync();

            // Insert QAs into Viewbag as the inital content
            ViewBag.InitialContent = await RenderViewAsync("_Filter", model);
            // Return the view with the QAs
            return PartialView("_Filter", model);
        }

        // Display the QAs of the logged in user
        [HttpGet]
        public async Task<IActionResult> YourQAs(string dateFilter, DateTime? startDate, DateTime? endDate, int? committedBy, int? loggedBy, string category, int? severity)
        {
            // Verify that the user is logged in, if not direct them to login page
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

            // Get the appropriate navigation menu
            ViewBag.menu = SessionUtil.GetQAMenu(HttpContext);

            // Pass all error categories into viewbag
            ViewBag.Categories = new List<SelectListItem>
            {
                new SelectListItem { Value = "Snipe-It", Text = "Snipe-It" },
                new SelectListItem { Value = "DocuSign", Text = "DocuSign" },
                new SelectListItem { Value = "Processes", Text = "Processes" },
                new SelectListItem { Value = "Desk Conduct", Text = "Desk Conduct" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };

            // Fetch all QAs as queryable
            var userId = SessionUtil.GetId(HttpContext);
            var qas = _context.GeekQAs
                .Where(q => q.CommittedById == userId)
                .AsQueryable();

            // Check if the datefilter box is checked
            if (!string.IsNullOrEmpty(dateFilter))
            {
                // Check if there is both a start and end date entered
                if (startDate.HasValue && endDate.HasValue)
                {
                    // Both start date and end date are provided
                    qas = qas.Where(q => q.ErrorDate >= startDate.Value && q.ErrorDate <= endDate.Value);
                }
                // If not, check for a lone start date
                else if (startDate.HasValue)
                {
                    // Only start date is provided, filter from start date to current date
                    qas = qas.Where(q => q.ErrorDate >= startDate.Value && q.ErrorDate <= DateTime.Now);
                }
                // Finally, check for a lone end date
                else if (endDate.HasValue)
                {
                    // Only end date is provided, filter from the minimum date to end date
                    qas = qas.Where(q => q.ErrorDate >= DateTime.MinValue && q.ErrorDate <= endDate.Value);
                }
            }

            // Check if the Category field was given a value
            if (!string.IsNullOrEmpty(category))
            {
                // Add QAs that are of the same category
                qas = qas.Where(q => q.CategoryOfError == category);
            }

            if (severity.HasValue)
            {
                qas = qas.Where(q => q.Severity == severity);
            }

            // Add the QAs to a list and await async
            var model = await qas.ToListAsync();

            // Insert QAs into Viewbag as the inital content
            ViewBag.InitialContent = await RenderViewAsync("YourQAs", model);
            // Return the view with the QAs
            return PartialView("YourQAs", model);
        }

        // Display all of the Geek QAs
        [HttpGet]
        public IActionResult AllGeekQAs()
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Coordinator", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Get the appropriate navigation menu
            ViewBag.menu = SessionUtil.GetQAMenu(HttpContext);

            // Fetch all QAs along with the attributes CommitedBy and FoundBy (These attributes are names while CommitedByID and FoundByID are IDs).
            var qas = _context.GeekQAs
                .Include(q => q.CommittedBy)
                .Include(q => q.FoundBy)
                .ToList();

            // Get dictionaries of all QA Categories/Nature in form {Name:Count}. Pass these Dicts into the ViewBag.
            ViewBag.categoryDict = GetQADict("category");
            ViewBag.natureDict = GetQADict("nature");
            ViewBag.timeDict = GetQADict("time");

            // Pass the user's role and name into the ViewBag to compare
            // against the QA they are trying to edit
            ViewBag.UserName = SessionUtil.GetFullName(HttpContext);
            ViewBag.UserRole = SessionUtil.GetRole(HttpContext);

            // Return view with ViewBag and list of QAs
            return View("_AllGeekQAs", qas);
        }

        // Display the FlaggedAccounts page
        [HttpGet]
        public IActionResult FlaggedAccounts()
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Coordinator", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Get the appropriate navigation menu
            ViewBag.menu = SessionUtil.GetQAMenu(HttpContext);

            // Fetch all of QAs along with the attributes CommitedBy and FoundBy
            var flaggedAccounts = _context.GeekQAs
                .Include(q => q.CommittedBy)
                .Include(q => q.FoundBy)
                .Where(q => q.Severity >= 10) // Only *select* those QAs where the severity is 10 (or greater)
                .ToList();

            // Return the view with the list of flagged accounts
            return PartialView("_FlaggedAccounts", flaggedAccounts);
        }

        // Display the QADescriptions page
        [HttpGet]
        public IActionResult QADescriptions()
        {
            // Verify that the user is logged in, if not direct them to login page
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

            // Get the appropriate navigation menu
            ViewBag.menu = SessionUtil.GetQAMenu(HttpContext);

            // Return the view
            return View("_QADescriptions");
        }

        // Management function for View/Edit and Delete QA
        [HttpPost]
        public IActionResult ManageQA(int qaId, string action, string source)
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Coordinator", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // No layout to assign

            /*                  */
            ViewBag.source = source;

            // Check if the passed action is to View/ Edit
            if (action == "View/Edit")
            {
                // Direct to the EditQA action and pass the ID of the QA and the source page
                return RedirectToAction("EditQA", new { id = qaId, source = source });
            }
            // Check if the passed action is to Delete
            else 
            if (action == "Delete QA")
            {
                // Direct to the DeleteQA action and pass the ID of the QA and the source page
                return RedirectToAction("DeleteQA", new { id = qaId, source = source });
            }

            // Otherwise, an error has occured and they will be redirected to the home page (Home/Index)
            return RedirectToAction("Index", "Home");
        }


        // Display the Edit(View)QA page
        // View and Edit QA are consolidated into the same page
        [HttpGet]
        public IActionResult EditQA(int id, string source)
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Coordinator", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Fetch the QA that matches the ID of the QA selected from table
            var qa = _context.GeekQAs.Find(id);


            // Verify that the QA was found and exists
            if (qa == null)
            {
                return NotFound();
            }

            // Fetch all of the users' names and store as a list
            var users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            // Get every coordinator id and name and place into a list
            var coordinators = _context.Users.Where(u => u.Role == "Coordinator" || u.Role == "Admin" || u.Role == "Super Admin")
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName}"
                })
                .ToList();


            // Assign the list of users to qa.Users and 
            qa.Users = users;
            // Assign the list of coordinators(and above) to qa.Coordinators
            qa.Coordinators = coordinators;

            // Pass the source into the ViewBag
            ViewBag.source = source;    
            // Pass the user's role and name into the ViewBag to compare
            // against the QA they are trying to edit
            ViewBag.UserName = SessionUtil.GetFullName(HttpContext);
            ViewBag.UserRole = SessionUtil.GetRole(HttpContext);

            // Return the view with the fetched QA and the ViewBag
            return View(qa);
        }

        // Process the EditQA POST request
        [HttpPost]
        public IActionResult EditQA(GeekQA model, string source)
        {
            // The source string is passed through the ViewBag into an input field during the HTTPGet for EditQA
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Coordinator", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Source is passed by the page that calls EditQA (_Filter or AllGeekQAs)
            // Verify that the passed model is valid and contains all required fields
            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the QA that matches the ID of the QA passed during POST
                    var qa = _context.GeekQAs.Find(model.Id);
                    // Verify that an existing QA was found
                    if (qa != null)
                    {
                        // Take all (relevant) data from the passed model and overwrite the fetched qa.
                        qa.CommittedById = model.CommittedById;
                        qa.FoundById = model.FoundById;
                        qa.CategoryOfError = model.CategoryOfError;
                        qa.NatureOfError = model.NatureOfError;
                        qa.Severity = model.Severity;
                        qa.CustomerName = model.CustomerName;
                        qa.UnitId = model.UnitId;
                        qa.ErrorDate = model.ErrorDate;
                        qa.FoundOn = model.FoundOn;
                        qa.Description = model.Description;

                        // Update the database and save the changes
                        _context.Update(qa);
                        _context.SaveChanges();
                        // Use the source parmater to determine the source page and redirect accordingly
                        if (source == "Filter")
                            return RedirectToAction("Filter");
                        else if (source == "AllGeekQAs")
                            return RedirectToAction("AllGeekQAs");
                    }

                }
                catch { }
            }

            // If the model passed was not valid, regenerate the drop down data for users and pass the model back through the view
            model.Users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            // Get every coordinator id and name and place into a list
            model.Coordinators = _context.Users.Where(u => u.Role == "Coordinator" || u.Role == "Admin" || u.Role == "Super Admin")
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName}"
                })
                .ToList();

            // Return the view with the passed model along with the generated Users list.
            return View(model);
        }

        // Display the DeleteQA page
        [HttpGet]
        public IActionResult DeleteQA(int id, string source)
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Coordinator", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Fetch the matching qa by the ID
            var qa = _context.GeekQAs.SingleOrDefault(q => q.Id == id);
            // Verify that the qa exists and was found
            if (qa != null)
            {
                // Delete the fetched qa from the database and save the changes
                _context.Remove(qa);
                _context.SaveChanges();

                // Use the source parmater to determine the source page and redirect accordingly
                if (source == "Filter")
                    return RedirectToAction("Filter");
                else if (source == "AllGeekQAs")
                    return RedirectToAction("AllGeekQAs");
            }
            // Return to the all geek qas list if something goes wrong
            return RedirectToAction("AllGeekQAs");
        }

        // Automatically flag the user's account if the QA's severity is 10.
        public void AutomaticFlag(User user, GeekQA model)
        {
            if (model.Severity == 10)
            {
                // Get the error date and remove the time from it
                string errorDate = model.ErrorDate.ToString().Split(" ")[0];

                if (String.IsNullOrEmpty(user.FlagDescription))
                {
                    string flagDescription = "Automatic flag for " + model.NatureOfError + " on " + errorDate;
                    user.FlagDescription = flagDescription;
                }
                else
                {
                    user.FlagDescription = user.FlagDescription +  "<br> Automatic flag for " + model.NatureOfError + " on " + errorDate;
                }

                user.isFlagged = true;
                // Save the change to the database
                _context.SaveChanges();
            }

        }

        // Helper function to render a view asynchronously
        private async Task<string> RenderViewAsync(string viewName, object model)
        {
            // Pass the model into the ViewData
            ViewData.Model = model;
            // Use a StringWriter to capture the output of the view rendering
            using (var writer = new StringWriter())
            {
                // Use FindView on the passed view name to fetch the current view
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

                // Verify that that view exists and is valid
                if (viewResult.View == null)
                {
                    // If it is not, throw an exception
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                // Create a new ViewContext 
                var viewContext = new ViewContext(
                    ControllerContext,      // The current Controller context
                    viewResult.View,        // View instance found by viewResult
                    ViewData,               // The ViewData which contains the model
                    TempData,               // For temporary data storage
                    writer,                 // StringWriter to capture the output
                    new HtmlHelperOptions()     
                );

                // Wait for the view to be rendered on viewResult
                await viewResult.View.RenderAsync(viewContext);
                // Return the view content
                return writer.GetStringBuilder().ToString();
            }
        }

        // Create the dictionary needed for making Donut chart
        public Dictionary<string, int> GetQADict(string type)
        {
            // Get all QA categories and natures from db
            var qas = _context.GeekQAs
                .Select(qa => new { qa.CategoryOfError, qa.NatureOfError, qa.ErrorDate })
                .ToList();
            // Initilize empty string:int dictionary
            var Dict = new Dictionary<string, int>();
            // if-else to retrieve target QA attribute
            if (type == "category")
            {
                // Get each category using.GroupBy, then convert it to a dictionary
                Dict = qas
                    .GroupBy(qa => qa.CategoryOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            else if (type == "nature")
            {
                // Get each nature using.GroupBy, then convert it to a dictionary
                Dict = qas
                    .GroupBy(qa => qa.NatureOfError)
                    .ToDictionary(g => g.Key, g => g.Count());
            }
            else if (type == "time")
            {
                // Read the JSON file into a string
                string jsonString = System.IO.File.ReadAllText(_filePath);

                // Deserialize the JSON string into a DateSettings object
                var settings = System.Text.Json.JsonSerializer.Deserialize<DateSettings>(jsonString);

                Console.WriteLine("settings = " + settings);

                // Parse the dates from the settings
                DateTime startDate = DateTime.Parse(settings.StartDate);
                DateTime endDate = DateTime.Parse(settings.EndDate);

                Console.WriteLine("startDate = " + startDate);
                Console.WriteLine("endDate = " + endDate);

                // Calculate the number of weeks between startDate and endDate
                int startWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(startDate, CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday);
                int endWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(endDate, CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday);
                int totalWeeks = endWeek - startWeek + 1;

                // Group the `qas` data by week number relative to startDate
                Dict = qas
                    .Where(qa => qa.ErrorDate >= startDate && qa.ErrorDate <= endDate)  // Filter by date range
                    .GroupBy(qa => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(qa.ErrorDate, CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday) - startWeek + 1)
                    .ToDictionary(g => $"Week {g.Key}", g => g.Count());

                Console.WriteLine("timeDict = " + Dict);
                Console.WriteLine("timeDict.length = " + Dict.Count);
                Console.WriteLine("qas.length = " + qas.Count);
                foreach (var entry in Dict)
                {
                    int weekNumber = int.Parse(entry.Key.Replace("Week ", ""));
                    DateTime weekStart = startDate.AddDays((weekNumber - 1) * 7);
                    DateTime weekEnd = weekStart.AddDays(6);

                    // Ensure that weekEnd does not exceed the endDate
                    if (weekEnd > endDate)
                    {
                        weekEnd = endDate;
                    }

                    string weekStartFormatted = weekStart.ToString("yyyy-MM-dd");
                    string weekEndFormatted = weekEnd.ToString("yyyy-MM-dd");

                    Console.WriteLine($"{entry.Key} ({weekStartFormatted} to {weekEndFormatted}): {entry.Value}");
                }
            }

            // Return the fetched dictionary
            return Dict;
        }

        public class DateSettings
        {
            public string StartDate { get; set; }
            public string EndDate { get; set; }
        }
    }
}
