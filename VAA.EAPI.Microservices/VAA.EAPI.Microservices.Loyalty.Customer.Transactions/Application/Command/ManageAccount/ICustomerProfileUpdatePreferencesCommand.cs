using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount
{
    public interface ICustomerProfileUpdatePreferencesCommand
    {
        Task<UpdateMarketingPreferenceResponse> Handle(CustomerProfileCommand request, CancellationToken cancellationToken);
    }
}
