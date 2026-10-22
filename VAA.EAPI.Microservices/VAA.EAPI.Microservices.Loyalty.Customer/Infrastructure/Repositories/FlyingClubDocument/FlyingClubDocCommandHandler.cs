using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.FlyingClubDocCommand
{
    public class FlyingClubDocCommandHandler : IRequestHandler<FlyingClubDocCommand, Result<Unit>>
    {
        public ICustomerTransactionService<FlyingClubMemberDTO> _customerService;
        public readonly ILogger<FlyingClubDocCommandHandler> _logger;

        public FlyingClubDocCommandHandler(ICustomerTransactionService<FlyingClubMemberDTO> customerService, ILogger<FlyingClubDocCommandHandler> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(FlyingClubDocCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _customerService.ReplaceItemAsync(new FlyingClubMemberDTO(request.MemberProfileDoc), request?.MemberProfileDoc?.CustomerId, DocumentType.FlyingClubMember);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Update FlyingClubMember failed for CustomerId :{request?.MemberProfileDoc?.CustomerId} with message :{ex.Message}");
                throw;
            }

            return Unit.Value;
        }
    }
}
