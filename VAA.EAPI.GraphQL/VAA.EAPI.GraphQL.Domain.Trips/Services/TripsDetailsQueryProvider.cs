using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Enums;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS.DeepLink;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using CommonGraphql = VAA.EAPI.GraphQL.Common;
using TripMS = VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS;
using Virgin = VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin;


namespace VAA.EAPI.GraphQL.Domain.Trips.Services
{
    public class TripsDetailsQueryProvider : ITripsDetailsQueryProvider
    {
        private static readonly Dictionary<string, string> _headers =
            new() { { CommonGraphql.Constants.Application.OcpKeyHeader, ConfigReader.Get(CommonGraphql.Constants.Environment.AccountTripMsOcpKey) } };

        protected readonly IHttpConnector _connector;

        public TripsDetailsQueryProvider(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        public async Task<TripMS.Booking> GetTripDetails(TripMS.TripsDetailsRequest tripDetailsRequest)
        {
            ArgumentNullException.ThrowIfNull(tripDetailsRequest);
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/trips/search";

            return (await _connector.SendAsync<TripMS.TripsDetailsResponse>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(tripDetailsRequest))).Response.Booking;
        }

        public async Task<TripMS.TripListResponseStore> GetTripDetailsList(IEnumerable<TripMS.TripsDetailsRequest> tripDetailsRequest)
        {
            ArgumentNullException.ThrowIfNull(tripDetailsRequest);
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/tripsList/search";

            return (await _connector.SendAsync<TripMS.TripListResponseStore>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(tripDetailsRequest))).Response;
        }

        public async Task<Virgin.MessageResponse> GetTripMessageDetails(string tripReservationID)
        {
            ArgumentNullException.ThrowIfNull(tripReservationID);

            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/messages/{tripReservationID}";
            var tripMessages = (await _connector.GetAsync<TripMS.MessageResponse>(url, _headers, string.Empty)).Response;

            return (Virgin.MessageResponse)(tripMessages);
        }
        public async Task<IEnumerable<FlyReadyStatus>> GetFlyreadyTripMessageStatus(string tripReservationID)
        {
            ArgumentNullException.ThrowIfNull(tripReservationID);

            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/messages/{tripReservationID}";
            var tripMessages = (await _connector.GetAsync<TripMS.MessageResponse>(url, _headers, string.Empty)).Response;

            var messageResponses = GetFlyReadyMessages(tripMessages);

            return messageResponses;
        }

        public IEnumerable<FlyReadyStatus> GetFlyReadyMessages(TripMS.MessageResponse messageResponse)
        {
            var flyReadyMessages = new List<FlyReadyStatus>();

            foreach (var passenger in messageResponse.Passengers)
            {
                var passengerMessages = messageResponse.Messages.Where(m => m.CodeType.ToString() == MessageCodeType.FLYREADY.ToString() &&
                       (m.Passengers != null && m.Passengers.Any(p => p.Id == passenger.Id)));

                foreach (var message in passengerMessages)
                {
                    flyReadyMessages.Add(new FlyReadyStatus
                    {
                        PassengerID = passenger.Id,
                        Sliceid = message.TripId,
                        Status = GetFlyReadyStatus(message.MessageCode),
                        GivenName = passenger.GivenNames
                    });
                }
            }

            return flyReadyMessages;
        }
        private string GetFlyReadyStatus(string messageCode)
        {
            var redCodes = Constants.Application.TripMessageRedCode.Split(',').Select(c => c.Trim()).ToHashSet();
            var amberCodes = Constants.Application.TripMessageAmberCode.Split(',').Select(c => c.Trim()).ToHashSet();
            var greenCodes = Constants.Application.TripMessageGreenCode.Split(',').Select(c => c.Trim()).ToHashSet();
            var paxGreenCodes = Constants.Application.PaxMessageGreenCode.Split(',').Select(c => c.Trim()).ToHashSet();

            string paxStatus = GetStatus(messageCode, paxGreenCodes, amberCodes, redCodes);
            return paxStatus;
        }

        private string GetStatus(string code, HashSet<string> greenCodes, HashSet<string> amberCodes, HashSet<string> redCodes)
        {
            if (greenCodes.Contains(code))
                return MessageStatusCodeTypes.COMPLETED.ToString();

            if (amberCodes.Contains(code))
                return MessageStatusCodeTypes.INPROGRESS.ToString();

            if (redCodes.Contains(code))
                return MessageStatusCodeTypes.NOTSTARTED.ToString();

            return MessageStatusCodeTypes.NOTSTARTED.ToString();
        }

        public async Task<DeepLinkDetail> GenerateDeeplinkForProxy(string id)
        {
            ArgumentNullException.ThrowIfNull(id);
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/deeplinkdetail/{id}";
            return (await _connector.GetAsync<DeepLinkDetail>(url, _headers, string.Empty)).Response;
        }

        public async Task<DeeplinkResponse> GetDeeplinkUrl(DeeplinkRequest request)
        {
            var url = $"{ConfigReader.Get<string>(CommonGraphql.Constants.Environment.AccountTripMsUrl)}/deeplink";
            return (await _connector.SendAsync<DeeplinkResponse>(url, _headers, string.Empty, HttpMethod.Post, Json.Serialize(request))).Response;
        }
    }
}