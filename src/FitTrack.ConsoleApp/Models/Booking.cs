using System;
using System.Collections.Generic;
using System.Text;

namespace FitTrack.ConsoleApp.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TrainingSessionId { get; set; }

        public DateTime BookedAt { get; set; } = DateTime.Now;
    }
}
