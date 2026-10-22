using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredConsent;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.ManageAccount
{
    public class VredConsentReceiptCommand : ConsentReceiptRequest, IRequest<Result<Response<string>>>
    {

    }
}
