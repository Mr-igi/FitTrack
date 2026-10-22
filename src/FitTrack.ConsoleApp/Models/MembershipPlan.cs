using System;
using System.Collections.Generic;
using System.Text;

namespace FitTrack.ConsoleApp.Models
{
    public class MembershipPlan : IEntity
    {
        public int Id { get; set; }

        public required string Name { get; set; }
        public decimal Price { get; set; }

        public int DurationInDays { get; set; }

        public int GetDiscountPercent() => DurationInDays switch
        {
            >= 365 => 20,
            >= 90 => 10,
            _ => 0
        };

        public decimal GetFinalPrice() => Price - Price * GetDiscountPercent() / 100;
    }
}