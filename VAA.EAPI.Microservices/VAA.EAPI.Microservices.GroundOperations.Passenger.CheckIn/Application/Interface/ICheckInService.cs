using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.BoardingPass;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ContactTracing;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.EmergencyContact;
using VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.ReturnDate;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Application.Interface
{
    public interface ICheckInService
    {
        Task<Result<Response<CheckInResponse>>> InitiateCheckInAsync(CheckInRequest checkInRequest);
        Task<Result<Response<CheckInResponse>>> GetCheckInAsync(string travelPartyId);
        Task<Result<Response<CheckInResponse>>> SelectPassengersAsync(CheckIn.Domain.SelectPassengerRequest selectPassengerRequest);
        Task<Result<Response<CheckInResponse>>> SavePassportAsync(SavePassportRequest savePassportRequest);
        Task<Result<Response<CheckInResponse>>> SaveContactTracingAsync(ContactTracingRequest contactTracingRequest);
        Task<Result<Response<CheckInResponse>>> SaveEmergencyContactDetailsAsync(EmergencyContactRequest emergencyContactRequest);
        Task<Result<Response<CheckInResponse>>> SaveReturnDateAsync(ReturnDateRequest returnDateReq);
        Task<Result<Response<Domain.BoardingPass.BoardingPassResponse>>> BoardingPassAsync(BoardingPassRequest request);
        Task<Result<Response<Domain.BoardingPass.BoardingPassResponse>>> RetrieveBoardingPass(BoardingPassRequest request);
        Task<bool> LogTransactionSummaryAsync(TransactionSummaryRequest request);

    }
}
