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

namespace QADraft.Controllers
{
    public class QAController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public QAController(ApplicationDbContext context,ICompositeViewEngine viewEngine)
        {
            _context = context;
            _viewEngine = viewEngine;
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

            // Get every user's id and name and place into a list
            var users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            // Create a new GeekQA instance and assign it the current DateTime and list of users.
            GeekQA model = new GeekQA
            {
                ErrorDate = DateTime.Now,
                FoundOn = DateTime.Now,
                Users = users
            };

            // Return the AddQA view with the GeekQA instance model. This will auto-fill the related input fields.
            return View(model);
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

            // Verify that the submitted model state is valid (checks types and if all required model attributes are there)
            if (ModelState.IsValid)
            {
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

            // If model state is invalid, repopulate the users list
            model.Users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            // Direct the user back to the same page
            return View(model);
        }

        // Automatically flag the user's account if the QA's severity is 10.
        public void AutomaticFlag(User user, GeekQA model)
        {
            if (model.Severity == 10)
            {
                // Set the attribute isFlagged to true
                user.isFlagged = true;
                // Save the change to the database
                _context.SaveChanges();
            }

        }

        // Display the Filter page
        [HttpGet]
        public async Task<IActionResult> Filter(string dateFilter, DateTime? startDate, DateTime? endDate, int? committedBy, int? loggedBy, string category)
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

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

            // Add the QAs to a list and await async
            var model = await qas.ToListAsync();

            // Insert QAs into Viewbag as the inital content
            ViewBag.InitialContent = await RenderViewAsync("_Filter", model);
            // Return the view with the QAs
            return PartialView("_Filter", model);
        }

        // Display the QAs of the logged in user
        [HttpGet]
        public IActionResult YourQAs()
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Get the ID of the logged in user
            var id = SessionUtil.GetId(HttpContext);
            // Find the matching ID in the database table Users
            var user = _context.Users.SingleOrDefault(u => u.Id == id);
            // Check if a user was found
            if (user != null)
            {
                // If so, find all QAs where the ID matches
                var qa = _context.GeekQAs.Where(q => q.CommittedById == user.Id).Include(q => q.FoundBy).ToList();
                return View(qa);
            }
            // Return the view with the QAs
            return View();
            
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

            // Fetch all QAs along with the attributes CommitedBy and FoundBy (These attributes are names while CommitedByID and FoundByID are IDs).
            var qas = _context.GeekQAs
                .Include(q => q.CommittedBy)
                .Include(q => q.FoundBy)
                .ToList();

            // Get dictionaries of all QA Categories/Nature in form {Name:Count}. Pass these Dicts into the ViewBag.
            ViewBag.categoryDict = GetQADict("category");
            ViewBag.natureDict = GetQADict("nature");

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

            // Check if the passed action is to View/ Edit
            if (action == "View/Edit")
            {
                // Direct to the EditQA action and pass the ID of the QA and the source page
                return RedirectToAction("EditQA", new { id = qaId, src = source });
            }
            // Check if the passed action is to Delete
            else if (action == "Delete QA")
            {
                // Direct to the DeleteQA action and pass the ID of the QA and the source page
                return RedirectToAction("DeleteQA", new { id = qaId });
            }

            // Otherwise, an error has occured and they will be redirected to the home page (Home/Index)
            return RedirectToAction("Index", "Home");
        }

        // Display the Edit(View)QA page
        // View and Edit QA are consolidated into the same page
        [HttpGet]
        public IActionResult EditQA(int id)
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Fetch the QA that matches the ID of the QA selected from table
            var qa = _context.GeekQAs.Find(id);
            // Verify that the QA was found and exists
            if (qa != null)
            {
                // Fetch all of the users' names and store as a list
                var users = _context.Users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = $"{u.FirstName} {u.LastName}"
                }).ToList();

                // Verify that users were found
                if (users != null)
                {
                    // Assign the list of users to qa.Users and return the view with the fetched QA
                    qa.Users = users;
                    return View(qa);
                }
            }
            // If the qa or users were not found (null), return the default view.
            return View();
        }

        // Process the EditQA POST request
        [HttpPost]
        public IActionResult EditQA(GeekQA model, string src)
        {
            Debug.WriteLine($"editsrc:{src}");
            // Verify that the passed model is valid and contains all required fields
            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch the QA that matches the ID of the QA passed during POST
                    var qa = _context.GeekQAs.Find(model.Id);

                    if (qa == null)
                    {
                        return NotFound();
                    }

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

                    _context.Update(qa);
                    _context.SaveChanges();
                    if (src == "Filter")
                        return RedirectToAction("Filter");
                    else if (src == "AllGeekQAs")
                        return RedirectToAction("AllGeekQAs");
                }
                catch
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the QA. Please try again.");
                }
            }

            model.Users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.Id.ToString(),
                Text = $"{u.FirstName} {u.LastName}"
            }).ToList();

            return View(model);
        }


        [HttpGet]
        public IActionResult DeleteQA(int id)
        {
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }
            var findQA = _context.GeekQAs.SingleOrDefault(q => q.Id == id);
            if (findQA != null)
            {
                _context.Remove(findQA);
                _context.SaveChanges();
            }

            return View();
        }


        private async Task<string> RenderViewAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using (var writer = new StringWriter())
            {
                var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"{viewName} does not match any available view");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return writer.GetStringBuilder().ToString();
            }
        }

        // Create the dictionary needed for making pie chart
        public Dictionary<string, int> GetQADict(string type)
        {
            // Get all QA categories and natures from db
            var qas = _context.GeekQAs
                .Select(qa => new { qa.CategoryOfError, qa.NatureOfError })
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

            // Return the fetched dictionary
            return Dict;
        }

        // Display the category / nature piechart
        [HttpGet]
        public IActionResult PieChart()
        {
            // Return the piechart view
            return View();
        }

    }
}
