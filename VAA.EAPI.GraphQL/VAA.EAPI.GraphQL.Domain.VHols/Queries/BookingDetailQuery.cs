using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System.Text.RegularExpressions;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Domain.VHols.Application;
using VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse;
using VAA.EAPI.GraphQL.Domain.VHols.Models.VHolsQueryAPIDTO;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.VHols.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class BookingDetailQuery
    {
        private readonly IHttpConnector _connector;

        public BookingDetailQuery(IHttpConnector connector)
        {
            _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }

        [UseFiltering]
        [UseSorting]
        [GraphQLName("vholsGetBookingDetail")]
        [GraphQLDescription("Retrieves the information for the booking referenced in the request's Authorization token (provided by the 'Login' request)")]
        public async Task<BookingDetail> getBookingDetail(bool? refresh, IResolverContext context)
        {
            // Extract Auth token from Request context headers 
            var token = AsyncContext.Current.Context.Features.Get<JwtPrincipalFeature>().AccessToken.Token;

            // Re-construct a GraphQL request from the inbound request
            var query = Regex.Replace(AsyncContext.Current.Context.BindingContext.BindingData.GetValueOrDefault("query").ToString(), "[\r\n]", " ");

            // As we are simply passing the request on to VHols Query API, we need to rename the operation
            query = Regex.Replace(query, "vholsGetBookingDetail", "getBookingDetail");

            var variables = Regex.Replace(AsyncContext.Current.Context.BindingContext.BindingData.GetValueOrDefault("variables").ToString(), "[\r\n]", " ");

            var content = $"{{ \"query\": \"{query}\", \"variables\": {variables} }}";

            // Fire off the request to VHols Query API
            var response = await _connector.SendAsync<string>(ConfigReader.Get(Common.Constants.Environment.VHolsQueryAPIRetrieve), new Dictionary<string, string>(), token, HttpMethod.Post, content);

            // Custom De-serialization as VH Query API has variable child elements of a list.
            var booking = Newtonsoft.Json.JsonConvert.DeserializeObject<BookingDTO>(response.RawResponse, new HolidayComponentConverter());

            // Surface the errors reported by VH MMB Query API
            if (booking.Errors is not null)
            {
                foreach (Models.VHolsQueryAPIDTO.Error error in booking.Errors)
                {
                    var eb = ErrorBuilder.New().SetMessage("VH MMB Query API Reported Error");

                    foreach (var f in error.GetType().GetProperties())
                        eb.SetExtension(f.Name, f.GetValue(error, null));

                    context.ReportError(eb.Build());
                }
            }

            return booking.Data.getBookingDetail;
        }
    }
}