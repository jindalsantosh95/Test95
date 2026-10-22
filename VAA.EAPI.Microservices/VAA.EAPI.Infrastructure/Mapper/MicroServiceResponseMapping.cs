
using System;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Entity;

namespace VAA.EAPI.Infrastructure.Mapper
{
    public class MicroServiceResponseMapping
    {
        public static ServiceResponse MicroServiceDbToResponseMapper(ServiceResponseDatabaseEntity microServiceResponseDatabaseEntity)
        {
            return new ServiceResponse
            {
                Id = microServiceResponseDatabaseEntity.Id,
                JsonResponseData = microServiceResponseDatabaseEntity.ResponseData,
                Status = (ServiceResponseProcessStatus)Enum.Parse(typeof(ServiceResponseProcessStatus), microServiceResponseDatabaseEntity.Status),
            };
        }
    }
}
