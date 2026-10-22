using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;

namespace VAA.EAPI.Microservices.Retail.Seat.Domain
{
    public class OrderEmailRequest
    {
        public string Email { get; set; }
        public string RecordLocator { get; set; }
        
        public static explicit operator OrderEmailRequest(SeatFulfilmentRequest request)
        {
            return new OrderEmailRequest
            {
                Email = request.paymentDetails.FirstOrDefault(a => a.Payer?.PayerContacts?.Email.IsNullOrWhiteSpace() == false)?.Payer.PayerContacts.Email,
                RecordLocator = request.BookingReferences.RLOC
            };
        }

    }
}
