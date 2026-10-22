using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Data.Constants;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCArcDocumentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCArcDocumentUpdate
{
    public class ARCDocumentQueryHandler : IARCDocumentQuery
    {
        private readonly IArcDocumentRepoQuery _arcDocumentRepoQuery;

        public ARCDocumentQueryHandler(IArcDocumentRepoQuery arcDocumentRepoQuery)
        {
            _arcDocumentRepoQuery = arcDocumentRepoQuery ?? throw new ArgumentNullException(nameof(arcDocumentRepoQuery));
        }

        public async Task<Response<ARCDocument>> Handle( string orderId, CancellationToken cancellationToken)
        {
            var arcDocuments = await _arcDocumentRepoQuery.Handle( $"{orderId}", cancellationToken);

            if (arcDocuments != null)
            {
                return new Response<ARCDocument>
                {
                    Status = Status.OK,
                    Value = arcDocuments
                };
            }

            return new Response<ARCDocument>
            {
                Status = Status.BusinessException,
                Errors = ErrorMapper.MapBusinessExceptionToEAPIException( new Exception(ErrorCodes.OrderRefErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
            };
        }
    }
}