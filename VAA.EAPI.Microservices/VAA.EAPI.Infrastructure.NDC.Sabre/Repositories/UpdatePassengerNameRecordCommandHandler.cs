using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdatePassengerNameRecord;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class UpdatePassengerNameRecordCommandHandler : IRequestHandler<UpdatePassengerNameRecordCommand, UpdatePassengerNameRecordRQResponse>
    {
        private readonly IUpdatePassengerNameRecord _updatePassengerNameRecord;

        public UpdatePassengerNameRecordCommandHandler(IUpdatePassengerNameRecord updatePassengerNameRecord)
        {
            _updatePassengerNameRecord = updatePassengerNameRecord ?? throw new ArgumentException(nameof(updatePassengerNameRecord));
        }

        public async Task<UpdatePassengerNameRecordRQResponse> Handle(UpdatePassengerNameRecordCommand request, CancellationToken cancellationToken)
        {
            return await _updatePassengerNameRecord.UpdatePassengerNameRecordRQAsync(request.UpdatePassengerNameRecordRQRequest);
        }
    }
}
