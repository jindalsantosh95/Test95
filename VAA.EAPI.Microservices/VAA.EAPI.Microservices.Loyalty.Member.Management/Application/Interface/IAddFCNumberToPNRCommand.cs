using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IAddFCNumberToPNRCommand
    {
        Task<string> Handle(AddFCNumberToPNRCommand addFCNumberToPNRCommandRequest, CancellationToken cancellationToken);
    }
}
