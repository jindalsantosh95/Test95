using System.Collections.Generic;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PNR;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation
{
    public class PassengerFare
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string FirstNameNumber { get; set; }
        public string LastName { get; set; }
        public string LastNameNumber { get; set; }
        public int FarePoints { get; set; }
        public IEnumerable<TicketInformation> Tickets { get; set; }
    }
}
