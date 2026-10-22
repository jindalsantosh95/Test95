using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class GetProductRefrenceDetailsQuery : IRequest<Product>
    {
        public string ProductName { get; set; }
    }

    public class GetProductRefrenceDetailsQueryHandler : IRequestHandler<GetProductRefrenceDetailsQuery, Product>
    {
        private readonly ICommonRefDBServices _commonRefDBService;
        private readonly ILogger<GetProductRefrenceDetailsQueryHandler> _logger;
        public GetProductRefrenceDetailsQueryHandler(ICommonRefDBServices commonRefDBService, ILogger<GetProductRefrenceDetailsQueryHandler> logger)
        {
            _commonRefDBService = commonRefDBService ?? throw new ArgumentNullException(nameof(commonRefDBService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Product> Handle(GetProductRefrenceDetailsQuery request, CancellationToken cancellationToken)
        {

            try
            {
                _logger.LogInformation($"Get Birthday Bonus Product Query Handler - Start");

                return await _commonRefDBService.GetProductReferenceDataByName(request.ProductName);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Get Birthday Bonus Product Query Handler failed due to Exception");
                throw;
            }
            finally
            {
                _logger.LogInformation($"Get Birthday Bonus Product Query Handler - End");
            }
        }
    }
}
