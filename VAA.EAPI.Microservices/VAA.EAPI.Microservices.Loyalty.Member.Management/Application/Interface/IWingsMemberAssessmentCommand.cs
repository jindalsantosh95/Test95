using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.WingsMemberAssessment;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface
{
    public interface IWingsMemberAssessmentCommand
    {
        Task<string> Handle(WingsMemberAssessmentCommand request, CancellationToken cancellationToken);
    }
}
