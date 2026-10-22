using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public interface ICreateTokenCommand
    {
        Task<string> Handle(string pcc);
    }
}
