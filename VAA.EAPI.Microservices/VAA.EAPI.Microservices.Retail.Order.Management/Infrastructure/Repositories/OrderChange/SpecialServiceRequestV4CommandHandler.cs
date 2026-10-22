using MediatR;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.SpecialServiceRequestV4Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Constants;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Repositories.OrderChange
{
    public class SpecialServiceRequestV4CommandHandler : IRequestHandler<SpecialServiceRequestV4Command, AddResponseType>
    {
        private readonly ISpecialServiceRequestV4 _specialServiceRequestV4;
        public SpecialServiceRequestV4CommandHandler(ISpecialServiceRequestV4 iSpecialServiceRequestV4)
        {
            _specialServiceRequestV4 = iSpecialServiceRequestV4 ?? throw new ArgumentNullException(nameof(iSpecialServiceRequestV4));
        }
        public async Task<AddResponseType> Handle(SpecialServiceRequestV4Command request, CancellationToken cancellationToken)
        {
            try
            {
                var specialServiceRequestV4Request = SpecialServiceRequestV4ReqResMapper.AddRequestV4ReqMapper(request.SessionId, request.TransactionId, request.Location, request.specialServiceRequestInfo, request.ChannelInfo);
                return _specialServiceRequestV4.AddSSRsRequest(specialServiceRequestV4Request);
            }
            catch (FaultException e)
            {
                throw Air4FaultExceptionChecker.SpecialServiceRequestV4Exception(e, ErrorCodes.DeltaErrorCode);
            }
        }
    }
}
