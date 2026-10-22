using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.CheckPaymentEligibility;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.ConfirmBaggage;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.SaveBags;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.UpdateBags;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Interface
{
    public interface IBaggageService
    {
        Task<Result<Response<BaggageAllowanceResponse>>> GetBaggageAllowance(string travelPartyId, bool hidePaidBaggage);
        Task<Result<Response<SaveBaggageResponse>>> SaveBagsAsync(SaveBaggageRequest request);
        Task<Result<Response<CheckPaymentResponse>>> CheckPaymentAsync(CheckPaymentRequest request);
        Task<Result<Response<ConfirmBaggageResponse>>> ConfirmBaggageAsync(ConfirmBaggageRequest request);


    }
}
