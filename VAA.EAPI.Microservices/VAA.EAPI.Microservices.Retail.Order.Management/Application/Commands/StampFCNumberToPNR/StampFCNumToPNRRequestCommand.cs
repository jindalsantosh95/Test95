
using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.StampFCNumberToPNR;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Application.Commands.StampFCNumberToPNR
{
    public class StampFCNumToPNRRequestCommand : StampFCNumToPNRRequest, IRequest<Result<Response<string>>>
    {
    }
}
