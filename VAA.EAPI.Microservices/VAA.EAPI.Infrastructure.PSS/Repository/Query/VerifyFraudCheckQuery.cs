using MediatR;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Query
{
    public class VerifyFraudCheckQuery : IRequest<string>
    {
        public string Pos { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string SessionId { get; set; }

    }
}