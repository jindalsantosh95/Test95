using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using VAA.EAPI.Common.Serialisation;
using VAA.Facade.AzureB2C.Shared;

namespace VAA.Facade.AzureB2C.Domain.Model
{
    public class B2CRequest : DynamicObject
    {
       
            // The inner dictionary.
            Dictionary<string, object> dictionary
                = new Dictionary<string, object>();

            // This property returns the number of elements
            // in the inner dictionary.
            public int Count
            {
                get
                {
                    return dictionary.Count;
                }
            }

        public string Step
        {
            get
            {
                object result;
                dictionary.TryGetValue("step", out result);
                return result.ToString();
            }
        }

        [JsonProperty(PropertyName = "identities")]
        public Identity[] Identities
        {
            get
            {
                object result;
                dictionary.TryGetValue("identities", out result);
                string resultJson = Json.Serialize(result);
                var identities = Json.DeserializeObject<List<Identity>>(resultJson);                
                return identities.ToArray();

            }
        }

        // If you try to get a value of a property
        // not defined in the class, this method is called.
        public override bool TryGetMember(
            GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            string name = binder.Name.ToLower();

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            if (name.Contains("marketpreferences"))
            {
                string extPropertyName = "extension_" + ReadConfig.GetConfigurationSettings(Constants.B2CAppId).Replace("-","") + "_marketpreferences";
                return dictionary.TryGetValue(extPropertyName, out result);
            }
            else if (name.Contains("customerId"))
            {
                string extPropertyName = "extension_" + ReadConfig.GetConfigurationSettings(Constants.B2CAppId).Replace("-", "") + "_customerId";
                return dictionary.TryGetValue(extPropertyName, out result);
            }            
            else
                return dictionary.TryGetValue(name, out result);
        }

            // If you try to set a value of a property that is
            // not defined in the class, this method is called.
            public override bool TrySetMember(
                SetMemberBinder binder, object value)
            {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            if (binder.Name.ToLower().Contains("marketpreferences"))
            {
                string extPropertyName = "extension_" + ReadConfig.GetConfigurationSettings(Constants.B2CAppId).Replace("-", "") + "_marketpreferences";
                dictionary[extPropertyName.ToLower()] = value;
            }
            else if (binder.Name.ToLower().Contains("customerid"))
            {
                string extPropertyName = "extension_" + ReadConfig.GetConfigurationSettings(Constants.B2CAppId).Replace("-", "") + "_customerId";
                dictionary[extPropertyName.ToLower()] = value;
            }
            else
                dictionary[binder.Name.ToLower()] = value;

                // You can always add a value to a dictionary,
                // so this method always returns true.
                return true;
            }
        
    }
   
}
