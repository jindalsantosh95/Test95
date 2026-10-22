using System;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Mapper
{
    public static class CurrencyConversionRequestMapper
    {
        public static string MapCurrencyConversionRequestToXml()
        {
            var request = new ExchangeRateRequestWrapper
            {
                Request = new ExchangeRateRequest
                {
                    callingApplication = "EnterpriseAPI", 
                    transactionID = string.Empty,
                    valueToConvert = "1",
                    originalCurrencyISOCode = "USD",
                    originalCurrency = string.Empty,
                    targetCurrencyISOCode = "GBP",
                    targetCurrency = string.Empty,
                    exchangeRateType = "ML", 
                    exchangeRateDate = DateTime.UtcNow.ToString("yyyy-MM-dd")
                }
            };

            string xmlBody = Xml.Serialize(request);

            xmlBody = xmlBody.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>", "");

            return $@"
            <soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:web=""http://www.virgin-atlantic.com/webservices/"">
                <soap:Header/>
                    <soap:Body>
                        <web:ConvertCurrency>
                            <web:xmlRequest>
                                {xmlBody}
                            </web:xmlRequest>
                        </web:ConvertCurrency>
                    </soap:Body>
            </soap:Envelope>";
        }

    }
}
