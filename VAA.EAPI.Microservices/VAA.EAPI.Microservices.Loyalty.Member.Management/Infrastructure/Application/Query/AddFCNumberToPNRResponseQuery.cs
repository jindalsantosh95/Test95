using MediatR;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query
{
    public class AddFCNumberToPNRResponseQuery : IRequest<AddFCNumberToPNRQueryResponse>
    {
        public string CorrelationId { get; set; }
    }
}
