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

            // Verify that the user Model passed is valid
            if (ModelState.IsValid)
            {
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

            // Verify that the user Model state is valid
            if (ModelState.IsValid)
            {
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
                user.theme = theme;
                _context.Update(user);
                _context.SaveChanges();
            }

            HttpContext.Session.SetString("Theme", theme);

            return View();
        }

    }
}
