using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore
{
    public sealed class SvcOrderQueryHandler(ISvcOrderRepository repository) : ISvcOrderQuery
    {
        private readonly ISvcOrderRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<Response<SvcOrderValidationResponse>> HandleAsync(string pnr, string firstName, string lastName, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pnr);

            var orders = await _repository.GetByPnrAsync(pnr.Trim().ToUpperInvariant(), cancellationToken);
            var customers = orders.SelectMany(order => order.CustomerDetails ?? []).ToList();
            var isGroup = customers.Any(customer => customer.Bookingtype?.Trim().Is("group") == true);
            var normalisedFirstName = firstName?.Trim() ?? string.Empty;
            var normalisedLastName = lastName?.Trim() ?? string.Empty;
            var hasFirstName = !normalisedFirstName.IsNullOrWhiteSpace();
            var hasLastName = !normalisedLastName.IsNullOrWhiteSpace();
            var firstNameMatches = !isGroup && hasFirstName && customers.Any(customer => customer.FirstName?.Trim().Is(normalisedFirstName) == true);
            var surnameMatches = !isGroup && hasFirstName && hasLastName && customers.Any(customer => customer.FirstName?.Trim().Is(normalisedFirstName) == true && customer.LastName?.Trim().Is(normalisedLastName) == true);

            return new Response<SvcOrderValidationResponse>
            {
                Status = Status.OK,
                Value = new SvcOrderValidationResponse
                {
                    Pnr = orders.Count != 0,
                    IsGroup = isGroup,
                    FirstName = firstNameMatches,
                    Surname = surnameMatches
                }
            };
        }
    }
}
