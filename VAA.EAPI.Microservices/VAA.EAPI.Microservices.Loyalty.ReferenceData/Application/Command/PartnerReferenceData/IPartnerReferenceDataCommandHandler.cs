using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;


namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.PartnerReferenceData
{
    public interface IPartnerReferenceDataCommandHandler
    {
        Task<Result<string>> Handle(PartnerReferenceDataCommand request, CancellationToken cancellationToken);
    }
}
