using System;

namespace QADraft.Models
{   //what created the user table and references for it.
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // e.g., coordinator, geek
        public bool isFlagged { get; set; }
        public bool isActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }
        public DateTime startDate { get; set;}
        public DateTime endDate { get; set;}
        public string? Theme { get; set; }
        public string? FlagDescription { get; set; }


    }
}
