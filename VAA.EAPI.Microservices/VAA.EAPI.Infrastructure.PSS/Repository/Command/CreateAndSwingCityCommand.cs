using MediatR;
using VAA.EAPI.Infrastructure.PSS.Models;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Command
{
    public class CreateAndSwingCityCommand : IRequest<string>
    {
        public Air4RequestContext Air4RequestContext { get; set; }
        public string PointOfSale { get; set; }

    }
}