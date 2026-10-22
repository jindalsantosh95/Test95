using VAA.EAPI.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.ConfirmBaggage;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.OrderEmail
{
    public class BagsOrderEmailRequest
    {
        public string Email { get; set; }
        public string RecordLocator { get; set; }

        public static explicit operator BagsOrderEmailRequest(ConfirmBaggageRequest request)
        {
            return new BagsOrderEmailRequest
            {
                Email = request?.Email,
                RecordLocator = request.PnrLocator
            };
        }

        public static explicit operator CloudEvent(BagsOrderEmailRequest request)
        {
            var cloudEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = request,
                Type = Constants.Application.EmailBagsPaymentEventType,
                Source = Constants.Application.EmailBagsPaymentEventTopicName,
                Subject = Constants.Application.EmailBagsPaymentEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = "1.0"
            };

            AsyncContext.Current.MapToCloudEvent(cloudEvent);
            return cloudEvent;
        }
    }

}
