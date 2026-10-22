using MediatR;
using VAA.EAPI.Domain.Virgin.Models.Customer;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Command
{
    public class PSDSSyncRepoCommand : IRequest<bool>
    {
        public Customer UpdateSFPDRequest { get; set; }
    }
}
