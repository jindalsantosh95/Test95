using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;

public class VerifyEmailCommand : IRequest<Response<bool>>
{
    public string ObjectId { get; set; }
    public CreateCustomerEntities CustomerDetails { get; set; }
}