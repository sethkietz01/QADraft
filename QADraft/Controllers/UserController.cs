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
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Identity;

namespace QADraft.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }


        // Display the accounts list page
        [HttpGet]
        public IActionResult GeekAccounts()
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

            // Pass the user role to viewbag
            ViewBag.role = SessionUtil.GetRole(HttpContext);

            // Fetch a list of all the users in the database
            var qas = _context.Users
                .ToList();
            // Return the view with the list of users
            return View(qas);
        }

        [HttpGet]
        public IActionResult InactiveAccounts()
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

            // Fetch a list of all the users in the database
            var qas = _context.Users
                .ToList();
            // Return the view with the list of users
            return View(qas);
        }


        // Display the AddUser page
        [HttpGet]
        public IActionResult AddUser()
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Admin", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Return the page
            return View();
        }

        // Process the AddUser POST request
        [HttpPost]
        public IActionResult AddUser(User user)
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Admin", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            Console.WriteLine("add user post");

            // Verify that the user Model passed is valid
            if (ModelState.IsValid)
            {
                Console.WriteLine("valid");
                // If it is, hash the password before it is stored in the database
                user.Password = PasswordHasher.HashPassword(user.Password);
                // Add the user to the database and save the changes made
                _context.Users.Add(user);
                _context.SaveChanges();
                // Return the GeekAccounts view (accounts list page)
                return RedirectToAction("GeekAccounts");
            }
            // If the model state is invalid, return the view again with the entered data.
            return View(user);
        }

        // Display the EditUser page
        [HttpGet]
        public IActionResult EditUser(string Id)
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Admin", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            // Conver the passed ID to an integer
            int intId = int.Parse(Id);
            // Fetch the user who's ID matches the passed ID
            var userData = _context.Users.SingleOrDefault(u => u.Id == intId);
            // Verify that the user exists and was found
            if (userData == null)
            {
                // If not, return an empty page
                return View();
            }
            // If the user was found, return the view with the user data
            return View(userData);
        }

        // Process the EditUser POST request
        [HttpPost]
        public IActionResult EditUser(User user, string id, string action)
        {
            // Verify that the user is logged in, if not direct them to login page
            if (!SessionUtil.IsAuthenticated(HttpContext))
            {
                return RedirectToAction("Login");
            }

            // Verify that the user has the permissions to view this page
            if (!SessionUtil.CheckPermissions("Admin", HttpContext))
            {
                return RedirectToAction("PermissionsDenied", "Home");
            }

            // Assign the appropriate layout
            ViewBag.layout = SessionUtil.GetLayout(HttpContext);

            Debug.WriteLine("Id: " + user.Id + "\nUsername: " + user.Username + "\nPassword: " + user.Password + "\nFirstName: " + user.FirstName + "\nLastName: " + user.LastName + "\nEmail: " + user.Email + "\nRole: " + user.Role + "\nisFlagged: " + user.isFlagged + "\nisActive: " + user.isActive + "\nCreatedAt: " + user.CreatedAt + "\nLastLogin: " + user.LastLogin + "\nStartDate: " + user.startDate + "\nendDate: " + user.endDate + "\nTheme: " + user.Theme + "\nFlagDescription: " + user.FlagDescription);

            // Verify that the user Model state is valid
            if (ModelState.IsValid)
            {
                Debug.WriteLine("User model is valid");
                // Fetch the user who's ID matches the passed ID
                var existingUser = _context.Users.SingleOrDefault(u => u.Id == int.Parse(id));
                // Verify that the user exists and was found
                if (existingUser != null)
                {
                    // Update all relevant attributes of the user to equal the passed data
                    existingUser.Username = user.Username;
                    // When the EditUser page is rendered, the password field will be blank. If the user does not change it
                    // (does not update the password) then the value is sent as "-".
                    // If the password value != "-", then update the password, otherwise don't
                    if (user.Password != "-")
                        existingUser.Password = PasswordHasher.HashPassword(user.Password);
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.isActive = user.isActive;
                    existingUser.Role = user.Role;
                    // Save all the changes made to the database.
                    _context.SaveChanges();
                }
            }
            else
            {
                Debug.WriteLine("User model is not valid");
            }
            // IReturn the user to the GeekAccounts page (accounts list page) whether the model state was valid or not
            return RedirectToAction("GeekAccounts");
        }

        // Display the UserInfo page
        [HttpGet]
        public IActionResult UserInfo()
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

            // Fetch the user from the database who's ID matches the ID of the current user
            var user = _context.Users.SingleOrDefault(u => u.Id == SessionUtil.GetId(HttpContext));
            // Verify that the user exists and was found
            if (user != null)
            {
                // If the user was found, return the view with the user's data
                return View(user);
            }
            // If the user was not found, return an empty view
            return View();
        }

        [HttpGet]
        public IActionResult UserThemes()
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

            return View();
        }

        [HttpPost]
        public IActionResult UserThemes(int id, string theme)
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

            var user = _context.Users.Find(id);

            if (user != null)
            {
                user.Theme = theme;
                _context.Update(user);
                _context.SaveChanges();
            }

            HttpContext.Session.SetString("Theme", theme);

            return View();
        }

        [HttpPost]
        public IActionResult FlagAccount(int id, string flagDescription)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user != null) 
            {
                user.isFlagged = true;

                if (String.IsNullOrEmpty(user.FlagDescription) || user.FlagDescription == "")
                {
                    user.FlagDescription = flagDescription;
                }
                else 
                {
                    user.FlagDescription = user.FlagDescription + "<br>" + flagDescription;
                }
                _context.SaveChanges();
            }

            return RedirectToAction("GeekAccounts");
        }

        [HttpPost]
        public IActionResult UnflagAccount(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user != null)
            {
                user.isFlagged = false;
                user.FlagDescription = "";
                _context.SaveChanges();
            }

            return RedirectToAction("GeekAccounts");
        }

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

            var flaggedAccounts = _context.Users
                .ToList();

            //return PartialView("FlaggedAccounts", flaggedAccounts);
            return View("FlaggedAccounts", flaggedAccounts);

        }

            /*
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
                //ViewBag.layout = SessionUtil.GetLayout(HttpContext);

                // Get the appropriate navigation menu
                //ViewBag.menu = SessionUtil.GetQAMenu(HttpContext);

                var flaggedAccounts = _context.Users
                    .Include(q => q.LastName)
                    .Include(q => q.FirstName)
                    .Include(q => q.FlagDescription)
                    .Where(q => q.isFlagged == true) 
                    .ToList();

                // Return the view with the list of flagged accounts
                return PartialView("FlaggedAccounts", flaggedAccounts);
            }
            */
        }
}
