using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse;
using VAA.EAPI.GraphQL.Domain.VHols.Models.ForgotMyBooking;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.VHols.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ForgotMyBookingQuery
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _connector;
        private readonly ILogger<Customer> _logger;

        public ForgotMyBookingQuery(IOAuthTokenProvider tokenProvider, IHttpConnector connector, ILogger<Customer> logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// LostMyBooking: If a VHols customer cannot recall their booking reference, they can supply their surname, dob and departure data and it
        /// will send an email with the details to the booking reference to the email address registered on the booking.
        /// 
        /// This is a Fire and Forget query, if the request does not fail, then we'll send an OK ... If it does fail, the error will be propogated
        /// </summary>
        /// <param name="surname">Lead passenger's surname</param>
        /// <param name="dateOfBirth">Lead passenger's dob</param>
        /// <param name="departureDate">Start of holiday</param>
        /// <returns>Simple OK ... Or an error object</returns>
        [UseFiltering]
        [UseSorting]
        [GraphQLDescription("Send a 'recovery' email to the registered email account for the requested booking")]
        [GraphQLName("vholsForgotMyBooking")]
        public async Task<FMBResponse> ForgotMyBooking(string surname, DateTime dateOfBirth, DateTime departureDate)
        {
            //********************//
            //   MOCK RESPONSE    //
            //********************//
            return new FMBResponse()
            {
                Status = Models.enums.FMBResponseStatus.OK
            };

            /*
            _ = await _connector.SendAsync<string>(ConfigReader.GetConfigurationSettings(Common.Constants.Environment.VHolsQueryAPIFMB),
                                                   new Dictionary<string, string>(),
                                                   string.Empty,
                                                   HttpMethod.Post,
                                                   Common.Serialisation.Json.Serialize(new FMBRequest() { Surname = surname, DateOfBirth = dateOfBirth, DepartureDate = departureDate }, false));

            return new FMBResponse() { Status = Models.enums.FMBResponseStatus.OK };
            */
        }
    }
}
