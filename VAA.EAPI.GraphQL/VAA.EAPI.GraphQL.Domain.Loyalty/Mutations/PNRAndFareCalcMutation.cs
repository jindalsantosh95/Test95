using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using System;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Mappers;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class PNRAndFareCalcMutation
    {
        private readonly IPNRQueryProvider _pnrQueryProvider;
        private readonly ILoyaltyMemberMgtProvider _loyaltyMemberMgtProvider;

        public PNRAndFareCalcMutation(IPNRQueryProvider pnrQueryProvider, ILoyaltyMemberMgtProvider loyaltyMemberMgtProvider)
        {
            _pnrQueryProvider = pnrQueryProvider ?? throw new ArgumentNullException(nameof(pnrQueryProvider));
            _loyaltyMemberMgtProvider = loyaltyMemberMgtProvider ?? throw new ArgumentNullException(nameof(pnrQueryProvider));
        }

        [Authorize(Roles = new[] { "Loyalty.PNR.FareCalc" })]
        [GraphQLName("loyaltyPNRAndFareCalculation")]
        [GraphQLDescription("Calculate points")]
        public async Task<PNRAndFareMutationResponse> PNRAndFareCalculation(PNRAndFareMutationRequest pointRequest)
        {
            try
            {
                //Retrieve PNR
                var pnrServiceResonse = await _pnrQueryProvider.RetrievePNR(pointRequest.Pnr);

                if (pnrServiceResonse.PnrType is null)
                    throw new RequestValidationException("PNR Retrieval Failure");

                RewardBookingPointCalculationRequest request = new RewardBookingPointCalculationRequest
                {
                    Passengers = pnrServiceResonse.PnrType.Passengers,
                    InfantInArms = pnrServiceResonse.PnrType.InfantInArms,
                    DtiSegmentInfos = pnrServiceResonse.PnrType.DtiSegmentInfos
                };

                var pointResponse = await _loyaltyMemberMgtProvider.RetrieveRewardBookingPoints(request);

                if (pointResponse is null)
                    throw new RequestValidationException("Loyalty Point Calculation Failure");

                return RewardPointCalcMapper.MapToPointCalcResponse(pointResponse, pnrServiceResonse.PnrType, pointRequest.Pnr);
            }
            catch (HttpRequestException ex)
            {

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.FirstOrDefault().Code == Constants.Application.PNRNotFoundErrorCode)
                {
                    var builder = ErrorBuilder.New().SetMessage("PNR Retrieve Failed");
                    foreach (var error in errorDetails)
                    {
                        builder.SetExtension("code", error.Code);
                        builder.SetExtension("message", error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    };

                    throw new GraphQLException(builder.Build());
                }

                throw;
            }
        }
    }
}
