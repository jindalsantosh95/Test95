using Microsoft.Extensions.Logging;
using System.Globalization;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.AircraftChangeNotification;
using VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification;
using VAA.EAPI.Microservices.CRM.Notification.Infrastructure.Services;

namespace VAA.EAPI.Microservices.CRM.Notification.Application.Events
{
    public class AircraftChangeNotifCommandHandler : IAircraftChangeNotifCommand
    {
        private readonly IMessageCenterService _msgCenterService;
        private readonly ILogger<AircraftChangeNotifCommandHandler> _logger;

        public AircraftChangeNotifCommandHandler(IMessageCenterService msgCenterService, ILogger<AircraftChangeNotifCommandHandler> logger)
        {
            _msgCenterService = msgCenterService ?? throw new ArgumentNullException(nameof(msgCenterService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(CloudEvent cloudEventReq, CancellationToken cancellationToken)
        {
            try
            {
                var testEmail = ConfigReader.Try(Constants.Environment.TestEmail);
                var flightOrderDataItems = Json.Deserialize<IEnumerable<FlightOrderData>>(Convert.FromBase64String(cloudEventReq.Data as string).DecompressToString());

                var aircraftChangeRequests = GetAircraftChangeRequests(flightOrderDataItems.Where(x => !string.IsNullOrWhiteSpace(x.Passengers?.FirstOrDefault()?.Email)), testEmail);

                foreach (var request in aircraftChangeRequests)
                {
                    try
                    {
                        await _msgCenterService.Handle(request, cancellationToken);
                    }
                    catch (Exception exReq)
                    {
                        _logger.LogError(exReq, "Error processing Aircraft Swap Notification email for booking {BookingReference}:", request.BookingReference);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AircraftChangeNotifCommandHandler: {ex.Message}");
            }
        }


        private IEnumerable<AircraftChangeRequest> GetAircraftChangeRequests(IEnumerable<FlightOrderData> flightOrderDataItems, string testEmail)
        {
            return flightOrderDataItems.Select(x =>
            {
                var passenger = x.Passengers?.FirstOrDefault();

                return new AircraftChangeRequest()
                {
                    LeadPaxFirstName = passenger?.FirstName,
                    LeadPaxLastName = passenger?.LastName,
                    BookingReference = x.BookingReference?.PnrLocator,
                    FlightNumber = x.EventFlight?.OperatingCarrier?.FlightNum,
                    Origin = x.EventFlight?.Departure,
                    Destination = x.EventFlight?.Arrival,

                    DepartureDay = x.EventFlight?.ScheduledDepartureLocalDateTime is DateTime dep ? dep.ToString("dd") : null,
                    DepartureMonth = x.EventFlight?.ScheduledDepartureLocalDateTime is DateTime depMonth ? depMonth.ToString("MMM", CultureInfo.InvariantCulture) : null,
                    DepartureTime = x.EventFlight?.ScheduledDepartureLocalDateTime is DateTime depTime ? depTime.ToString("HH:mm") : null,

                    ArrivalDay = x.EventFlight?.ScheduledArrivalLocalDateTime is DateTime arr ? arr.ToString("dd") : null,
                    ArrivalMonth = x.EventFlight?.ScheduledArrivalLocalDateTime is DateTime arrMonth ? arrMonth.ToString("MMM", CultureInfo.InvariantCulture) : null,
                    ArrivalTime = x.EventFlight?.ScheduledArrivalLocalDateTime is DateTime arrTime ? arrTime.ToString("HH:mm") : null,

                    ToEmail = string.IsNullOrEmpty(testEmail) ? passenger?.Email : testEmail,
                };
            });
        }

    }
}

