using System;
using System.Collections.Generic;
using System.Text;

namespace FitTrack.ConsoleApp.Models
{
    public class TrainingSession : IEntity
    {
        public int Id { get; set; }

        public int TrainerId { get; set; }

        public required string Title { get; set; }

        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }

        public required string Location { get; set; }

        public int Capacity { get; set; }

        public DateTime EndTime => StartTime.AddMinutes(DurationMinutes);

    }
}
