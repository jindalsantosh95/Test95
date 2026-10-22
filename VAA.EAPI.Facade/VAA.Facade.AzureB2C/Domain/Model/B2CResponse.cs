using VAA.Facade.AzureB2C.Shared;

namespace VAA.Facade.AzureB2C.Domain.Model
{
    public class B2CResponse
    {
        public B2CResponse()
        {
            this.version = Constants.B2CResponseApiVersion;     //Change the API version in constants whenever there is change in B2C Response
            this.action = Constants.B2CActionContinue;            
        }

        public B2CResponse(string action, string userMessage)
        {
            this.version = Constants.B2CResponseApiVersion;
            this.action = action;
            this.userMessage = userMessage;
            if (action == Constants.B2CActionValidation)
            {
                this.status = "400";
            }
        }

        public string version { get; }
        public string action { get; set; }

        public string userMessage { get; set; }
               
        public string status { get; set; }
        
        public string extension_customerId { get; set; }
        
        public string displayName { get; set; }
        
        public bool extension_staffMember { get; set; }
        
        public string  extension_MemberStatus { get; set; }
    }
}
