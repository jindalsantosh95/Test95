using System.Collections.Generic;

namespace VAA.Facade.AzureB2C.Infrastructure.Proxy
{
    public class EnrolmentServiceResponse
    {        
        public EnrolmentSuccessData EnrolmentData { get; set; }
        public List<EnrolmentFailureData> EnrolmentFailedData { get; set; }
    }  
   
}
