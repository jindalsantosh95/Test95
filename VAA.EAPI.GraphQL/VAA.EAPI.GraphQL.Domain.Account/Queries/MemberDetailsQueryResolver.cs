using HotChocolate.Types;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Models.Activity;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    public class MemberDetailsQueryResolver : ObjectType<MemberDetailsResolverResponse>
    {
        private readonly IVoucherQueryProvider _voucherProvider;
        private readonly IFlightQueryProvider _flightProvider;
        private readonly IActivityQueryProvider _activityProvider;
        private readonly ITokenB2CProvider<string> _tokenB2CProvider;
        private readonly IAccountCustomerService _customerProvider;

        public MemberDetailsQueryResolver(IVoucherQueryProvider voucherQueryProvider, IFlightQueryProvider flightQueryProvider, IActivityQueryProvider activityQueryProvider, ITokenB2CProvider<string> tokenB2CProvider, IAccountCustomerService customerProvider)
        {
            _voucherProvider = voucherQueryProvider ?? throw new ArgumentNullException(nameof(voucherQueryProvider)); ;
            _flightProvider = flightQueryProvider ?? throw new ArgumentNullException(nameof(flightQueryProvider)); ;
            _activityProvider = activityQueryProvider ?? throw new ArgumentNullException(nameof(activityQueryProvider)); ;
            _tokenB2CProvider = tokenB2CProvider ?? throw new ArgumentNullException(nameof(tokenB2CProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
        }

        protected override void Configure(IObjectTypeDescriptor<MemberDetailsResolverResponse> descriptor)
        {
            descriptor.Field(fld => fld.VoucherResponse)
                      .Resolve(async (cntx, ct) =>
                      {
                          try
                          {
                              var vouchers = await _voucherProvider.GetVoucherQuery((await GetClaim())?.CustomerId, Common.Constants.Application.PageSizeResolver, cntx.Parent<MemberDetailsResolverResponse>().VouchersCount);

                              return new VoucherResponse { VoucherDetails = vouchers };
                          }
                          catch (HttpRequestException ex)
                          {
                              return new VoucherResponse
                              {
                                  ErrorResponse = HandleError(ex.Message, "Voucher", ex.StatusCode.HasValue ? ((int)ex.StatusCode.Value).ToString() : Constants.Application.VouchersError)
                              };
                          }
                          catch (Exception ex)
                          {
                              return new VoucherResponse
                              {
                                  ErrorResponse = HandleError(ex.Message, "Voucher", Constants.Application.VouchersError)
                              };

                          }
                      });

            descriptor.Field(fld => fld.MemberVoucherResponse)
                      .Resolve(async (cntx, ct) =>
                      {
                          try
                          {
                              var vouchers = await _voucherProvider.GetAccountMemberVouchers((await GetClaim())?.CustomerId, Common.Constants.Application.PageSizeResolver, cntx.Parent<MemberDetailsResolverResponse>().VouchersCount);

                              return new MemberVoucherResponse { MemberVoucherDetails = vouchers };
                          }
                          catch (HttpRequestException ex)
                          {
                              return new MemberVoucherResponse
                              {
                                  ErrorResponse = HandleError(ex.Message, "Member Voucher", ex.StatusCode.HasValue ? ((int)ex.StatusCode.Value).ToString() : Constants.Application.VouchersError)
                              };
                          }
                          catch (Exception ex)
                          {
                              return new MemberVoucherResponse
                              {
                                  ErrorResponse = HandleError(ex.Message, "Member Voucher", Constants.Application.VouchersError)
                              };

                          }
                      });

            descriptor.Field(fld => fld.FlightDetails)
                     .Resolve(async (cntx, ct) =>

                     {
                         try
                         {
                             var flightSegments = await _flightProvider.GetFlightsQueryData((await GetClaim())?.FcNumber, Common.Constants.Application.PageSizeResolver, cntx.Parent<MemberDetailsResolverResponse>().FlightsCount);
                             return new FlightDetailsResponse { FlightSegmentDetails = flightSegments?.FlightSegmentDetails };
                         }
                         catch (HttpRequestException ex)
                         {
                             return new FlightDetailsResponse
                             {
                                 ErrorResponse = HandleError(ex.Message, "Flights", ex.StatusCode.HasValue ? ((int)ex.StatusCode.Value).ToString() : Constants.Application.FlightsError)
                             };
                         }
                         catch (Exception ex)
                         {
                             return new FlightDetailsResponse
                             {
                                 ErrorResponse = HandleError(ex.Message, "Flights", Constants.Application.FlightsError)
                             };
                         }
                     });

            descriptor.Field(fld => fld.ActivityResponse)
                     .Resolve(async (cntx, ct) =>
                     {
                         try
                         {
                             return new ActivityResponse { ActivityDetails = await _activityProvider.GetActivitiesQueryData((await GetClaim()), Common.Constants.Application.PageSizeResolver, cntx.Parent<MemberDetailsResolverResponse>().ActivitiesCount) };
                         }
                         catch (HttpRequestException ex)
                         {
                             return new ActivityResponse
                             {
                                 ErrorResponse = HandleError(ex.Message, "Activities", ex.StatusCode.HasValue ? ((int)ex.StatusCode.Value).ToString() : Constants.Application.ActivityError)
                             };
                         }
                         catch (Exception ex)
                         {
                             return new ActivityResponse
                             {
                                 ErrorResponse = HandleError(ex.Message, "Activities", Constants.Application.ActivityError)
                             };
                         }
                     });
        }

        private async Task<TokenB2CResponse> GetClaim()
        {
            var jwtPrincipalFeature = AsyncContext.Current.Context.GetJwtPrincipalFeature();

            if (string.IsNullOrWhiteSpace(jwtPrincipalFeature?.AccessToken?.Token))
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            var tokenResponse = await _tokenB2CProvider.ValidateTokenAsync(jwtPrincipalFeature.AccessToken.Token);

            var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse);

            if (claim is null)
                throw new InvalidOperationException("Token validation response could not be deserialized to TokenB2CResponse.");

            if (string.IsNullOrEmpty(claim.CustomerId))
                claim.CustomerId = (await _customerProvider.GetCustomer(claim.CustomerId, claim.FcNumber))?.Customer?.CustomerId;

            return claim;
        }

        private ErrorResponse HandleError(string message, string entityType, string code)
        {
            return new ErrorResponse()
            {
                message = $"Unable to retrieve {entityType}",
                extentions = new()
                {
                    code = code,
                    exception = message
                }
            };
        }
    }
}
