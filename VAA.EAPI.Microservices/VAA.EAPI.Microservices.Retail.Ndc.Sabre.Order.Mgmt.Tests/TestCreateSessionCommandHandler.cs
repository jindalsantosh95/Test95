using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class TestCreateSessionCommandHandler : IRequestHandler<CreateSessionCommand, string>
    {
        public Task<string> Handle(CreateSessionCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult("testToken");
        }
    }
}
