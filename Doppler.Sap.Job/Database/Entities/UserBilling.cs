using System;

namespace Doppler.Sap.Job.Service.Database.Entities
{
    public class UserBilling
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string PlanType { get; set; }
        public long CreditsAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Fee { get; set; }
        public decimal TotalAmount { get; set; }

    }
}
