using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
namespace VAA.EAPI.Automation.NDC.Tests.ServiceList
{
    public class ServiceListAssertions
    {

        //ValidateAlcarteOffers 
        public static bool ValidateOffers(string responseXml, out ValidationResultHelper result)
        {

            result = new ValidationResultHelper();

            XmlDocument xmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            xmlDocument.LoadXml(responseXml);

            string offerID = xmlDocument.SelectSingleNode("msg:IATA_ServiceListRS/msg:Response/ct:ALaCarteOffer/ct:OfferID", xmlNamespaceManager).InnerText;

            if (string.IsNullOrEmpty(offerID))
            {
                result.Message.Add("No offers returned in service list response");

            }

            return result.IsValid;
        }

        //DatedMarketingAndOperatingList
        public static bool ValidateDatedMarketingAndOperatingList(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument serviceListResponseDocument = new XmlDocument();

            serviceListResponseDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = serviceListResponseDocument.CreateXmlNamespaceManager();

            XmlNode[] datedMarketingSegmentCount = serviceListResponseDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment", xmlNamespaceManager).OfType<XmlNode>().ToArray();

            IGrouping<string, XmlNode>[] datedMarketingSegments = serviceListResponseDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment[ct:DatedMarketingSegmentId=../../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:DatedMarketingSegmentRefId and ct:DatedOperatingSegmentRefId=../../../ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment/ct:DatedOperatingSegmentId and ct:Arrival/ct:IATA_LocationCode != ct:Dep/ct:IATA_LocationCode  and ct:Arrival/ct:AircraftScheduledDateTime != ct:Dep/ct:AircraftScheduledDateTime]", xmlNamespaceManager)
                                                            .OfType<XmlNode>()
                                                            .GroupBy(x => x.SelectSingleNode("ct:DatedMarketingSegmentId", xmlNamespaceManager).InnerText)
                                                            .ToArray();

            if (!datedMarketingSegments.Length.Equals(datedMarketingSegmentCount.Length))
            {
                result.Message.Add($"Invalid datedMarketingSegments, was expecting {datedMarketingSegmentCount.Length} but found {datedMarketingSegments.Length} instead.");

            }

            int nonVSsegmentCount = 0;

            foreach (XmlNode node in datedMarketingSegmentCount)
            {
                int dmsID = (node["MarketingCarrierFlightNumberText"].InnerText).Length;
                if (dmsID == 4)
                {
                    nonVSsegmentCount++;
                }
            }

            int nonVSFlightWarningCount = serviceListResponseDocument.SelectNodes($"//ct:Warning[ct:Code='76H' and ct:DescText]", xmlNamespaceManager).Count;

            if (nonVSFlightWarningCount != nonVSsegmentCount)
            {
                result.Message.Add($"Non VS flight warning, was expecting {nonVSsegmentCount} but found {nonVSFlightWarningCount} instead.");
            }

            return result.IsValid;
        }

        //PaxList Validation
        public static bool ValidatePaxList(string responseXml, string offerPriceResponse, out ValidationResultHelper result)
        {

            result = new ValidationResultHelper();
            XmlDocument xmlDocument = new XmlDocument();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            xmlDocument.LoadXml(responseXml);

            XmlNodeList PaxListServiceList = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", xmlNamespaceManager);

            XmlDocument offerPriceResponseDocument = new XmlDocument();
            offerPriceResponseDocument.LoadXml(offerPriceResponse);

            XmlNamespaceManager offerPriceXmlNamespaceManager = new XmlNamespaceManager(offerPriceResponseDocument.NameTable);
            offerPriceXmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            offerPriceXmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            XmlNodeList paxListOfferPrice = offerPriceResponseDocument.SelectNodes("msg:IATA_OfferPriceRS/msg:Response/ct:DataLists/ct:PaxList/ct:Pax", offerPriceXmlNamespaceManager);

            if (PaxListServiceList.Count != paxListOfferPrice.Count)
            {
                result.Message.Add("paxList in servicelist response is not as expected");
            }

            foreach (XmlNode paxNode in PaxListServiceList)
            {
                string paxID = paxNode.SelectSingleNode("ct:PaxID", xmlNamespaceManager).InnerText;

                string pTC = paxNode.SelectSingleNode("ct:PTC", xmlNamespaceManager).InnerText;

                if (paxID.Has("ADULT") && !pTC.Is("ADT"))
                {
                    result.Message.Add("paxList in servicelist response is not as expected");

                }

                if (paxID.Has("CHILD") && !pTC.Is("CHD"))
                {
                    result.Message.Add("paxList in servicelist response is not as expected");

                }

                if (paxID.Has("YOUNG") && !pTC.Is("GBE"))
                {
                    result.Message.Add("paxList in servicelist response is not as expected");

                }

                if (paxID.Has("INFANT") && !pTC.Is("INF"))
                {
                    result.Message.Add("paxList in servicelist response is not as expected");

                }

            }

            return result.IsValid;
        }

        //PaxSegmentList Validation
        public static bool ValidatePaxSegmentList(string responseXml, out ValidationResultHelper result)
        {

            result = new ValidationResultHelper();

            XmlDocument xmlDocument = new XmlDocument();

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            xmlDocument.LoadXml(responseXml);

            int paxSegmentCount = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment", xmlNamespaceManager).Count;

            int paxSegments = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:DataLists/ct:PaxSegmentList/ct:PaxSegment[ct:DatedMarketingSegmentRefId=../../../ct:DataLists/ct:DatedMarketingSegmentList/ct:DatedMarketingSegment/ct:DatedMarketingSegmentId]", xmlNamespaceManager).Count;


            if (!paxSegmentCount.Equals(paxSegments))
            {
                result.Message.Add("pax segment list is no as expected");

            }

            return result.IsValid;
        }

        //ServiceDefinitionList
        public static bool ValidateServiceDefnitionList(string responseXml, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(responseXml);
            XmlNamespaceManager xmlNamespaceManager = xmlDocument.CreateXmlNamespaceManager();

            int alcarteOfferCount = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:ALaCarteOffer/ct:OfferItem", xmlNamespaceManager).Count;

            int serviceDefinitionCount = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager).Count;

            if (!alcarteOfferCount.Equals(serviceDefinitionCount))
            {
                result.Message.Add("mismatch serviceDefintion count and alcarte offer count");

            }

            XmlNodeList serviceDefinitionActualCount = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition[ ct:ServiceDefinitionID=../../../ct:ALaCarteOffer/ct:OfferItem/ct:Service/ct:ServiceDefinitionRefID and ct:OwnerCode='VS']", xmlNamespaceManager);

            if (!serviceDefinitionCount.Equals(serviceDefinitionActualCount.Count))
            {
                result.Message.Add("pax segment list is no as expected");

            }

            List<string> duplicatesCheckName = new List<string>();

            List<string> duplicatesCheckServiceCode = new List<string>();

            foreach (XmlNode serviceDefinition in serviceDefinitionActualCount)
            {
                string name = serviceDefinition.SelectSingleNode("ct:Name", xmlNamespaceManager).InnerText;

                if (!duplicatesCheckName.Contains(name))
                {
                    duplicatesCheckName.Add(name);
                }
                else
                {
                    result.Message.Add("duplicate names found in the servicedefinition");

                }

                string serviceCode = serviceDefinition.SelectSingleNode("ct:ServiceCode", xmlNamespaceManager).InnerText;

                if (!duplicatesCheckServiceCode.Contains(serviceCode))
                {
                    duplicatesCheckServiceCode.Add(serviceCode);
                }
                else
                {
                    result.Message.Add("duplicate serviceCode found in the servicedefinition");

                }
            }

            return result.IsValid;
        }

        //ValidateAlcarteOffers
        public static bool ValidateAlcarteOffers(string responseXml, out ValidationResultHelper result)
        {

            result = new ValidationResultHelper();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(responseXml);

            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
            xmlNamespaceManager.AddNamespace("msg", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");
            xmlNamespaceManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            string offerID = xmlDocument.SelectSingleNode("msg:IATA_ServiceListRS/msg:Response/ct:ALaCarteOffer/ct:OfferID", xmlNamespaceManager).InnerText;


            int alcarteOfferCount = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:ALaCarteOffer/ct:OfferItem", xmlNamespaceManager).Count;

            int serviceDefinitionCount = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition", xmlNamespaceManager).Count;

            if (!alcarteOfferCount.Equals(serviceDefinitionCount))
            {
                result.Message.Add("mismatch serviceDefintion count and alcarte offer count");

            }

            XmlNodeList alcarteOfferActualCount = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:ALaCarteOffer/ct:OfferItem[ct:Eligibility/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID=../../ct:DataLists/ct:PaxSegmentList/ct:PaxSegment/ct:PaxSegmentID and ct:Eligibility/ct:PaxRefID=../../ct:DataLists/ct:PaxList/ct:Pax/ct:PaxID and ct:Service/ct:ServiceDefinitionRefID =../../ct:DataLists/ct:ServiceDefinitionList/ct:ServiceDefinition/ct:ServiceDefinitionID]", xmlNamespaceManager);

            int vsFlightCount = xmlDocument.SelectNodes("msg:IATA_ServiceListRS/msg:Response/ct:DataLists/ct:DatedOperatingSegmentList/ct:DatedOperatingSegment[ct:CarrierDesigCode='VS']", xmlNamespaceManager).Count;

            if (!serviceDefinitionCount.Equals(alcarteOfferActualCount.Count))
            {
                result.Message.Add("pax segment list is no as expected");

            }

            int serviceIDCountFlag = 0;

            foreach (XmlNode alcarteOffer in alcarteOfferActualCount)
            {
                serviceIDCountFlag++;

                string name = alcarteOffer.SelectSingleNode("ct:OfferItemID", xmlNamespaceManager).InnerText;

                XmlNode amount = alcarteOffer.SelectSingleNode("ct:UnitPrice/ct:TotalAmount", xmlNamespaceManager);

                string curcode = amount.Attributes.GetNamedItem("CurCode").InnerText;

                if (string.IsNullOrEmpty(curcode))
                {
                    result.Message.Add("Currency Code is expected in service list offer items");

                }

                string serviceID = alcarteOffer.SelectSingleNode("ct:Service/ct:ServiceID", xmlNamespaceManager).InnerText;

                int eligibilieSegments = alcarteOffer.SelectNodes("ct:Eligibility/ct:OfferFlightAssociations/ct:PaxSegmentReferences/ct:PaxSegmentRefID", xmlNamespaceManager).Count;

                if (eligibilieSegments != vsFlightCount)
                {
                    result.Message.Add("VS flights segments are only expected in offerItem");

                }

                if (!name.Has(offerID))
                {
                    result.Message.Add("Invalid OfferItem ID in AlacteOffers");

                }

                if (!serviceID.Equals("SV" + serviceIDCountFlag))
                {
                    result.Message.Add("Invalid service ID inside alcarte offers");

                }

            }

            return result.IsValid;
        }


    }
}
