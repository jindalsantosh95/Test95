using MediatR;
using VAA.Facade.AzureB2C.Domain.Entities;
using VAA.Facade.AzureB2C.Domain.Model;
using VAA.Facade.AzureB2C.Mapper;

namespace VAA.Facade.AzureB2C.Infrastructure.Repositories
{
    public class EnrolmentRequestRepoQuery : IRequest<Response<EnrolmentResponse>>
    {
        public EnrolmentRequest EnrolmentServieRequest { get; set; }

        public EnrolmentRequestRepoQuery(B2CRequest b2cRequest)
        {
            EnrolmentServieRequest = B2CToEnrolmentRequestMapper.MapB2CRequestToEnrolmentRequest(b2cRequest);
        }
    }
}