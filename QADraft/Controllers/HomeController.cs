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

        public IActionResult Index()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return View();
        }


        // This is the initial login referencer
        [HttpGet]
        public IActionResult Login()
        {
            if (IsAuthenticated())
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        // This is the actual function to see if the user has the correct login credentials, if not it redirects back to the login screen.
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            Debug.WriteLine("Login attempt");
            var user = _context.Users.SingleOrDefault(u => u.Username == username);

            if (user != null)
            {
                Debug.WriteLine($"User found: {username}");
                if (PasswordHasher.VerifyPassword(password, user.Password))
                {
                    Debug.WriteLine("Password verified");
                    HttpContext.Session.SetString("IsAuthenticated", "true");
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("FirstName", user.FirstName);
                    HttpContext.Session.SetString("LastName", user.LastName);
                    HttpContext.Session.SetInt32("Id", user.Id);
                    HttpContext.Session.SetString("Role", user.Role);

                    return RedirectToAction("Index");
                }
                else
                {
                    Debug.WriteLine("Password verification failed");
                }
            }
            else
            {
                Debug.WriteLine("User not found");
            }

            return View();
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("IsAuthenticated", "false");
            HttpContext.Session.SetString("Username", "");
            HttpContext.Session.SetInt32("Id", 0);
            return RedirectToAction("Login");
        }

        public bool IsAuthenticated()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        public int? GetId()
        {
            return HttpContext.Session.GetInt32("Id");
        }
    }
}
