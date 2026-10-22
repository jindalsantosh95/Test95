using System.Xml;

namespace VAA.EAPI.Automation.NDC.Tests.EndToEnd
{
    public class Assertions
    {
        public static bool ValidateCurrencyCodeOfOffersReturned(string response, string expCurrCode, out string message)
        {
            message = string.Empty;
            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            responseXmlDocument.LoadXml(response);

            //Currency Code validation for Total Amount in offers
            XmlNodeList Offers_TotalAmount_CurrCode = responseXmlDocument.GetElementsByTagName("TotalAmount");

            foreach (XmlNode xn_offerTotal in Offers_TotalAmount_CurrCode)
            {
                string curr = xn_offerTotal.Attributes["CurCode"].Value;
                if (!curr.Equals(expCurrCode))
                {
                    message = $"Currency code for Total Amount in offers is not as expected currency {expCurrCode} and actual currency {curr} ";
                    return false;
                }
            }

            // Currency Code validation for Base Amount in Offer Items         
            XmlNodeList OfferItem_BaseAmount_CurrCode = responseXmlDocument.GetElementsByTagName("BaseAmount");
            foreach (XmlNode xn_offerBaseAmount in OfferItem_BaseAmount_CurrCode)
            {
                string curr = xn_offerBaseAmount.Attributes["CurCode"].Value;
                if (!curr.Equals(expCurrCode))
                {
                    message = $"Currency code for Base Amount in offers is not as expected currency {expCurrCode} and actual currency {curr}";
                    return false;
                }
            }
            //Currency code for Total Tax amount in Offer items
            XmlNodeList OfferItem_TotalTax_CurrCode = responseXmlDocument.GetElementsByTagName("TotalTaxAmount");
            foreach (XmlNode xn_TotalTaxt in OfferItem_TotalTax_CurrCode)
            {
                string curr = xn_TotalTaxt.Attributes["CurCode"].Value;
                if (!curr.Equals(expCurrCode))
                {
                    message = $"Currency code for Total Amount in offers is as expected currency {expCurrCode} and actual currency {curr} ";
                    return false;
                }
            }
            return true;
        }

        public static bool ValidateWarningwithCurrencyCodeReturned(string response, out string message)
        {
            message = string.Empty;
            string expWarCode = "71A";
            string expWarning = "The requested currency is not applicable for the agent location. Prices are quoted in the currency applicable for the agent location.";

            XmlDocument responseXmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(responseXmlDocument.NameTable);

            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            responseXmlDocument.LoadXml(response);
            XmlNode actual_Warning_Code = responseXmlDocument.SelectSingleNode("//ct:Warning//ct:Code", xmlNamespaceManager);// GetElementsByTagName("Code");
            XmlNode actual_Warning_Desc = responseXmlDocument.SelectSingleNode("//ct:Warning//ct:DescText", xmlNamespaceManager);

            if (actual_Warning_Code != null)
            {
                if ((!(actual_Warning_Code.InnerText).Equals(expWarCode)) || (!actual_Warning_Desc.InnerText.Equals(expWarning)))
                {
                    message = $" Currency Warning code error -: expected code was : {expWarCode} -- actual code was : {actual_Warning_Code.InnerText} \r\n" +
                    $" Currency Warning description error -: expected description was : {expWarning} -- actual description was : {actual_Warning_Desc.InnerText} \r\n";
                    return false;
                }
            }
            return true;
        }
    }
}
