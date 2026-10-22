using System.Collections.Generic;
using VAA.Facade.AzureB2C.Domain.Model;
using VAA.Facade.AzureB2C.Domain.Entities;
using VAA.Facade.AzureB2C.Shared;

namespace VAA.Facade.AzureB2C.Mapper
{
    class EnrolmentErrorCodeMapping
    {
        public static B2CResponse MapEnrolmentErrorToB2CResponse(List<FailureResponse> failureResponses)
        {
            string errorMessages = "Miscellaneous technical error.", action = Constants.B2CActionBlockPage;  

            foreach (var error in failureResponses)
            {
                if (error.Code == "G012")
                {
                    errorMessages = "Underlying service expected more information.";
                    action = Constants.B2CActionValidation;
                }                   
                else if(error.Code == "1011" || error.Code == "1012" || error.Code == "1013")
                {
                    errorMessages = "Please make sure to use unique emailId.";
                    action = Constants.B2CActionValidation;
                }
                else
                {
                    errorMessages = "Configuration missing for underlying service. Please Contact Service Owner.";                    
                }
               
            }
            return new B2CResponse(action, errorMessages);

        }
    }
}
