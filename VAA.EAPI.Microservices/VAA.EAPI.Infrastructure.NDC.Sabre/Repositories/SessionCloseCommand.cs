using MediatR;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class SessionCloseCommand : IRequest<bool>
    {
        public string BinaryToken { get; set; }
        public string PCC { get; set; }
    }
}
