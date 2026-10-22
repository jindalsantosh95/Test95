using MediatR;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery : IRequest<Customer>
    {
        public Customer Customer { get; set; }
        public string SessionId { get; set; }
        public bool RetrieveFlag { get; set; }
    }
}
