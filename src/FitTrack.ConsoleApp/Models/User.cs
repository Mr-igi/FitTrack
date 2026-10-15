using System;
using System.Collections.Generic;
using System.Text;

namespace FitTrack.ConsoleApp.Models
{
    internal class User
    {
        public int Id { get; set; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }

        public Role Role { get; set; } = Role.Member;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string FullName => $"{FirstName} {LastName}";
    }
}
