using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Infrastructure.Email.Entities;

namespace VAA.EAPI.Infrastructure.Email.Mapper
{
    public class SendEmailToCCPRequestMapper
    {
        public static CcpRequest MapRequest(SendEmailRequest request)
        {
            var data = Json.Deserialize<DataFields>(request.DataFields.ToString());
            return new CcpRequest()
            {
                CustomerId = data.Membership_Number,
                EmailAddress = request?.To,
                FirstName = data.Customer_FirstName,
                LastName = data.Customer_LastName,
                Channel = ConfigReader.Get(Constants.Constants.Ccpchannel),
                FromAddress = ConfigReader.Get(Constants.Constants.CcpfromAddress),
                LanguageCode = Constants.Constants.NotApplicable,
                Template = string.IsNullOrEmpty(request.Template) ? ConfigReader.Get(Constants.Constants.Ccptemplate) : request.Template.Trim(),
                MessageData = new()
                {
                    Membership_Number = data.Membership_Number,
                    Member_First_Name = data.Member_First_Name,
                    Member_Name = (data.Customer_FirstName != null ? data.Customer_FirstName : data.Member_First_Name) + " " + data.Customer_LastName,
                    Mileage_Balance = "0",
                    Mileage_Balance_As_Of_Date = "0",
                    Mileage_Expiry_Date = "0",
                    Tier = data.Tier,
                    Territory = data.Territory,
                    Virgin_Points = data.Virgin_Points,
                    Child_First_Name = data.Child_First_Name,
                    Child_Membership_Number = data.Child_Membership_Number
                }
            };
        }
    }
}
