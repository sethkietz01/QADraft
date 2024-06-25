using QADraft.Models;

namespace QADraft.Utilities
{
    //utility class to carry util/helper functions
    public static class SessionUtil
    {
        // Helper function to determine if the user is currently logged in or not
        public static bool IsAuthenticated(HttpContext _context)
        {
            // Check if the session string "IsAuthenticated" is true and return true/false
            return _context.Session.GetString("IsAuthenticated") == "true";
        }

        // Helper function to fetch the ID of the logged in user
        public static int? GetId(HttpContext _context)
        {
            // Grab the session integer "Id" and return it
            return _context.Session.GetInt32("Id");
        }

        // Helper function to fetch the ID of the logged in user
        public static string? GetRole(HttpContext _context)
        {
            // Grab the session integer "Id" and return it
            return _context.Session.GetString("Role");
        }

        // Helper function to get the First and Last name of the logged in user
        public static string GetFullName(HttpContext _context)
        {
            // Get the first and last name from the HttpContext session
            string? firstName = _context.Session.GetString("FirstName");
            string? lastName = _context.Session.GetString("LastName");
            // Concatenate into a single string
            string? name = firstName + " " + lastName;
            // Return th 
            return name;
        }

        // Helper function to determine the correct layout to use for the user's role
        public static string GetLayout(HttpContext _context)
        {
            // Get the current user's role from the httpcontext session.
            string? role = _context.Session.GetString("Role");

            // Assign the appropriate layout for each role.
            if (role == "Geek")
                return "~/Views/Shared/_LayoutGeek.cshtml";

            else
                return "~/Views/Shared/_Layout.cshtml";
        }

        // Helper function to determine if the logged in user has access to a given page
        public static bool CheckPermissions(string clearance, HttpContext _context)
        {
            // Get the role of the current user
            string? userRole = _context.Session.GetString("Role");

            // Compare the passed clearance string agiainst the roles
            // Then, compare the current user's role aginst the roles
            // that should have access at said clearance level
            if (clearance == "Geek")
            {
                if (userRole == "Geek" || userRole == "Coordinator" || userRole == "Admin")
                {
                    return true;
                }
            }
            else if (clearance == "Coordinator")
            {
                if (userRole == "Coordinator" || userRole == "Admin")
                {
                    return true;
                }
            }
            else if (clearance == "Admin")
            {
                if (userRole == "Admin")
                {
                    return true;
                }
            }

            // If the passed clearence was not one of these roles, return false
            return false;
        }

        // Helper function to get the role-appropriate QA navigation menu
        public static string GetQAMenu(HttpContext _context)
        {
            // Get the current user's role from the httpcontext session.
            string? role = _context.Session.GetString("Role");

            // Assign the appropriate layout for each role.
            if (role == "Geek")
                return "_ReportsMenuGeek";

            else
                return "_ReportsMenu";
        }

    }
}
