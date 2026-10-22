using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.CRM.Content.Domain.Content;

namespace VAA.EAPI.Microservices.CRM.Content.Interfaces
{
    public interface IContentRequestHandler
    {
        Task<JToken> Handle(string contentId, CancellationToken cancellationToken);
        Task<JToken> Handle(ContentRequest contentRequest, CancellationToken cancellationToken);
    }
}