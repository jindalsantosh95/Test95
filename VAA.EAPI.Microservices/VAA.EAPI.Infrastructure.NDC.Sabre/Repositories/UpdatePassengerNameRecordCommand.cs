using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class UpdatePassengerNameRecordCommand : IRequest<UpdatePassengerNameRecordRQResponse>
    {
        public UpdatePassengerNameRecordRQRequest UpdatePassengerNameRecordRQRequest { get; set; }

    }
}
