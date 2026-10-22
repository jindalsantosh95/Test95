using MediatR;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Command
{
    public class AddRemarksCommand : IRequest<bool>
    {
        public string SessionId { get; set; }
        public string Remarks { get; set; }

    }
}