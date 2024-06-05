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
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }



        [HttpGet]
        public IActionResult GeekAccounts()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }

            var qas = _context.Users
                .ToList();
            return View(qas);
        }



        [HttpGet]
        public IActionResult AddUser()
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            return View();
        }

        [HttpPost]
        public IActionResult AddUser(User user)
        {
            if (ModelState.IsValid)
            {
                user.Password = PasswordHasher.HashPassword(user.Password);
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("GeekAccounts");
            }
            return View(user);
        }





        [HttpGet]
        public IActionResult EditUser(string Id)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            int intId = int.Parse(Id);
            var userData = _context.Users.SingleOrDefault(u => u.Id == intId);
            if (userData == null)
            {
                return View();
            }

            return View(userData);
        }

        [HttpPost]
        public IActionResult EditUser(User user, string id, string action)
        {
            if (!IsAuthenticated())
            {
                return RedirectToAction("Login");
            }
            else if (ModelState.IsValid)
            {
                Debug.WriteLine("Valid ModelState");
                var existingUser = _context.Users.SingleOrDefault(u => u.Id == int.Parse(id));
                if (existingUser != null)
                {
                    Debug.WriteLine("User Exists");

                    Debug.WriteLine("Edit User");
                    existingUser.Username = user.Username;
                    if (user.Password != "-")
                        existingUser.Password = PasswordHasher.HashPassword(user.Password);
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.Role = user.Role;
                    _context.SaveChanges();
                }
            }
            Debug.WriteLine("End");
            return RedirectToAction("GeekAccounts");
        }

        

        

        

        [HttpGet]
        public IActionResult UserInfo()
        {
            if (!IsAuthenticated() || GetId() < 0)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.SingleOrDefault(u => u.Id == GetId());
            if (user != null)
            {
                return View(user);
            }

            return View();
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
