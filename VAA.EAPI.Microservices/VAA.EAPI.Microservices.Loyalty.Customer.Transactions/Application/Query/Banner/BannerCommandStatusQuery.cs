using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Banner
{
    public class BannerCommandStatusQuery : CommandStatusQueryBase, IRequest<Result<Response<BannerCommandQueryResponse>>>
    {
    }
}
