using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Command
{
    public class AddRemarksCommandHandler : IRequestHandler<AddRemarksCommand, bool>
    {
        private readonly IAir4RemarksV3 _iAir4RemarksV3;
        public AddRemarksCommandHandler(IAir4RemarksV3 iRemarksV3)
        {
            _iAir4RemarksV3 = iRemarksV3 ?? throw new ArgumentNullException(nameof(iRemarksV3));

        }
        public async Task<bool> Handle(AddRemarksCommand addRemarksCommand, CancellationToken cancellationToken)
        {
            await _iAir4RemarksV3.AddSpecialRemarkRequest(RemarksV3Mapper.MapSpecialRemarkRequest(addRemarksCommand));
            return true;
        }
    }

}