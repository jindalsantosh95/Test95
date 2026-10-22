using MediatR;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class CreateSessionCommand: IRequest<string>
    {
        public string PCC { get; set; }
    }
}
