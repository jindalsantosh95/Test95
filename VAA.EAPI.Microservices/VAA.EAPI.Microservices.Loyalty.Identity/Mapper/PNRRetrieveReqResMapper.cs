using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken;
using Environment = VAA.EAPI.Microservices.Loyalty.Identity.Constants.Environment;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class PNRRetrieveReqResMapper
    {
        public static RetrieveRequestType RetrieveRequestMapper(AcquireTokenRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new RetrieveRequestType
            {
                SetupContextRequest = new SetupContextRequestType
                {
                    RequestInfo = new RequestInfoType
                    {
                        ApplicationId = ConfigReader.Get(Environment.ApplicationId),
                        DebugOptions = ConfigReader.Get(Environment.DebugOptions),
                        TestLab = ConfigReader.Get(Environment.TestLab),
                        HostContextParams = new HostContextParamsType
                        {
                            PoolType = ConfigReader.Get(Environment.PoolType)
                        }
                    },
                    RetrieveInfo = new RetrieveInfoType
                    {
                        PassengerInfo = new PassengerInfoType
                        {
                            RecordLocator = request.RLOC
                        }
                    },
                    StampingInfo = new StampingInfoType
                    {
                        UserAuthorizationData = new UserAuthorizationDataType
                        {
                            AgentCityCode = ConfigReader.Get(Environment.AgentCityCode),
                            RoleIndicator = ConfigReader.Get(Environment.RoleIndicator),
                            SecurityId = ConfigReader.Get(Environment.SecurityId)
                        }
                    }
                }
            };
        }

        public static List<Claim> ResponseMapper(RetrieveResponseType serviceResponse, AcquireTokenRequest request)
        {
            var claims = new List<Claim>();

            if (request == null) throw new ArgumentNullException(nameof(request));

            if (serviceResponse != null)
            {
                foreach (var rloc in serviceResponse.TeardownContextResponse.Pnr.RecordLocator)
                {
                    if (serviceResponse.TeardownContextResponse.Pnr.Passenger.Any(x => string.Equals(x.FirstName, request.FirstName, StringComparison.OrdinalIgnoreCase) && string.Equals(x.LastName, request.LastName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var claim = new Claim
                        {
                            RLOC = rloc.RecordLocator1,
                            Type = request.Type,
                            HashKey = Hash.SHA256Managed($"{request.FirstName.ToUpper()}{request.LastName.ToUpper()}"),
                            CreationDate = DateTimeHelper.Air4PNRDateTimeConversion(rloc.PnrCreationDateWithYear, "0000A")?.ToString(Constants.Application.ISODateTimeFormat),
                        };
                        claims.Add(claim);
                    }
                }
            }

            return claims;
        }
    }
}
