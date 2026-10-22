using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;

namespace VAA.EAPI.GraphQL.Infrastructure.Interfaces
{
    public interface IPDMProvider
    {
        Task<VSFlownDetailsRespone> PDMGetFlownDetails(string ticketNumber);
        Task<string> UpdateTicketStatus(TicketStatusRequest ticketstatusRequest);
    }
}
