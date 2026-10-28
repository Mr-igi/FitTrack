using System;
using System.Collections.Generic;
using System.Text;

namespace FitTrack.Core.Models
{
    public class Membership : IEntity
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int MembershipPlanId { get; set; }

        public decimal PricePaid { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public bool IsMarkedExpired { get; set; }

        public bool IsActiveOn(DateOnly date)
        {
            return date >= StartDate && date <= EndDate;
        }
        public static Membership Create(int userId, MembershipPlan plan, DateOnly startDate)
        {
            return new Membership
            {
                UserId = userId,
                MembershipPlanId = plan.Id,
                PricePaid = plan.GetFinalPrice(),
                StartDate = startDate,
                EndDate = startDate.AddDays(plan.DurationInDays - 1)
            };
        }
    }
}