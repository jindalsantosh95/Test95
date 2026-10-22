using System.Collections.Generic;
using VAA.Facade.AzureB2C.Domain.Model;
using VAA.Facade.AzureB2C.Shared;

namespace VAA.Facade.AzureB2C.Mapper
{
    class B2CToEnrolmentRequestMapper
    {
        public static EnrolmentRequest MapB2CRequestToEnrolmentRequest(B2CRequest req)
        {
            dynamic data = req;

            CustomerDetails customerDetails = new CustomerDetails();
            customerDetails.FirstName = data.givenName;
            customerDetails.LastName = data.surname;
            customerDetails.CustomerType = Constants.BasicCustomerType;            
           
            /*        ================== Park Marketing Preferences
            customerDetails.Preferences = new List<Preference>() { new Preference() { PreferenceType = Constants.MarketingPreferences, PreferenceValue = data.extension_marketpreferences ? "Y" : "N" } };
            */
            ContactDetails contactDetails = new ContactDetails();
            contactDetails.EmailDetails = new List<EmailDetail> {
                                                                                                new EmailDetail()
                                                                                                {  
                                                                                                    EmailId = data.email,
                                                                                                    PreferredFlag = "Y" 
                                                                                                } 
                                                                                           };           
            customerDetails.ContactDetails = contactDetails;

            return new EnrolmentRequest
            {
                CustomerDetails = customerDetails                
            };
        }
    }
}
