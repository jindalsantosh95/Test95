using MediatR;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PNRSetupContextRepoQuery : IRequest<string>
    {
        public string RecordLocator { get; set; }
        public bool AssignPrinter { get; set; }

    }
}
