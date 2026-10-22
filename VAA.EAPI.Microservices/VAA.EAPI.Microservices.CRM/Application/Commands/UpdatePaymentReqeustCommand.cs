using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.CRM.Domain;

namespace VAA.EAPI.Microservices.CRM.Application.Commands
{
    public class UpdatePaymentReqeustCommand : PaymentRequest, IRequest<Result<Response<string>>>
    {

    }
}
