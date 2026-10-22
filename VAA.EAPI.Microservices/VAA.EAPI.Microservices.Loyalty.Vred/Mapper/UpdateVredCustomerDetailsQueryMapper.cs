
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.CustomerDetail;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Mapper
{
    public class UpdateVredCustomerDetailsQueryMapper
    {
        public static Response<UpdateVredCustomerDetailsQueryResponse> ResponseMapper(ServiceResponse serviceResponse)
        {
            var apiStatus = serviceResponse.Status == ServiceResponseProcessStatus.Completed ? Status.OK : Status.Error;
            return new Response<UpdateVredCustomerDetailsQueryResponse>
            {
                Status = apiStatus,
                Value = new UpdateVredCustomerDetailsQueryResponse
                {
                    Id = serviceResponse.Id,
                    JsonResponseData = serviceResponse.JsonResponseData,
                    ProcessingStatus = serviceResponse.Status.ToString(),
                    Type = serviceResponse.Type

                }

            };
        }
    }
}
