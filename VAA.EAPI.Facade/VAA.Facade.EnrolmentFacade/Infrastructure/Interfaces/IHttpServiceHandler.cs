using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Application.Command;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces
{
    public interface IHttpServiceHandler
    {
        Task<HttpResponseMessage> Handle(HttpServiceCommand request, CancellationToken cancellationToken);
    }
}
