using System;
using System.Collections.Generic;
using System.Text;

namespace FitTrack.ConsoleApp.Models
{
    internal class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TrainingSessionId { get; set; }

        public DateTime BookedAt { get; set; } = DateTime.Now;
    }
}
