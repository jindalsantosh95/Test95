using MediatR;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class IgnoreTransactionCommand : IRequest<bool>
    {
        public string BinarySecurityToken { get; set; }
        public string PCC { get; set; }
    }
}
