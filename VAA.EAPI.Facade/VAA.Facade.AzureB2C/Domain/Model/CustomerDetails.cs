using System;
using System.Collections.Generic;
using System.Text;

namespace VAA.Facade.AzureB2C.Domain.Model
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
   

    public class EmailDetail
    {
        public string EmailId { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public string PreferredFlag { get; set; }
    }

   

    public class ContactDetails
    {
        public List<EmailDetail> EmailDetails { get; set; }        
    }
    
    
    public class Preference
    {
        public string PreferenceType { get; set; }
        public string PreferenceValue { get; set; }
    }

   

    public class CustomerDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }            
        public string CustomerType { get; set; }       
        public ContactDetails ContactDetails { get; set; }
        public List<Preference> Preferences { get; set; }
      
    }


   
}
