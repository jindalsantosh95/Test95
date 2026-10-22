using System;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity
{  
    public class MealOrders
    {
        public int MealOrderID { get; set; }

        public string MealOrderCode { get; set; }

        public string OrderID { get; set; }

        public string FlightNumber { get; set; }

        public string RecordLocator { get; set; }

        public string PaxId { get; set; }

        public string MealCode { get; set; }
            
        public DateTime FlightDate { get; set; }

        public string OrderStatus { get; set; }
        
        public DateTimeOffset ModifiedOn {  get; set; }
    }
}
