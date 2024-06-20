using QADraft.Models;

namespace QADraft.Utilities
{
    //utility class to carry util/helper functions
    public static class SessionUtil
    {

        public static bool IsAuthenticated(HttpContext _context)
        {
            // Check if the session string "IsAuthenticated" is true and return true/false
            return _context.Session.GetString("IsAuthenticated") == "true";
        }

        public static int? GetId(HttpContext _context)
        {
            // Grab the session integer "Id" and return it
            return _context.Session.GetInt32("Id");
        }

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

        internal static dynamic GetLayout()
        {
            throw new NotImplementedException();
        }
    }
}
