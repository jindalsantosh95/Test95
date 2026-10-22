using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.VSRetro
{
    public class ValidateVSClaimInputsCommand : IRequest<Result<Response<ValidateVSClaimInputsResponse>>>
    {
        public ValidateVSClaimInputsCommand(ValidateVSClaimInputsRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            TicketDesignator = request.TicketDesignator;
            FareBasis = request.FareBasis;
            PassengerType = request.PassengerType;
        }

        public string? TicketDesignator { get; }

        public string? FareBasis { get; }

        public string? PassengerType { get; }
    }
}
