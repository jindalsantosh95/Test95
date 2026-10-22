using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.PSS.Proxy.PassengerV4Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class PSDSSyncRQMapper
    {
        public static UpdateSFPDRequestType UpdateSFPDRequest(Customer request)
        {

            return new UpdateSFPDRequestType()
            {
                BaseRequestInfo = new()
                {
                    ApplicationName = ConfigReader.Get(Constants.Environment.ApplicationName),
                    TransactionId = request.CustId,
                    ApplicationVersion = ConfigReader.Get(Constants.Environment.ApplicationVersion)
                },
                UpdateSFPDRequestInfo = new SFPDInfoType[]{ new SFPDInfoType()
                    {
                        AirlineCode = Constants.Application.VSAirlineCode,
                        DateOfBirth = request.DateOfBirth,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        GenderCode = request.Gender,
                        LoyaltyAccountNumber = request.LoyaltyMemberDetails.MemberNumber,
                        AuthorizationToSave = ConfigReader.Get(Constants.Environment.AuthorizationToSave),
                        CreateUserId= ConfigReader.Get(Constants.Environment.CreateUserID)
                    }
                }
            };
        }
    }
}
