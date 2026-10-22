using System.ComponentModel;
using System.Reflection;
using System.Xml;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderchangeSplitPnr;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderCreate;

namespace VAA.EAPI.Automation.NDC.Requests
{
    public static class RequestHelper
    {
        public static readonly string env = Config.Environment;
        public static readonly string apiVersion = Config.Version;

        public enum SeatPriceType
        {
            None = 0,
            Free,
            Paid
        }

        public enum ApiResult
        {
            None = 0,
            AirShoppingRQ,
            AirShoppingRS,
            OfferPriceRQ,
            OfferPriceRS,
            SeatAvailabilityRQ,
            PostSaleSeatAvailabilityRQ,
            SeatAvailabilityRS,
            PostSaleSeatAvailabilityRS,
            ServiceListRQ,
            PostSaleServiceListRQ,
            ServiceListRS,
            PostSaleServiceListRS,
            OrderCreateRQ,
            OrderCreateRS,
            OrderRetrieveRQ,
            OrderRetrieveRS,
            OrderReshopRQ,
            OrderReshopCancelRQ,
            OrderReshopCancelRS,
            OrderReshopUpdatePaxNameRQ,
            OrderReshopUpdatePaxNameRS,
            OrderReshopRS,
            OrderQuoteRQ,
            OrderQuoteHoldBookingRQ,
            OrderQuoteRS,
            OrderQuoteHoldBookingRS,
            OrderChangeConifrmHeldBookingRQ,
            OrderChangeConifrmHeldBookingRS,
            OrderChangeRQ,
            OrderChangeRS,
            OrderChangeSeatRQ,
            OrderChangeSeatRS,
            OrderChangeCancelRQ,
            OrderChangeCancelRS,
        }

        public enum ChangeOrder
        {
            None = 0,
            CompleteJourney,
            OutBoundJourney,
            InBoundJourney,
        }

        public enum FareSubTypeCode
        {
            None = 0,

            [Description("Seat Only")]
            SeatOnly,

            [Description("Tour Operator")]
            TourOperator,

            [Description("VFR")]
            VFR,

            [Description("Charity")]
            Charity,

            [Description("Marine")]
            Marine,

            [Description("Cruise")]
            Cruise,

            [Description("Student")]
            Student,

            [Description("SME")]
            SME,

            [Description("Corporate")]
            Corporate
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static string GenerateAirshoppingRequest(string ordinal, string orgRole, string participatingOrgName, string participatingOrgOrgID,
            JourneyType journeyType, LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None,
            LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None, LocationType locationSeven = LocationType.None, LocationType locationEight = LocationType.None, LocationType locationNine = LocationType.None, LocationType locationTen = LocationType.None, LocationType locationEleven = LocationType.None, LocationType locationTwelve = LocationType.None, string cabinTypeCodeOne = "", string cabinTypeCodeTwo = "",
            string cabinTypeCodeThree = "", string cabinTypeCodeFour = "", string cabinTypeCodeFive = "", string cabinTypeCodeSix = "", int numberOfadults = 0, int numberOfchildren = 0, int numberOfyouths = 0, int numberOfinfants = 0, string deptDate = "", string flighttype = "", string fareTypeCode = "", FareSubTypeCode subTypeCode = FareSubTypeCode.None, string taxonomy = "", string curCode = "", string corporate = "", string agentType = "Direct", string participatingOrgOrgID2 = "", string calendarSearch = "")
        {
            if (string.IsNullOrEmpty(deptDate))
                deptDate = DateTime.Now.AddMonths(10).ToString("yyyy-MM-dd");
            var dateTwo = Convert.ToDateTime(deptDate).AddDays(7).ToString("yyyy-MM-dd");
            var dateThree = Convert.ToDateTime(dateTwo).AddDays(6).ToString("yyyy-MM-dd");
            var dateFour = Convert.ToDateTime(dateThree).AddDays(8).ToString("yyyy-MM-dd");
            var dateFive = Convert.ToDateTime(dateFour).AddDays(7).ToString("yyyy-MM-dd");
            var dateSix = Convert.ToDateTime(dateFive).AddDays(6).ToString("yyyy-MM-dd");

            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_AirShoppingRequest();

            //Setting the values for DistributionChain
            switch (agentType)
            {
                case "Direct":
                    instance.DistributionChain.DistributionChainLink.AddRange(new[]
                    {
                        new Models.AirShopping.DistributionChainLink()
                        {
                            Ordinal="1",
                            OrgRole = "Seller",
                            ParticipatingOrg= new Models.AirShopping.ParticipatingOrg()
                            {
                                Name = participatingOrgName,
                                OrgID=participatingOrgOrgID
                            }
                        }
                    });
                    break;

                case "InDirect":
                    instance.DistributionChain.DistributionChainLink.AddRange(new[]
                    {
                        new Models.AirShopping.DistributionChainLink()
                        {
                            Ordinal="1",
                            OrgRole = "Seller",
                            ParticipatingOrg= new Models.AirShopping.ParticipatingOrg()
                            {
                                Name = participatingOrgName,
                                OrgID=participatingOrgOrgID
                            }

                        },
                        new Models.AirShopping.DistributionChainLink()
                        {
                            Ordinal="2",
                            OrgRole = "Distributor",
                            ParticipatingOrg= new Models.AirShopping.ParticipatingOrg()
                            {
                                Name = participatingOrgName,
                                OrgID=participatingOrgOrgID2
                            }
                        }
                    });
                    break;
                default:
                    break;
            }

            //Adding FlightRequest
            switch (journeyType)
            {
                case JourneyType.OneWay:
                    instance.Request.FlightRequest.FlightRequestOriginDestinationsCriteria.OriginDestCriteria.AddRange(new[]
                    {
                        new Models.AirShopping.OriginDestCriteria()
                        {
                            OriginDepCriteria = new Models.AirShopping.OriginDepCriteria()
                            {
                               IATA_LocationCode = locationOne.ToString(),
                               Date = deptDate
                            },
                           DestArrivalCriteria = new Models.AirShopping.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationTwo.ToString()
                           },
                           CabinType = new Models.AirShopping.CabinType()
                           {
                               CabinTypeCode = cabinTypeCodeOne,
                               PrefLevel = new()
                               {
                                   PrefLevelCode = "Required"
                               }
                           },
                           CalendarDateCriteria=new Models.AirShopping.CalendarDateCriteria()
                           {
                                DaysAfterNumber=3,
                                DaysBeforeNumber=3
                           }
                        }
                    });
                    break;
                case JourneyType.Return:
                    instance.Request.FlightRequest.FlightRequestOriginDestinationsCriteria.OriginDestCriteria.AddRange(new[]
{
                        new Models.AirShopping.OriginDestCriteria()
                        {
                           OriginDepCriteria = new Models.AirShopping.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationOne.ToString(),
                               Date = deptDate
                           },
                           DestArrivalCriteria = new Models.AirShopping.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationTwo.ToString()
                           },
                           CabinType = new Models.AirShopping.CabinType()
                           {
                               CabinTypeCode = cabinTypeCodeOne,
                               PrefLevel = new()
                               {
                                   PrefLevelCode = "Required"
                               }
                           },
                           CalendarDateCriteria=new Models.AirShopping.CalendarDateCriteria()
                           {
                                DaysAfterNumber=3,
                                DaysBeforeNumber=3
                           }

                        },
                       new Models.AirShopping.OriginDestCriteria()
                       {
                           OriginDepCriteria = new Models.AirShopping.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationThree.ToString(),
                               Date = dateTwo
                           },
                           DestArrivalCriteria = new Models.AirShopping.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationFour.ToString()
                           },
                           CabinType = new Models.AirShopping.CabinType()
                           {
                               CabinTypeCode = cabinTypeCodeTwo,
                               PrefLevel = new()
                               {
                                   PrefLevelCode = "Required"
                               }
                           },
                           CalendarDateCriteria=new Models.AirShopping.CalendarDateCriteria()
                           {
                                DaysAfterNumber=3,
                                DaysBeforeNumber=3
                           }
                       }
                    });
                    break;
                case JourneyType.MultiCity:
                    instance.Request.FlightRequest.FlightRequestOriginDestinationsCriteria.OriginDestCriteria.AddRange(new[]
                    {
                        new Models.AirShopping.OriginDestCriteria()
                        {
                           OriginDepCriteria = new Models.AirShopping.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationOne.ToString(),
                               Date = deptDate
                           },
                           DestArrivalCriteria = new Models.AirShopping.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationTwo.ToString()
                           },
                           CabinType = new Models.AirShopping.CabinType()
                           {
                               CabinTypeCode = cabinTypeCodeOne,
                               PrefLevel = new()
                               {
                                   PrefLevelCode = "Required"
                               }
                           },
                           CalendarDateCriteria=new Models.AirShopping.CalendarDateCriteria()
                           {
                                DaysAfterNumber=3,
                                DaysBeforeNumber=3
                           }
                        },
                       new Models.AirShopping.OriginDestCriteria()
                       {
                           OriginDepCriteria = new Models.AirShopping.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationThree.ToString(),
                               Date = dateTwo
                           },
                           DestArrivalCriteria = new Models.AirShopping.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationFour.ToString()
                           },
                           CabinType = new Models.AirShopping.CabinType()
                           {
                               CabinTypeCode = cabinTypeCodeOne,
                               PrefLevel = new()
                               {
                                   PrefLevelCode = "Required"
                               }
                           },
                           CalendarDateCriteria=new Models.AirShopping.CalendarDateCriteria()
                           {
                                DaysAfterNumber=3,
                                DaysBeforeNumber=3
                           }
                       },
                       new Models.AirShopping.OriginDestCriteria()
                       {
                           OriginDepCriteria = new Models.AirShopping.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationFive.ToString(),
                               Date = dateThree
                           },
                           DestArrivalCriteria = new Models.AirShopping.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationSix.ToString()
                           },
                           CabinType = new Models.AirShopping.CabinType()
                           {
                               CabinTypeCode = cabinTypeCodeOne,
                               PrefLevel = new()
                               {
                                   PrefLevelCode = "Required"
                               }
                           },
                           CalendarDateCriteria=new Models.AirShopping.CalendarDateCriteria()
                           {
                                DaysAfterNumber=3,
                                DaysBeforeNumber=3
                           }
                       },
                       new Models.AirShopping.OriginDestCriteria()
                       {
                           OriginDepCriteria = new Models.AirShopping.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationSeven.ToString(),
                               Date = dateFour
                           },
                           DestArrivalCriteria = new Models.AirShopping.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationEight.ToString()
                           },
                           CabinType = new Models.AirShopping.CabinType()
                           {
                               CabinTypeCode = cabinTypeCodeOne,
                               PrefLevel = new()
                               {
                                   PrefLevelCode = "Required"
                               }
                           },
                           CalendarDateCriteria=new Models.AirShopping.CalendarDateCriteria()
                           {
                                DaysAfterNumber=3,
                                DaysBeforeNumber=3
                           }
                       },
                       new Models.AirShopping.OriginDestCriteria()
                       {
                           OriginDepCriteria = new Models.AirShopping.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationNine.ToString(),
                               Date = dateFour
                           },
                           DestArrivalCriteria = new Models.AirShopping.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationTen.ToString()
                           },
                           CabinType = new Models.AirShopping.CabinType()
                           {
                               CabinTypeCode = cabinTypeCodeOne,
                               PrefLevel = new()
                               {
                                   PrefLevelCode = "Required"
                               }
                           },
                           CalendarDateCriteria=new Models.AirShopping.CalendarDateCriteria()
                           {
                                DaysAfterNumber=3,
                                DaysBeforeNumber=3
                           }
                       },
                       new Models.AirShopping.OriginDestCriteria()
                       {
                           OriginDepCriteria = new Models.AirShopping.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationEleven.ToString(),
                               Date = dateSix
                           },
                           DestArrivalCriteria = new Models.AirShopping.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationTwelve.ToString()
                           },
                           CabinType = new Models.AirShopping.CabinType()
                           {
                               CabinTypeCode = cabinTypeCodeOne,
                               PrefLevel = new()
                               {
                                   PrefLevelCode = "Required"
                               }
                           },
                           CalendarDateCriteria=new Models.AirShopping.CalendarDateCriteria()
                           {
                                DaysAfterNumber=3,
                                DaysBeforeNumber=3
                           }
                       }
                    });
                    break;
                default:
                    break;
            }

            instance.Request.PaxList.Pax.AddRange(Enumerable.Range(1, numberOfadults).Select(i => new Models.AirShopping.Pax()
            {
                PaxID = $"ADULT_{i}",
                PTC = "ADT"
            }));

            instance.Request.PaxList.Pax.AddRange(Enumerable.Range(1, numberOfyouths).Select(i => new Models.AirShopping.Pax()
            {
                PaxID = $"YOUNG_{i}",
                PTC = "GBE"
            }));

            instance.Request.PaxList.Pax.AddRange(Enumerable.Range(1, numberOfchildren).Select(i => new Models.AirShopping.Pax()
            {
                PaxID = $"CHILD_{i}",
                PTC = "CHD"
            }));

            instance.Request.PaxList.Pax.AddRange(Enumerable.Range(1, numberOfinfants).Select(i => new Models.AirShopping.Pax()
            {
                PaxID = $"INFANT_{i}",
                PTC = "INF"
            }));

            fareTypeCode = string.IsNullOrEmpty(fareTypeCode) ? "70J" : fareTypeCode;
            instance.Request.OfferCriteria.FareCriteria.FareTypeCode = fareTypeCode;
            if (fareTypeCode == "758" || fareTypeCode == "70G")
            {
                instance.Request.OfferCriteria.FareCriteria.FareSubTypeCode = subTypeCode.GetEnumDescription();
            }
            instance.Request.OfferCriteria.FareCriteria.PrefLevel.PrefLevelCode = "Required";

            instance.Request.OfferCriteria.ServiceCriteria.IncludeInd = "true";
            instance.Request.OfferCriteria.ServiceCriteria.TaxonomyCode = new List<string> { "189C", "13EC" };

            instance.Request.FlightRelatedCriteria.FlightCriteria.FlightCharacteristicsCriteria.CharacteristicCode = "Direct";
            instance.Request.FlightRelatedCriteria.FlightCriteria.FlightCharacteristicsCriteria.PrefLevel.PrefLevelCode = "Required";

            instance.Request.OfferCriteria.ProgramCriteria.ProgramOwner.Org.Name = "Corporate_name";
            instance.Request.OfferCriteria.ProgramCriteria.ProgramOwner.Org.OrgID = corporate;

            instance.Request.ResponseParameters.CurParameter.CurCode = curCode;

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "AirShopping", taxonomy, flighttype, fareTypeCode, corporate, curCode, calendarSearch: calendarSearch);
        }

        public static string GenerateUpdatedRequest(string request, string apiRequest, string taxonomy = "", string flighttype = "", string fareTypeCode = "", string corporate = "", string curCode = "", string offerId = "", string orderId = "", string flyingClubMember = "", string gst = "", string addressAtDestination = "", string paymentMethod = "", string calendarSearch = "", string reshopFlow = "", SeatPriceType seat = SeatPriceType.None, string exchangeType = "", string accountNumber = "", string seatPaymentCardType = "")
        {
            var updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(request);

            XmlNamespaceManager airShoppingRequestXmlnsManager = updatedRequest.CreateXmlNamespaceManager();

            var originDest = updatedRequest.SelectNodes("//ct:FlightRequestOriginDestinationsCriteria//ct:OriginDestCriteria", airShoppingRequestXmlnsManager);

            if (originDest.Count > 2 && apiRequest.Is("AirShopping"))
            {
                foreach (XmlNode node in originDest)
                {
                    var parentNode = node.SelectSingleNode("//ct:FlightRequestOriginDestinationsCriteria", airShoppingRequestXmlnsManager);

                    if (node.InnerText.Has("None"))
                    {
                        parentNode.RemoveChild(node);
                    }
                }
            }

            if (apiRequest.Is("OrderList"))
            {
                var orderSearchFilterCriteriaType = updatedRequest.SelectSingleNode("//ct:OrderSearchFilterCriteria", airShoppingRequestXmlnsManager);

                if (!orderSearchFilterCriteriaType.SelectSingleNode("//ct:CreationDateTimeFilterCriteria", airShoppingRequestXmlnsManager).HasChildNodes)
                {
                    orderSearchFilterCriteriaType.RemoveChild(orderSearchFilterCriteriaType.SelectSingleNode("//ct:CreationDateTimeFilterCriteria", airShoppingRequestXmlnsManager));
                }

                if (!orderSearchFilterCriteriaType.SelectSingleNode("//ct:LastModifiedDateTimeFilterCriteria", airShoppingRequestXmlnsManager).HasChildNodes)
                {
                    orderSearchFilterCriteriaType.RemoveChild(orderSearchFilterCriteriaType.SelectSingleNode("//ct:LastModifiedDateTimeFilterCriteria", airShoppingRequestXmlnsManager));
                }

                if (!orderSearchFilterCriteriaType.SelectSingleNode("//ct:OrderCancellationDateFilterCriteria", airShoppingRequestXmlnsManager).HasChildNodes)
                {
                    orderSearchFilterCriteriaType.RemoveChild(orderSearchFilterCriteriaType.SelectSingleNode("//ct:OrderCancellationDateFilterCriteria", airShoppingRequestXmlnsManager));
                }

                if (!orderSearchFilterCriteriaType.SelectSingleNode("//ct:PaymentFilterCriteria", airShoppingRequestXmlnsManager).HasChildNodes)
                {
                    orderSearchFilterCriteriaType.RemoveChild(orderSearchFilterCriteriaType.SelectSingleNode("//ct:PaymentFilterCriteria", airShoppingRequestXmlnsManager));
                }

                if (!orderSearchFilterCriteriaType.SelectSingleNode("//ct:TravelDateFilterCriteria", airShoppingRequestXmlnsManager).HasChildNodes)
                {
                    orderSearchFilterCriteriaType.RemoveChild(orderSearchFilterCriteriaType.SelectSingleNode("//ct:TravelDateFilterCriteria", airShoppingRequestXmlnsManager));
                }
            }
            if (apiRequest.Is("OrderCreate"))
            {
                var selectNode = updatedRequest.SelectSingleNode("//ct:ContactInfo", airShoppingRequestXmlnsManager);

                var distributionChainCount = updatedRequest.SelectNodes("//ct:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

                var removeNode = updatedRequest.SelectSingleNode($"//ct:DistributionChainLink[{distributionChainCount - 1}]", airShoppingRequestXmlnsManager);

                removeNode.ParentNode.RemoveChild(removeNode);

                var addNode = updatedRequest.SelectSingleNode("//ct:DistributionChainLink[1]//ct:Ordinal", airShoppingRequestXmlnsManager);

                addNode.ParentNode.PrependChild(selectNode);
            }

            if (string.IsNullOrEmpty(taxonomy) && string.IsNullOrEmpty(corporate) && apiRequest.Is("AirShopping") && string.IsNullOrEmpty(fareTypeCode))
            {
                XmlNode OfferCriteria = updatedRequest.SelectSingleNode("//ct:OfferCriteria", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                OfferCriteria.ParentNode.RemoveChild(OfferCriteria);
            }

            if (string.IsNullOrEmpty(calendarSearch) && apiRequest.Is("AirShopping"))
            {
                var calendarNodes = updatedRequest.SelectNodes("//ct:FlightRequestOriginDestinationsCriteria//ct:OriginDestCriteria//ct:CalendarDateCriteria", airShoppingRequestXmlnsManager);

                foreach (XmlNode node in calendarNodes)
                {
                    node.ParentNode.RemoveChild(node);
                }
            }

            if (!string.IsNullOrEmpty(taxonomy) && string.IsNullOrEmpty(corporate))
            {
                XmlNode OfferCriteria = updatedRequest.SelectSingleNode("//ct:OfferCriteria//ct:ProgramCriteria", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                OfferCriteria.ParentNode.RemoveChild(OfferCriteria);
            }

            if (string.IsNullOrEmpty(taxonomy) && !string.IsNullOrEmpty(corporate))
            {
                XmlNode OfferCriteria = updatedRequest.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                OfferCriteria.ParentNode.RemoveChild(OfferCriteria);
            }

            if (string.IsNullOrEmpty(flighttype) && apiRequest.Is("AirShopping"))
            {
                XmlNode FlightRelatedCriteria = updatedRequest.SelectSingleNode("//ct:FlightRelatedCriteria", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                FlightRelatedCriteria.ParentNode.RemoveChild(FlightRelatedCriteria);
            }

            if (string.IsNullOrEmpty(curCode) && apiRequest.Is("AirShopping"))
            {
                XmlNode ResponseParameters = updatedRequest.SelectSingleNode("//ct:ResponseParameters", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                ResponseParameters.ParentNode.RemoveChild(ResponseParameters);
            }

            if (string.IsNullOrEmpty(taxonomy) && string.IsNullOrEmpty(corporate) && !string.IsNullOrEmpty(fareTypeCode))
            {
                XmlNode serviceCriteria = updatedRequest.SelectSingleNode("//ct:OfferCriteria//ct:ServiceCriteria", airShoppingRequestXmlnsManager);
                //Removing serviceCriteria node as taxanomy type is not requested
                serviceCriteria.ParentNode.RemoveChild(serviceCriteria);

                XmlNode programCriteria = updatedRequest.SelectSingleNode("//ct:OfferCriteria//ct:ProgramCriteria", airShoppingRequestXmlnsManager);
                //Removing serviceCriteria node as taxanomy type is not requested
                programCriteria.ParentNode.RemoveChild(programCriteria);
            }

            if (string.IsNullOrEmpty(offerId) && apiRequest.Is("Seat"))
            {
                XmlNode ResponseParameters = updatedRequest.SelectSingleNode("//ct:SeatAvailCoreRequest//ct:OfferRequest", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                ResponseParameters.ParentNode.RemoveChild(ResponseParameters);
            }

            if (string.IsNullOrEmpty(orderId) && apiRequest.Is("Seat"))
            {
                XmlNode ResponseParameters = updatedRequest.SelectSingleNode("//ct:SeatAvailCoreRequest//ct:OrderRequest", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                ResponseParameters.ParentNode.RemoveChild(ResponseParameters);
            }

            if (string.IsNullOrEmpty(flyingClubMember) && apiRequest.Is("OrderCreate"))
            {
                var paxNodes = updatedRequest.SelectNodes("//ct:Pax", airShoppingRequestXmlnsManager);

                foreach (XmlNode node in paxNodes)
                {
                    var loyaltyProgramAccount = node.SelectSingleNode("//ct:Pax//ct:LoyaltyProgramAccount", airShoppingRequestXmlnsManager);

                    loyaltyProgramAccount.ParentNode.RemoveChild(loyaltyProgramAccount);
                }
            }

            if (string.IsNullOrEmpty(gst) && apiRequest.Is("OrderCreate"))
            {
                var paxNodes = updatedRequest.SelectNodes("//ct:Pax", airShoppingRequestXmlnsManager);

                foreach (XmlNode node in paxNodes)
                {
                    var loyaltyProgramAccount = node.SelectSingleNode("//ct:Pax//ct:Employer", airShoppingRequestXmlnsManager);

                    loyaltyProgramAccount.ParentNode.RemoveChild(loyaltyProgramAccount);
                }
            }

            if (string.IsNullOrEmpty(addressAtDestination) && apiRequest.Is("OrderCreate"))
            {
                var paxNodes = updatedRequest.SelectNodes("//ct:ContactInfoList//ct:ContactInfo", airShoppingRequestXmlnsManager);

                foreach (XmlNode node in paxNodes)
                {
                    var postalAddress = node.SelectSingleNode("//ct:ContactInfo//ct:PostalAddress", airShoppingRequestXmlnsManager);

                    postalAddress.ParentNode.RemoveChild(postalAddress);
                }
            }

            if (!string.IsNullOrEmpty(flyingClubMember) && apiRequest.Is("OrderCreate"))
            {
                var paxNodes = updatedRequest.SelectNodes("//ct:Pax", airShoppingRequestXmlnsManager);

                int i = paxNodes.Count;

                foreach (XmlNode node in paxNodes)
                {
                    if (i != 1)
                    {
                        var loyaltyProgramAccount = node.SelectSingleNode("//ct:Pax//ct:LoyaltyProgramAccount", airShoppingRequestXmlnsManager);

                        loyaltyProgramAccount.ParentNode.RemoveChild(loyaltyProgramAccount);
                    }
                    i--;
                }
            }

            if (string.IsNullOrEmpty(flighttype) && apiRequest.Is("reshopRequest"))
            {
                XmlNode FlightRelatedCriteria = updatedRequest.SelectSingleNode("//ct:UpdateOrder//ct:ReshopOrder//ct:ReshopOrderChoice//ct:ServiceOrder//ct:AddOfferItems//ct:FlightRelatedCriteria", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                FlightRelatedCriteria.ParentNode.RemoveChild(FlightRelatedCriteria);
            }

            if (string.IsNullOrEmpty(offerId) && apiRequest.Is("ServiceList"))
            {
                XmlNode ResponseParameters = updatedRequest.SelectSingleNode("//ct:ServiceListCoreRequest//ct:OfferRequest", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                ResponseParameters.ParentNode.RemoveChild(ResponseParameters);
            }

            if (string.IsNullOrEmpty(orderId) && apiRequest.Is("ServiceList"))
            {
                XmlNode ResponseParameters = updatedRequest.SelectSingleNode("//ct:ServiceListCoreRequest//ct:OrderRequest", airShoppingRequestXmlnsManager);
                //Removing FlightRelatedCriteria node as flight type is not requested
                ResponseParameters.ParentNode.RemoveChild(ResponseParameters);
            }

            if (reshopFlow.Is("cancel") && apiRequest.Is("reshopRequest"))
            {
                XmlNode FlightRelatedCriteria = updatedRequest.SelectSingleNode("//ct:UpdateOrder//ct:ReshopOrder", airShoppingRequestXmlnsManager);

                FlightRelatedCriteria.ParentNode.RemoveChild(FlightRelatedCriteria);

                XmlNode bookingIdNode = updatedRequest.SelectSingleNode("//ct:BookingRef", airShoppingRequestXmlnsManager);

                bookingIdNode.ParentNode.RemoveChild(bookingIdNode);
            }

            if (reshopFlow.Is("updatePaxName") && apiRequest.Is("reshopRequest"))
            {
                XmlNode serviceOrder = updatedRequest.SelectSingleNode("//ct:UpdateOrder//ct:ReshopOrder//ct:ReshopOrderChoice//ct:ServiceOrder", airShoppingRequestXmlnsManager);

                serviceOrder.ParentNode.RemoveChild(serviceOrder);

                XmlNode cancelOrderRef = updatedRequest.SelectSingleNode("//ct:UpdateOrder//ct:CancelOrderRef", airShoppingRequestXmlnsManager);

                cancelOrderRef.ParentNode.RemoveChild(cancelOrderRef);
            }

            if (reshopFlow.Is("evenExchange") && apiRequest.Is("reshopRequest"))
            {
                XmlNode FlightRelatedCriteria = updatedRequest.SelectSingleNode("//ct:UpdateOrder//ct:CancelOrderRef", airShoppingRequestXmlnsManager);

                FlightRelatedCriteria.ParentNode.RemoveChild(FlightRelatedCriteria);

                XmlNode updatePaxName = updatedRequest.SelectSingleNode("//ct:UpdateOrder//ct:ReshopOrder//ct:ReshopOrderChoice//ct:UpdatePaxName", airShoppingRequestXmlnsManager);

                updatePaxName.ParentNode.RemoveChild(updatePaxName);
            }

            if (paymentMethod.Has("Unsecure"))
            {
                var secureProgram = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentProcessingDetails//ct:PaymentCard//ct:SecureProgram", airShoppingRequestXmlnsManager);
                secureProgram.ParentNode.RemoveChild(secureProgram);
            }

            if (apiRequest.Is("OrderRetrieve") || apiRequest.Is("OrderHistory"))
            {
                var selectNode = updatedRequest.SelectSingleNode("//ct:ContactInfo", airShoppingRequestXmlnsManager);

                var distributionChainCount = updatedRequest.SelectNodes("//ct:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

                var removeNode = updatedRequest.SelectSingleNode($"//ct:DistributionChainLink[{distributionChainCount - 1}]", airShoppingRequestXmlnsManager);

                var addNode = updatedRequest.SelectSingleNode("//ct:DistributionChainLink[1]//ct:Ordinal", airShoppingRequestXmlnsManager);

                if (apiVersion.Is("v2"))
                {
                    removeNode.ParentNode.RemoveChild(removeNode);

                    addNode.ParentNode.PrependChild(selectNode);
                }

                if (apiVersion.Is("v1"))
                {
                    removeNode.ParentNode.RemoveChild(removeNode);
                }
            }

            if (apiRequest.Is("OrderCreate") || apiRequest.Is("OrderChange") || apiRequest.Is("OrderChange") || apiRequest.Is("OrderChangeSeat") || apiRequest.Is("OrderChangeConfirmHeldBooking"))
            {
                if (paymentMethod.Is("Cash"))
                {
                    updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethodCriteria//ct:PaymentTypeCode", airShoppingRequestXmlnsManager).InnerText = "CA";

                    var paymentMethodCriteria = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethod//ct:PaymentCard", airShoppingRequestXmlnsManager);

                    paymentMethodCriteria.ParentNode.RemoveChild(paymentMethodCriteria);

                    var payerName = updatedRequest.SelectSingleNode("//ct:Payer", airShoppingRequestXmlnsManager);

                    payerName.ParentNode.RemoveChild(payerName);
                }
                else
                {
                    var paymentMethodCriteria = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethod//ct:SettlementPlan", airShoppingRequestXmlnsManager);

                    paymentMethodCriteria.ParentNode.RemoveChild(paymentMethodCriteria);
                }

                if (paymentMethod.Is("EasyPay"))
                {
                    var paymentTypeCode = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethodCriteria", airShoppingRequestXmlnsManager);

                    paymentTypeCode.ParentNode.RemoveChild(paymentTypeCode);

                    var paymentMethodCriteria = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethod//ct:PaymentCard", airShoppingRequestXmlnsManager);

                    paymentMethodCriteria.ParentNode.RemoveChild(paymentMethodCriteria);

                    var payerName = updatedRequest.SelectSingleNode("//ct:Payer", airShoppingRequestXmlnsManager);

                    payerName.ParentNode.RemoveChild(payerName);
                }
                else
                {
                    var paymentMethodCriteria = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethod//ct:IATA_EasyPay", airShoppingRequestXmlnsManager);

                    paymentMethodCriteria.ParentNode.RemoveChild(paymentMethodCriteria);
                }
            }

            if (apiRequest.Is("OrderChangeSeat") && seat.Equals(SeatPriceType.Free) || apiRequest.Is("OrderChange") && exchangeType.Is("EvenExchange"))
            {
                if (!paymentMethod.Is("Cash") && !paymentMethod.Is("Easypay"))
                {
                    var paymentCard = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentProcessingDetails//ct:PaymentMethod//ct:PaymentCard", airShoppingRequestXmlnsManager);

                    paymentCard.ParentNode.RemoveChild(paymentCard);

                    var payerName = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentProcessingDetails//ct:Payer", airShoppingRequestXmlnsManager);

                    payerName.ParentNode.RemoveChild(payerName);
                }
                else if (paymentMethod.Is("Cash"))
                {
                    var paymentMethodCriteria = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethod//ct:SettlementPlan", airShoppingRequestXmlnsManager);

                    paymentMethodCriteria.ParentNode.RemoveChild(paymentMethodCriteria);
                }
                else
                {
                    var paymentMethodCriteria = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethod//ct:IATA_EasyPay", airShoppingRequestXmlnsManager);

                    paymentMethodCriteria.ParentNode.RemoveChild(paymentMethodCriteria);

                }
            }

            if (apiRequest.Is("OrderChange") && !paymentMethod.Is("EasyPay"))
            {
                var paymentMethodCriteria = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentMethodCriteria", airShoppingRequestXmlnsManager);

                paymentMethodCriteria.ParentNode.RemoveChild(paymentMethodCriteria);
            }

            if (apiRequest.Is("OrderChange") && !exchangeType.Is("EvenExchange") || apiRequest.Is("OrderChangeSeat") && !seat.Equals(SeatPriceType.Free) || apiRequest.Is("OrderChangeConfirmHeldBooking"))
            {
                var paymentCard = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:PaymentProcessingDetails//ct:PaymentMethod//ct:OfflinePayment", airShoppingRequestXmlnsManager);

                paymentCard.ParentNode.RemoveChild(paymentCard);
            }

            if (apiRequest.Is("OrderCreate"))
            {
                if (!seat.Equals(SeatPriceType.Paid) || paymentMethod.Has("CPD"))
                {
                    if (seatPaymentCardType.IsNullOrEmpty())
                    {
                        var paymentOfferAssociation = updatedRequest.SelectSingleNode("//ct:PaymentFunctions//ct:OfferAssociation", airShoppingRequestXmlnsManager);
                        paymentOfferAssociation?.ParentNode?.RemoveChild(paymentOfferAssociation);
                    }
                }
            }

            return updatedRequest.OuterXml;
        }

        public static string GenerateOfferPriceRequest(string airShoppingRequest, string airShoppingResult, string offerRefId)
        {
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            var offerNode = airShoppingResponseXmlDocument.SelectSingleNode($"//def:Offer[def:OfferID='{offerRefId}']", airShoppingResponseXmlnsManager);

            XmlDocument requestXmlDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OfferPriceRequest();

            airShoppingRequestXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OfferPrice.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//def:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OfferPrice.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OfferPrice.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding Request
            var paxNode = requestXmlDocument.SelectSingleNode("//def:PaxList", airShoppingRequestXmlnsManager).OuterXml;
            instance.Request.DataLists.PaxList = XMLSerialize.Deserialize<Models.AirShopping.PaxList, Models.OfferPrice.PaxList>(paxNode);

            //Adding selected offers
            instance.Request.PricedOffer.SelectedOfferList.SelectedOffer.OfferRefID = offerRefId;
            instance.Request.PricedOffer.SelectedOfferList.SelectedOffer.OwnerCode = "VS";

            instance.Request.PricedOffer.SelectedOfferList.SelectedOffer.SelectedOfferItem = instance.Request.DataLists.PaxList.Pax.GroupBy(x => x.PTC).Select(x =>
            {
                var paxRefIds = x.Select(p => p.PaxID).ToList();

                string condition = string.Join(" and ", paxRefIds.Select(x => $"def:FareDetail/def:PaxRefID = '{x}'"));

                var xPath = $"//def:OfferItem[{condition} and ../def:OfferID ='{offerRefId}']/def:OfferItemID";

                return new Models.OfferPrice.SelectedOfferItem()
                {
                    PaxRefID = paxRefIds,
                    OfferItemRefID = offerNode?.SelectSingleNode(xPath, airShoppingResponseXmlnsManager)?.InnerText ?? string.Empty
                };
            }).ToList();

            return XMLSerialize.Serialize(instance);
        }

        public static string GenerateSeatAvailabilityRequest(string airShoppingRequest, string offerRefId = "", string orderId = "")
        {
            var instance = XmlRequestBuilder.IATA_SeatAvailabilityRequest();

            XmlDocument requestXmlDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            airShoppingRequestXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.SeatAvailability.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//def:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.SeatAvailability.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.SeatAvailability.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding selected offers
            instance.Request.SeatAvailCoreRequest.OfferRequest.Offer.OfferID = offerRefId;
            instance.Request.SeatAvailCoreRequest.OfferRequest.Offer.OwnerCode = "VS";

            instance.Request.SeatAvailCoreRequest.OrderRequest.Order.OrderID = orderId;
            instance.Request.SeatAvailCoreRequest.OrderRequest.Order.OwnerCode = "VS";

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "Seat", orderId: orderId, offerId: offerRefId);
        }

        public static string GenerateOrderHistoryRQ(string airShoppingRequest, string orderCreateRS)
        {
            var instance = XmlRequestBuilder.IATA_OrderHistoryRQ();

            XmlDocument requestXmlDocument = new XmlDocument();

            requestXmlDocument.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = requestXmlDocument.CreateXmlNamespaceManager();

            var agentNode = requestXmlDocument.SelectSingleNode("//msg:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderHistory.DistributionChain>(agentNode);

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
               new Models.OrderHistory.DistributionChainLink()
               {
                  ContactInfo= new Models.OrderHistory.ContactInfo()
                  {
                      ContactInfoID = "AgentContactInfoID",
                      EmailAddress=new Models.OrderHistory.EmailAddress()
                      {
                          EmailAddressText="agentemail@gmail.com"
                      }
                  }

               }
            });

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderHistory.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderHistory.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            instance.Request.Order.OrderID = "VS932" + FindAndReturnDataHelper.ReturnBookingId(orderCreateRS) + FindAndReturnDataHelper.GetYearSuffix();
            instance.Request.Order.OwnerCode = "VS";

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "OrderHistory");
        }

        public static string GenerateOrderCreateRequest(string airShoppingRequest, string airShoppingResult, string offerRefId, string currencyCode, string totalAmount, string countryDialingCode = "", string paymentMethod = "", SeatPriceType seat = default, string seatAvailabilityXml = "", string flyingClubMember = "", string gst = "", string passport = "", string greencard = "", string visa = "", string knowntraveller = "", string redress = "", string addressAtDestination = "", string serviceList = "", string serviceListRS = "", int serviceCount = 1, string accountNumber = "", string agentMailID = "agentemail@gmail.com", string seatPaymentCardType = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderCreateRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            var offerNode = airShoppingResponseXmlDocument.SelectSingleNode($"//def:Offer[def:OfferID='{offerRefId}']", airShoppingResponseXmlnsManager);

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            airShoppingRequestXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderCreate.DistributionChain>(agentNode);

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
               new Models.OrderCreate.DistributionChainLink()
               {
                  ContactInfo= new Models.OrderCreate.ContactInfo()
                  {
                      ContactInfoID = "AgentContactInfoID",
                      EmailAddress=new Models.OrderCreate.EmailAddress()
                      {
                          EmailAddressText=agentMailID
                      }
                  }
               }
            });

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//def:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderCreate.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderCreate.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding PAX 
            var paxNode = requestXmlDocument.SelectSingleNode("//def:PaxList", airShoppingRequestXmlnsManager).OuterXml;
            instance.Request.DataLists.PaxList = XMLSerialize.Deserialize<Models.AirShopping.PaxList, Models.OrderCreate.PaxList>(paxNode);
            instance.Request.DataLists.PaxList.Pax.ForEach(p =>
            {
                var index = instance.Request.DataLists.PaxList.Pax.IndexOf(p);
                p.ContactInfoRefID = $"CID_{index + 1}";
                var givename = index == 0 ? "DAVID" : $"DAVID{DataGenerator.RandomString(5).ToUpper()}";
                var surname = index == 0 ? "SMITH" : $"SMITH{DataGenerator.RandomString(5).ToUpper()}";
                var birthdate = GenerateDataHelper.DateOfBirth(p.PTC);
                var genderCode = GenerateDataHelper.Gender();

                p.Individual = new()
                {
                    //infant <2 years, for child <12, for youth <18
                    Birthdate = birthdate,
                    GenderCode = genderCode,
                    GivenName = givename,
                    Surname = surname,
                    //LoyaltyProgramAccount may need to be added as / when required

                };
                if (!string.IsNullOrWhiteSpace(passport) && !string.IsNullOrWhiteSpace(visa))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderCreate.IdentityDoc()
                        {
                            Birthdate=birthdate,
                            CitizenshipCountryCode="USA",
                            ExpiryDate="2036-06-03",
                            GivenName=givename,
                            GenderCode=genderCode,
                            IdentityDocID="12345678",
                            IdentityDocTypeCode="PT",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname=surname,
                            Visa=new Models.OrderCreate.Visa {ExpiryDate="2036-06-03",HostCountryCode="USA",IssuingCountryCode="USA",VisaID="001001"},
                        },
                    });
                }

                if (!string.IsNullOrWhiteSpace(passport) && string.IsNullOrWhiteSpace(visa))
                {
                    p.IdentityDoc.Add(

                        new Models.OrderCreate.IdentityDoc()
                        {
                            Birthdate = birthdate,
                            CitizenshipCountryCode = "USA",
                            ExpiryDate = "2036-06-03",
                            GivenName = givename,
                            GenderCode = genderCode,
                            IdentityDocID = "12345678",
                            IdentityDocTypeCode = "PT",
                            IssueDate = "2016-06-03",
                            IssuingCountryCode = "USA",
                            ResidenceCountryCode = "USA",
                            Surname = surname,
                        }
                    );
                }

                if (!string.IsNullOrWhiteSpace(greencard))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderCreate.IdentityDoc()
                        {
                            Birthdate=birthdate,
                            ExpiryDate="2036-06-03",
                            GenderCode=genderCode,
                            GivenName=givename,
                            IdentityDocID="USA0001234",
                            IdentityDocTypeCode="GC",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname=surname
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(knowntraveller))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderCreate.IdentityDoc()
                        {
                            ExpiryDate="2036-06-03",
                            GivenName=givename,
                            IdentityDocID="000011111",
                            IdentityDocTypeCode="CR",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            Surname=surname
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(redress))
                {

                    p.RedressCase = new Models.OrderCreate.RedressCase { CountryCode = "USA", RedressCaseID = "123456" };
                }

                p.LoyaltyProgramAccount = new()
                {
                    AccountNumber = "",
                    LoyaltyProgram = new()
                    {
                        ProgramCode = "VS"
                    }
                };

                p.Employer = new()
                {
                    Name = "TCS",
                    SalesTaxRegistration = new()
                    {
                        CityName = "Chennai",
                        CountryCode = "IND",
                        CountryDialingCode = "91",
                        EmailAddressText = "abc@tcs.com",
                        PhoneNumber = "12345",
                        PostalCode = "6000",
                        SalesTaxRegistrationID = "11223",
                        StreetText = new List<string> { "Navaluroad", "Siruseri" },
                    },
                    TaxDeductedatSourceCapabilityInd = "true",
                };
            });

            XmlNodeList node2 = requestXmlDocument.SelectNodes("//def:Pax", airShoppingRequestXmlnsManager);

            instance.Request.CreateOrder.AcceptSelectedQuotedOfferList.SelectedPricedOffer.Add(new()
            {
                OfferRefID = offerRefId,
                OwnerCode = "VS",
                TotalOfferPriceAmount = new()
                {
                    CurCode = currencyCode,
                    Text = totalAmount
                },
                SelectedOfferItem = instance.Request.DataLists.PaxList.Pax.GroupBy(x => x.PTC).Select(x =>
                {
                    var paxRefIds = x.Select(p => p.PaxID).ToList();

                    string condition = string.Join(" and ", paxRefIds.Select(x => $"def:FareDetail/def:PaxRefID = '{x}'"));

                    var xPath = $"def:OfferItem[{condition}]/def:OfferItemID";

                    return new Models.OrderCreate.SelectedOfferItem()
                    {
                        PaxRefID = paxRefIds,
                        OfferItemRefID = offerNode.SelectSingleNode(xPath, airShoppingResponseXmlnsManager)?.InnerText ?? string.Empty
                    };
                }).ToList()
            });

            var totalSeatPrice = 0d;
            var seatOfferRefID = string.Empty;
            //Adding seats if required
            if (seat != SeatPriceType.None)
            {
                var allocatedSeats = FindAndReturnDataHelper.FindSeats(seatAvailabilityXml, seat);
                seatOfferRefID = allocatedSeats.FirstOrDefault()?.OfferID;
                if (allocatedSeats.Any())
                {
                    var selectedPricedOffers = new Models.OrderCreate.SelectedPricedOffer()
                    {
                        OfferRefID = allocatedSeats.First().OfferID,
                        OwnerCode = "VS",
                        TotalOfferPriceAmount = new()
                        {
                            CurCode = currencyCode,
                            Text = allocatedSeats.Sum(s => (double)s.UnitAmount).ToString()
                        },
                        SelectedOfferItem = allocatedSeats.Select(x => new Models.OrderCreate.SelectedOfferItem()
                        {
                            OfferItemRefID = x.OfferItemRefID,
                            PaxRefID = new() { x.PaxRefID },
                            SelectedSeat = new()
                            {
                                ColumnID = x.ColumnID,
                                SeatRowNumber = x.RowNumber
                            }
                        }).ToList()
                    };
                    instance.Request.CreateOrder.AcceptSelectedQuotedOfferList.SelectedPricedOffer.Add(selectedPricedOffers);
                    totalSeatPrice = selectedPricedOffers.TotalOfferPriceAmount.Text.Parse<double>();
                }

                instance.Request.PaymentFunctions.OfferAssociation.OfferRefID = seatOfferRefID ?? string.Empty;
                instance.Request.PaymentFunctions.OfferAssociation.OwnerCode = "VS";

            }
            //adding services if requireed
            if (!string.IsNullOrEmpty(serviceList))
            {
                XmlDocument ServicelistRS = new XmlDocument();
                ServicelistRS.LoadXml(serviceListRS);

                var serviceOfferID = ServicelistRS.SelectSingleNode("//def:ALaCarteOffer//def:OfferID", airShoppingRequestXmlnsManager).InnerText;

                XmlNodeList serviceListOfferItemIDs = ServicelistRS.SelectNodes("//def:OfferItem", airShoppingRequestXmlnsManager);

                List<SelectedOfferItem> sOff = new List<SelectedOfferItem>();

                if (serviceListOfferItemIDs.Count >= serviceCount)
                {
                    foreach (XmlNode offerItem in serviceListOfferItemIDs)
                    {
                        serviceCount--;
                        var offerItemID = offerItem.SelectSingleNode("def:OfferItemID", airShoppingRequestXmlnsManager)?.InnerText;
                        sOff.Add(
                        new Models.OrderCreate.SelectedOfferItem()
                        {
                            OfferItemRefID = offerItemID,
                            PaxRefID = offerItem.SelectNodes("def:Eligibility//def:PaxRefID", airShoppingRequestXmlnsManager).OfType<XmlNode>().Select(x => x.InnerText).ToList(),
                            SelectedALaCarteOfferItem = new()
                            {
                                OfferFlightAssociations = new Models.OrderCreate.OfferFlightAssociations()
                                {
                                    PaxSegmentReferences = new() { PaxSegmentRefID = offerItem.SelectNodes("def:Eligibility//def:OfferFlightAssociations//def:PaxSegmentReferences//def:PaxSegmentRefID", airShoppingRequestXmlnsManager).OfType<XmlNode>().Select(x => x.InnerText).ToList() },
                                },
                                Qty = "1"
                            }
                        }
                        );
                        if (serviceCount == 0)
                            break;
                    }
                    instance.Request.CreateOrder.AcceptSelectedQuotedOfferList.SelectedPricedOffer.Add(new()
                    {
                        OfferRefID = serviceOfferID,
                        OwnerCode = "VS",
                        SelectedOfferItem = sOff
                    });
                }
            }

            //Adding Contact Info List
            XmlNodeList node1 = requestXmlDocument.SelectNodes("//def:Pax", airShoppingRequestXmlnsManager);

            if (string.IsNullOrEmpty(countryDialingCode))
            {
                countryDialingCode = "44";
            }

            instance.Request.DataLists.ContactInfoList.ContactInfo.AddRange(Enumerable.Range(1, node1.Count).Select(i => new Models.OrderCreate.ContactInfo()
            {
                ContactInfoID = $"CID_{i}",

                Phone = new()
                {
                    ContactTypeText = "Home",
                    CountryDialingCode = countryDialingCode,
                    PhoneNumber = "1234567891",
                },
                EmailAddress = new()
                {
                    // EmailAddressText = $"{DataGenerator.RandomString(5)}@gmail.com",
                    EmailAddressText = "test.skyscanner@fly.virgin.com"
                },

                PostalAddress = new()
                {
                    CityName = "DENVER",
                    CountryCode = "USA",
                    CountrySubDivisionName = "KS",
                    PostalCode = 80014,
                    StreetText = "365 DOVONOVON FIRST LAKE"
                }
            }));

            double addingTotalAmount = totalAmount.Parse<double>() + totalSeatPrice;

            addingTotalAmount = Math.Round(addingTotalAmount, 2);

            AddingPaymentDetailsOrderCreateRq(currencyCode, addingTotalAmount, instance, paymentMethod, accountNumber, offerRefId);

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "OrderCreate", flyingClubMember: flyingClubMember, gst: gst, addressAtDestination: addressAtDestination, paymentMethod: paymentMethod, accountNumber: accountNumber, seat: seat, seatPaymentCardType: seatPaymentCardType);
        }

        public static string GenerateOrderReshopRequest(string airShoppingRequest, string airShoppingResult = "", string orderId = "", string bookingId = "", int countOrderItemID = 0, JourneyType journeyType = JourneyType.OneWay, LocationType locationOne = LocationType.None, LocationType locationTwo = LocationType.None, LocationType locationThree = LocationType.None, LocationType locationFour = LocationType.None, LocationType locationFive = LocationType.None, LocationType locationSix = LocationType.None, string newCabinTypeCodeOne = "", string newCabinTypeCodeTwo = "", string newCabinTypeCodeThree = "", string deptDate = "", string dateTwo = "", string datethree = "", string flighttype = "", string flow = "evenExchange", string paxRefID = "", string orderCreateRS = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderReshopRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();

            requestXmlDocument.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = requestXmlDocument.CreateXmlNamespaceManager();

            var agentNode = requestXmlDocument.SelectSingleNode("//msg:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderReshop.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderReshop.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderReshop.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Setting the values for BookingRef
            instance.Request.OrderRefID = orderId;

            if (flow.Is("cancel"))
            {
                instance.Request.UpdateOrder.CancelOrderRef.OrderRefID = orderId;
            }

            if (flow.Is("updatePaxName"))
            {
                XmlDocument responseXmlDocument = new XmlDocument();
                responseXmlDocument.LoadXml(orderCreateRS);

                var givenName = responseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS//msg:Response//ct:DataLists//ct:PaxList//ct:Pax[ct:PaxID='{paxRefID}']//ct:Individual//ct:GivenName", airShoppingRequestXmlnsManager).InnerText;

                var surname = responseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS//msg:Response//ct:DataLists//ct:PaxList//ct:Pax[ct:PaxID='{paxRefID}']//ct:Individual//ct:Surname", airShoppingRequestXmlnsManager).InnerText;

                instance.Request.BookingRef.BookingID = bookingId;
                instance.Request.UpdateOrder.ReshopOrder.ReshopOrderChoice.UpdatePaxName.GivenName = DataGenerator.RandomString(2) + givenName + DataGenerator.RandomString(2);
                instance.Request.UpdateOrder.ReshopOrder.ReshopOrderChoice.UpdatePaxName.PaxRefID = paxRefID;
                instance.Request.UpdateOrder.ReshopOrder.ReshopOrderChoice.UpdatePaxName.Surname = DataGenerator.RandomString(2) + surname + DataGenerator.RandomString(1);
            }

            if (flow.Is("evenExchange"))
            {
                instance.Request.BookingRef.BookingID = bookingId;

                switch (journeyType)
                {
                    case JourneyType.OneWay:
                        instance.Request.UpdateOrder.ReshopOrder.ReshopOrderChoice.ServiceOrder.AddOfferItems.FlightRequest
                            .FlightRequestOriginDestinationsCriteria.OriginDestCriteria.AddRange(new[]
                        {
                        new Models.OrderReshop.OriginDestCriteria()
                        {
                            OriginDepCriteria = new Models.OrderReshop.OriginDepCriteria()
                            {
                               IATA_LocationCode = locationOne.ToString(),
                               Date = deptDate
                            },
                           DestArrivalCriteria = new Models.OrderReshop.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationTwo.ToString()
                           },
                           CabinType = new()
                           {
                               new Models.OrderReshop.CabinType()
                               {
                                   CabinTypeCode = newCabinTypeCodeOne,
                                   PrefLevel = new()
                                   {
                                       PrefLevelCode = "Required"
                                   }
                               }
                           }
                        }
                        });
                        break;
                    case JourneyType.Return:
                        instance.Request.UpdateOrder.ReshopOrder.ReshopOrderChoice.ServiceOrder.AddOfferItems.FlightRequest
                            .FlightRequestOriginDestinationsCriteria.OriginDestCriteria.AddRange(new[]
                        {
                        new Models.OrderReshop.OriginDestCriteria()
                        {
                           OriginDepCriteria = new Models.OrderReshop.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationOne.ToString(),
                               Date = deptDate
                           },
                           DestArrivalCriteria = new Models.OrderReshop.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationTwo.ToString()
                           },
                           CabinType = new()
                           {
                               new Models.OrderReshop.CabinType()
                               {
                                   CabinTypeCode = newCabinTypeCodeOne,
                                   PrefLevel = new()
                                   {
                                       PrefLevelCode = "Required"
                                   }
                               }
                           }
                        },
                       new Models.OrderReshop.OriginDestCriteria()
                       {
                           OriginDepCriteria = new Models.OrderReshop.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationThree.ToString(),
                               Date = dateTwo
                           },
                           DestArrivalCriteria = new Models.OrderReshop.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationFour.ToString()
                           },
                           CabinType = new()
                           {
                               new Models.OrderReshop.CabinType()
                               {
                                   CabinTypeCode = newCabinTypeCodeTwo,
                                   PrefLevel = new()
                                   {
                                       PrefLevelCode = "Required"
                                   }
                               }
                           }
                       }
                        });
                        break;
                    case JourneyType.MultiCity:
                        instance.Request.UpdateOrder.ReshopOrder.ReshopOrderChoice.ServiceOrder.AddOfferItems.FlightRequest
                            .FlightRequestOriginDestinationsCriteria.OriginDestCriteria.AddRange(new[]
                        {
                        new Models.OrderReshop.OriginDestCriteria()
                        {
                           OriginDepCriteria = new Models.OrderReshop.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationOne.ToString(),
                               Date = deptDate
                           },
                           DestArrivalCriteria = new Models.OrderReshop.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationTwo.ToString()
                           },
                           CabinType = new()
                           {
                               new Models.OrderReshop.CabinType()
                               {
                                   CabinTypeCode = newCabinTypeCodeOne,
                                   PrefLevel = new()
                                   {
                                       PrefLevelCode = "Required"
                                   }
                               }
                           }
                        },
                       new Models.OrderReshop.OriginDestCriteria()
                       {
                           OriginDepCriteria = new Models.OrderReshop.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationThree.ToString(),
                               Date = dateTwo
                           },
                           DestArrivalCriteria = new Models.OrderReshop.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationFour.ToString()
                           },
                           CabinType = new()
                           {
                               new Models.OrderReshop.CabinType()
                               {
                                   CabinTypeCode = newCabinTypeCodeTwo,
                                   PrefLevel = new()
                                   {
                                       PrefLevelCode = "Required"
                                   }
                               }
                           }
                       },
                       new Models.OrderReshop.OriginDestCriteria()
                       {
                           OriginDepCriteria = new Models.OrderReshop.OriginDepCriteria()
                           {
                               IATA_LocationCode = locationFive.ToString(),
                               Date = datethree
                           },
                           DestArrivalCriteria = new Models.OrderReshop.DestArrivalCriteria()
                           {
                               IATA_LocationCode = locationSix.ToString()
                           },
                           CabinType = new()
                           {
                               new Models.OrderReshop.CabinType()
                               {
                                   CabinTypeCode = newCabinTypeCodeThree,
                                   PrefLevel = new()
                                   {
                                       PrefLevelCode = "Required"
                                   }
                               }
                           }
                       }
                        });
                        break;
                    default:
                        break;
                }
                instance.Request.UpdateOrder.ReshopOrder.ReshopOrderChoice.ServiceOrder.DeleteOrderItem
                    .AddRange(Enumerable.Range(1, countOrderItemID).Select(i => new Models.OrderReshop.DeleteOrderItem()
                    {
                        OrderItemRefID = $"{orderId}-{i}",
                    }));

                instance.Request.UpdateOrder.ReshopOrder.ReshopOrderChoice.ServiceOrder.AddOfferItems.FlightRelatedCriteria.FlightCriteria.FlightCharacteristicsCriteria.CharacteristicCode = "Direct";
                instance.Request.UpdateOrder.ReshopOrder.ReshopOrderChoice.ServiceOrder.AddOfferItems.FlightRelatedCriteria.FlightCriteria.FlightCharacteristicsCriteria.PrefLevel.PrefLevelCode = "Required";
            }

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "reshopRequest", flighttype: flighttype, reshopFlow: flow);
        }

        public static string GenerateOrderQuoteRequest(string airShoppingRequest, string orderReshopResult, string offerRefId, string orderId)
        {
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager orderReshopResponseXmlnsManager = new XmlNamespaceManager(orderReshopResponseXmlDocument.NameTable);
            orderReshopResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderReshopResponseXmlDocument.LoadXml(orderReshopResult);

            var offerNode = orderReshopResponseXmlDocument.SelectSingleNode($"//def:Offer[def:OfferID='{offerRefId}']", orderReshopResponseXmlnsManager);

            XmlDocument requestXmlDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderQuoteRequest();

            orderReshopResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", orderReshopResponseXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderQuote.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//def:DistributionChainLink", orderReshopResponseXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderQuote.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderQuote.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding Payload Existing Order 
            instance.Payload.ExistingOrder.OrderID = orderId;
            instance.Payload.ExistingOrder.OwnerCode = "VS";

            //Adding selected offers
            instance.Payload.SelectedOffers.OfferRefID = offerRefId;
            instance.Payload.SelectedOffers.OwnerCode = "VS";

            var paxNode = requestXmlDocument.SelectNodes("//def:PaxList/def:Pax", airShoppingRequestXmlManager);

            instance.Payload.PaxList = paxNode.OfType<XmlNode>()
                                              .Select(x => new Models.OrderQuote.PaxList()
                                              {
                                                  PaxID = x.SelectSingleNode("def:PaxID", airShoppingRequestXmlManager).InnerText,
                                                  PTC = x.SelectSingleNode("def:PTC", airShoppingRequestXmlManager).InnerText
                                              })
                                              .ToList();

            instance.Payload.SelectedOffers.SelectedOfferItem = instance.Payload.PaxList.GroupBy(x => x.PTC).Select(x =>
            {
                var paxRefIds = x.Select(p => p.PaxID).ToList();

                string condition = string.Join(" and ", paxRefIds.Select(x => $"def:Service/def:PaxRefID = '{x}'"));

                var xPath = $"def:AddedOfferItem[{condition}]/def:OfferItemID";

                return new Models.OrderQuote.SelectedOfferItem()
                {
                    PaxRefID = paxRefIds,
                    OfferItemRefID = offerNode.SelectSingleNode(xPath, orderReshopResponseXmlnsManager)?.InnerText ?? string.Empty
                };
            }).ToList();



            return XMLSerialize.Serialize(instance);
        }

        public static string GenerateOrderChangeRequest(string airShoppingRequest, string orderQuoteResult, string reshopOfferRefId, string currencyCode, string totalAmount, string bookingId, string orderId, string gst = "", string passport = "", string greencard = "", string visa = "", string knowntraveller = "", string redress = "", string apiInfo = "", string orderCreateRequest = "", string orderChangePayment = "", string accountNumber = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderChangeRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager orderQuoteResponseXmlnsManager = new XmlNamespaceManager(orderQuoteResponseXmlDocument.NameTable);
            orderQuoteResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            var offerNode = orderQuoteResponseXmlDocument.SelectSingleNode($"//def:RequotedOffer[def:OfferID='{reshopOfferRefId}']", orderQuoteResponseXmlnsManager);

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            orderQuoteResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            // Extract existing DistributionChain from airShoppingRequest
            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", orderQuoteResponseXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderChange.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//def:DistributionChainLink", orderQuoteResponseXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderChange.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderChange.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });


            //Adding Booking ID & offer
            instance.Request.BookingRef.BookingID = bookingId;
            instance.Request.ChangeOrderChoice.AcceptSelectedQuotedOfferList.SelectedPricedOffer.OfferRefID = reshopOfferRefId;
            instance.Request.ChangeOrderChoice.AcceptSelectedQuotedOfferList.SelectedPricedOffer.OwnerCode = "VS";

            //Adding Order details
            instance.Request.Order.OrderID = orderId;
            instance.Request.Order.OwnerCode = "VS";

            //Adding PAX 

            var paxNode = requestXmlDocument.SelectSingleNode("//def:PaxList", airShoppingRequestXmlnsManager).OuterXml;

            instance.Request.DataLists.PaxList = XMLSerialize.Deserialize<Models.AirShopping.PaxList, Models.OrderChange.PaxList>(paxNode);

            foreach (var p in instance.Request.DataLists.PaxList.Pax)
            {
                if (!string.IsNullOrWhiteSpace(gst))
                {
                    p.Employer = new()
                    {
                        Name = "TCS",
                        SalesTaxRegistration = new()
                        {
                            CityName = "Chennai",
                            CountryCode = "IND",
                            CountryDialingCode = "91",
                            EmailAddressText = "abc@tcs.com",
                            PhoneNumber = "12345",
                            PostalCode = "6000",
                            SalesTaxRegistrationID = "11223",
                            StreetText = new List<string> { "Navaluroad", "Siruseri" },
                        },
                        TaxDeductedatSourceCapabilityInd = "true",
                    };
                }

                if (!string.IsNullOrWhiteSpace(passport) && !string.IsNullOrWhiteSpace(visa))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChange.IdentityDoc()
                        {
                            CitizenshipCountryCode="USA",
                            ExpiryDate="2026-06-03",
                            GivenName="DAVID",
                            IdentityDocID="12345678",
                            IdentityDocTypeCode="PT",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="SMITH",
                            Visa=new Models.OrderChange.Visa{ExpiryDate="2026-06-03",HostCountryCode="USA",IssuingCountryCode="USA",VisaID="001001"},

                        },

                    });
                }

                if (!string.IsNullOrWhiteSpace(passport) && string.IsNullOrWhiteSpace(visa))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChange.IdentityDoc()
                        {
                            CitizenshipCountryCode="USA",
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="12345678",
                            IdentityDocTypeCode="PT",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="Smith",

                        },

                    });

                }

                if (!string.IsNullOrWhiteSpace(greencard))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChange.IdentityDoc()
                        {
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="USA0001234",
                            IdentityDocTypeCode="GC",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="Smith"
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(knowntraveller))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChange.IdentityDoc()
                        {
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="000011111",
                            IdentityDocTypeCode="CR",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            Surname="Smith"
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(redress))
                {
                    p.RedressCase = new Models.OrderChange.RedressCase { CountryCode = "USA", RedressCaseID = "123456" };
                }

            }

            //Adding Multiple selected offers

            instance.Request.ChangeOrderChoice.AcceptSelectedQuotedOfferList.SelectedPricedOffer.SelectedOfferItem = instance.Request.DataLists.PaxList.Pax.GroupBy(x => x.PTC).Select(x =>
            {
                var paxRefIds = x.Select(p => p.PaxID).ToList();

                string condition = string.Join(" and ", paxRefIds.Select(x => $"def:Service/def:PaxRefID = '{x}'"));

                var xPath = $"def:AddedOfferItem[{condition}]/def:OfferItemID";

                return new Models.OrderChange.SelectedOfferItem()
                {
                    PaxRefID = paxRefIds,
                    OfferItemRefID = offerNode.SelectSingleNode(xPath, orderQuoteResponseXmlnsManager)?.InnerText ?? string.Empty
                };

            }).ToList();

            AddingPaymentDetailsOrderChangeRq(currencyCode, totalAmount, instance, orderChangePayment, accountNumber);

            var exchangeType = orderQuoteResponseXmlDocument.SelectSingleNode($"ct:IATA_OrderReshopRS/ct:Response/def:ReshopResults/def:ReshopOffers/def:RequotedOffer/def:AddedOfferItem[1]/def:ReshopPrice/def:PriceDifferential/def:DifferentialTypeCode", orderQuoteResponseXmlnsManager).InnerText;

            if (exchangeType.Is("EvenExchange"))
            {
                instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.OfflinePayment.PaymentTypeCode = "OT";

                instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.OfflinePayment.PaymentBrandCode = "OT";
            }

            // **Remove Secure Version if Payment Type is Offline**
            string finalXml = XMLSerialize.Serialize(instance);

            if (orderChangePayment.Contains("offline", StringComparison.OrdinalIgnoreCase))
            {
                XmlDocument finalXmlDoc = new XmlDocument();
                finalXmlDoc.LoadXml(finalXml);

                XmlNode secureProgram = finalXmlDoc.SelectSingleNode("//def:PaymentFunctions//def:PaymentProcessingDetails//def:PaymentCard//def:SecureProgram", airShoppingRequestXmlnsManager);

                if (secureProgram != null && secureProgram.ParentNode != null)
                {
                    secureProgram.ParentNode.RemoveChild(secureProgram);
                }

                finalXml = finalXmlDoc.OuterXml;
            }

            return GenerateUpdatedRequest(finalXml, apiRequest: "OrderChange", gst: gst, paymentMethod: orderChangePayment, exchangeType: exchangeType, accountNumber: accountNumber);
        }

        public static string GenerateOrderQuoteRequestHoldBooking(string airShoppingRequest, string airShoppingResult, string orderId)
        {
            //Creating the instance of the object

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            var instance = XmlRequestBuilder.IATA_OrderQuoteHoldBookingRequest();

            airShoppingResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", airShoppingResponseXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderQuoteHoldBooking.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//def:DistributionChainLink", airShoppingResponseXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderQuoteHoldBooking.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderQuoteHoldBooking.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding Payload Existing Order 
            instance.Payload.RepricedOrder.OrderID = orderId;
            instance.Payload.RepricedOrder.OwnerCode = "VS";

            return XMLSerialize.Serialize(instance);

        }

        public static string GenerateOrderChangeHoldBookingRequest(string airShoppingRequest, string orderQuoteResult, string currencyCode, string amount, string bookingId, string orderId, string paymentType = "", string gst = "", string passport = "", string greencard = "", string visa = "", string knowntraveller = "", string redress = "", string apiInfo = "", string accountNumber = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderChangeRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument orderQuoteResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager orderQuoteResponseXmlnsManager = new XmlNamespaceManager(orderQuoteResponseXmlDocument.NameTable);
            orderQuoteResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            orderQuoteResponseXmlDocument.LoadXml(orderQuoteResult);

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            orderQuoteResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            // Extract existing DistributionChain from airShoppingRequest
            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", orderQuoteResponseXmlnsManager).OuterXml;

            // Deserialize the agentNode into DistributionChain
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderChange.DistributionChain>(agentNode);

            // Ensure there is at least one DistributionChainLink
            if (!instance.DistributionChain.DistributionChainLink.Any())
            {
                instance.DistributionChain.DistributionChainLink.Add(new Models.OrderChange.DistributionChainLink());
            }

            // Get the first DistributionChainLink
            var distributionChainLink = instance.DistributionChain.DistributionChainLink.First();

            // Add ContactInfo to the existing DistributionChainLink
            distributionChainLink.ContactInfo = new Models.OrderChange.ContactInfo
            {
                ContactInfoID = "AgentContactInfoID",
                EmailAddress = new Models.OrderChange.EmailAddress
                {
                    EmailAddressText = "agentemail@gmail.com"
                }
            };

            // Ensure serialization puts ContactInfo before Ordinal (reordering logic)
            distributionChainLink.Ordinal = 1.ToString(); // Set Ordinal value explicitly, but it will appear after ContactInfo
                                                          // Dynamically extract OrgID from airShoppingRequest or orderQuoteResult
            string orgId = ""; // Initialize OrgID variable

            // Attempt to extract OrgID from airShoppingRequest
            var orgIdNode = requestXmlDocument.SelectSingleNode("//def:OrgID", airShoppingRequestXmlnsManager);
            if (orgIdNode != null)
            {
                orgId = orgIdNode.InnerText;
            }

            distributionChainLink.ParticipatingOrg = new Models.OrderChange.ParticipatingOrg
            {
                Name = "Skyscanner",
                OrgID = orgId,
            };


            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//def:DistributionChain//def:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderChange.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderChange.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding Booking ID & offer
            instance.Request.BookingRef.BookingID = bookingId;

            //Adding Order details
            instance.Request.Order.OrderID = orderId;
            instance.Request.Order.OwnerCode = "VS";

            //Adding PAX 
            var paxNode = requestXmlDocument.SelectSingleNode("//def:PaxList", airShoppingRequestXmlnsManager).OuterXml;
            instance.Request.DataLists.PaxList = XMLSerialize.Deserialize<Models.AirShopping.PaxList, Models.OrderChange.PaxList>(paxNode);
            instance.Request.DataLists.PaxList.Pax.ForEach(p =>
            {
                var index = instance.Request.DataLists.PaxList.Pax.IndexOf(p);
                p.ContactInfoRefID = $"CID_{index + 1}";

                p.Individual = new()
                {
                    //infant <2 years, for child <12, for youth <18
                    Birthdate = GenerateDataHelper.DateOfBirth(p.PTC),
                    GenderCode = GenerateDataHelper.Gender(),
                    Surname = $"Smith{DataGenerator.RandomString(5)}"
                    //LoyaltyProgramAccount may need to be added as / when required
                };
            });

            instance.Request.DataLists.PaxList = XMLSerialize.Deserialize<Models.AirShopping.PaxList, Models.OrderChange.PaxList>(paxNode);

            foreach (var p in instance.Request.DataLists.PaxList.Pax)
            {
                if (!string.IsNullOrWhiteSpace(gst))
                {
                    p.Employer = new()
                    {
                        Name = "TCS",
                        SalesTaxRegistration = new()
                        {
                            CityName = "Chennai",
                            CountryCode = "IND",
                            CountryDialingCode = "91",
                            EmailAddressText = "abc@tcs.com",
                            PhoneNumber = "12345",
                            PostalCode = "6000",
                            SalesTaxRegistrationID = "11223",
                            StreetText = new List<string> { "Navaluroad", "Siruseri" },
                        },
                        TaxDeductedatSourceCapabilityInd = "true",
                    };
                }

                if (!string.IsNullOrWhiteSpace(passport) && !string.IsNullOrWhiteSpace(visa))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChange.IdentityDoc()
                        {
                            CitizenshipCountryCode="USA",
                            ExpiryDate="2036-06-03",
                            GivenName="DAVID",
                            IdentityDocID="ABCD45678",
                            IdentityDocTypeCode="PT",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="SMITH",
                            Visa=new Models.OrderChange.Visa{ExpiryDate="2036-06-03",HostCountryCode="USA",IssuingCountryCode="USA",VisaID="001001"},
                        },
                    });
                }

                if (!string.IsNullOrWhiteSpace(passport) && string.IsNullOrEmpty(visa))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChange.IdentityDoc()
                        {
                            CitizenshipCountryCode="USA",
                            ExpiryDate="2036-06-03",
                            GivenName="DAVID",
                            IdentityDocID="12345678",
                            IdentityDocTypeCode="PT",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="SMITH",
                        },
                    });

                }

                if (!string.IsNullOrWhiteSpace(greencard))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChange.IdentityDoc()
                        {
                            ExpiryDate="2036-06-03",
                            GivenName="David",
                            IdentityDocID="USA0001234",
                            IdentityDocTypeCode="GC",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="Smith"
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(knowntraveller))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChange.IdentityDoc()
                        {
                            ExpiryDate="2036-06-03",
                            GivenName="David",
                            IdentityDocID="000011111",
                            IdentityDocTypeCode="CR",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            Surname="Smith"
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(redress))
                {
                    p.RedressCase = new Models.OrderChange.RedressCase { CountryCode = "USA", RedressCaseID = "123456" };
                }
            }
            ;

            //HERE

            var cardDetails = new NdcCardDetails();

            double totalAmount = double.Parse(amount);

            AddingPaymentDetailsOrderChangeRequestHoldBookingRq(currencyCode, totalAmount, instance, paymentType, accountNumber);

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(XMLSerialize.Serialize(instance));

            if (paymentType.Has("Offline"))
            {
                var secureProgram = updatedRequest.SelectSingleNode("//def:PaymentFunctions//def:PaymentProcessingDetails//def:PaymentCard//def:SecureProgram", airShoppingRequestXmlnsManager);

                secureProgram.ParentNode.RemoveChild(secureProgram);

                return updatedRequest.OuterXml;
            }

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "OrderChangeConfirmHeldBooking", accountNumber: accountNumber, paymentMethod: paymentType);
        }

        public static string GenerateOrderRetrieveRequest(string airShoppingRequest, string orderCreateResult)
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderRetrieveRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

            XmlNamespaceManager xmlNamespaceManager = orderCreateResponseXmlDocument.CreateXmlNamespaceManager();

            string bookingId = orderCreateResponseXmlDocument.SelectSingleNode("//ct:BookingID", xmlNamespaceManager).InnerText;

            string orderId = orderCreateResponseXmlDocument.SelectSingleNode("//ct:OrderID", xmlNamespaceManager).InnerText;

            requestXmlDocument.LoadXml(airShoppingRequest);

            var agentNode = requestXmlDocument.SelectSingleNode("//msg:DistributionChain", xmlNamespaceManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderRetrieve.DistributionChain>(agentNode);

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
               new Models.OrderRetrieve.DistributionChainLink()
               {
                  ContactInfo= new Models.OrderRetrieve.ContactInfo()
                  {
                      ContactInfoID = "AgentContactInfoID",
                      EmailAddress=new Models.OrderRetrieve.EmailAddress()
                      {
                          EmailAddressText="agentemail@gmail.com"
                      }
                  }

               }
            });

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChainLink", xmlNamespaceManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderRetrieve.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderRetrieve.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding Booking ID 
            instance.Request.OrderValidationFilterCriteria.BookingRefFilterCriteria.BookingID = bookingId;
            instance.Request.OrderValidationFilterCriteria.BookingRefFilterCriteria.Carrier.AirlineDesigCode = "VS";

            //Adding Order ID
            instance.Request.OrderValidationFilterCriteria.OrderFilterCriteria.OrderID = orderId;
            instance.Request.OrderValidationFilterCriteria.OrderFilterCriteria.OwnerCode = "VS";

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "OrderRetrieve");
        }

        public static string GenerateOrderChangeSeatRequest(string airShoppingRequest, string seatAvailabilityResponse, string currencyCode, string totalAmount, string bookingId, string orderId, SeatPriceType seat = default, string paymentMethod = "", string gst = "", string passport = "", string greencard = "", string visa = "", string knowntraveller = "", string redress = "", string apiInfo = "", string accountNumber = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderChangeSeatRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument seatAvailabilityResponseDocument = new XmlDocument();

            XmlNamespaceManager seatAvailabilityResponseXmlnsManager = new XmlNamespaceManager(seatAvailabilityResponseDocument.NameTable);
            seatAvailabilityResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            seatAvailabilityResponseDocument.LoadXml(seatAvailabilityResponse);

            var offerNode = seatAvailabilityResponseDocument.SelectSingleNode($"//def:ALaCarteOffer//def:OfferID", seatAvailabilityResponseXmlnsManager);

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            seatAvailabilityResponseXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            // Extract existing DistributionChain from airShoppingRequest
            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", seatAvailabilityResponseXmlnsManager).OuterXml;

            // Deserialize the agentNode into DistributionChain
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderChangeSeat.DistributionChain>(agentNode);

            // Ensure there is at least one DistributionChainLinkordercreateresult
            if (!instance.DistributionChain.DistributionChainLink.Any())
            {
                instance.DistributionChain.DistributionChainLink.Add(new Models.OrderChangeSeat.DistributionChainLink());
            }

            // Get the first DistributionChainLink
            var distributionChainLink = instance.DistributionChain.DistributionChainLink.First();

            // Add ContactInfo to the existing DistributionChainLink
            distributionChainLink.ContactInfo = new Models.OrderChangeSeat.ContactInfo
            {
                ContactInfoID = "AgentContactInfoID",
                EmailAddress = new Models.OrderChangeSeat.EmailAddress
                {
                    EmailAddressText = "agentemail@gmail.com"
                }
            };

            // Ensure serialization puts ContactInfo before Ordinal (reordering logic)
            distributionChainLink.Ordinal = 1.ToString(); // Set Ordinal value explicitly, but it will appear after ContactInfo
            // Dynamically extract OrgID from airShoppingRequest or orderQuoteResult
            string orgId = ""; // Initialize OrgID variable

            // Attempt to extract OrgID from airShoppingRequest
            var orgIdNode = requestXmlDocument.SelectSingleNode("//def:OrgID", airShoppingRequestXmlnsManager);
            if (orgIdNode != null)
            {
                orgId = orgIdNode.InnerText;
            }

            distributionChainLink.ParticipatingOrg = new Models.OrderChangeSeat.ParticipatingOrg
            {
                Name = "Skyscanner",
                OrgID = orgId,
            };

            //Adding Booking ID & offer
            instance.Request.BookingRef.BookingID = bookingId;

            //Adding Order details
            instance.Request.Order.OrderID = orderId;
            instance.Request.Order.OwnerCode = "VS";

            var paxNode = requestXmlDocument.SelectSingleNode("//def:PaxList", airShoppingRequestXmlnsManager).OuterXml;

            instance.Request.DataLists.PaxList = XMLSerialize.Deserialize<Models.AirShopping.PaxList, Models.OrderChangeSeat.PaxList>(paxNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//def:DistributionChain//def:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderChangeSeat.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderChangeSeat.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            foreach (var p in instance.Request.DataLists.PaxList.Pax)
            {
                if (!string.IsNullOrWhiteSpace(gst))
                {
                    p.Employer = new()
                    {
                        Name = "TCS",
                        SalesTaxRegistration = new()
                        {
                            CityName = "Chennai",
                            CountryCode = "IND",
                            CountryDialingCode = "91",
                            EmailAddressText = "abc@tcs.com",
                            PhoneNumber = "12345",
                            PostalCode = "6000",
                            SalesTaxRegistrationID = "11223",
                            StreetText = new List<string> { "Navaluroad", "Siruseri" },
                        },
                        TaxDeductedatSourceCapabilityInd = "true",
                    };
                }

                if (!string.IsNullOrWhiteSpace(passport) && !string.IsNullOrWhiteSpace(visa))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChangeSeat.IdentityDoc()
                        {
                            CitizenshipCountryCode="USA",
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="12345678",
                            IdentityDocTypeCode="PT",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="Smith",
                            Visa=new Models.OrderChangeSeat.Visa{ExpiryDate="2026-06-03",HostCountryCode="USA",IssuingCountryCode="USA",VisaID="001001"},

                        },

                    });
                }

                if (!string.IsNullOrWhiteSpace(passport) && string.IsNullOrWhiteSpace(visa))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChangeSeat.IdentityDoc()
                        {
                            CitizenshipCountryCode="USA",
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="12345678",
                            IdentityDocTypeCode="PT",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="Smith",

                        },

                    });

                }

                if (!string.IsNullOrWhiteSpace(greencard))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChangeSeat.IdentityDoc()
                        {
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="USA0001234",
                            IdentityDocTypeCode="GC",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="Smith"
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(knowntraveller))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChangeSeat.IdentityDoc()
                        {
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="000011111",
                            IdentityDocTypeCode="CR",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            Surname="Smith"
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(redress))
                {
                    p.RedressCase = new Models.OrderChangeSeat.RedressCase { CountryCode = "USA", RedressCaseID = "123456" };
                }

            }
            ;

            var totalSeatPrice = 0d;

            //Adding seats if required
            if (seat != SeatPriceType.None)
            {
                var allocatedSeats = FindAndReturnDataHelper.FindSeats(seatAvailabilityResponse, seat);

                if (allocatedSeats.Any())
                {
                    var selectedPricedOffers = new Models.OrderChangeSeat.SelectedPricedOffer()
                    {
                        OfferRefID = allocatedSeats.First().OfferID,
                        OwnerCode = "VS",
                        TotalOfferPriceAmount = new()
                        {
                            CurCode = currencyCode,
                            Text = allocatedSeats.Sum(s => (double)s.UnitAmount).ToString()
                        },
                        SelectedOfferItem = allocatedSeats.Select(x => new Models.OrderChangeSeat.SelectedOfferItem()
                        {
                            OfferItemRefID = x.OfferItemRefID,
                            PaxRefID = x.PaxRefID,
                            SelectedSeat = new()
                            {
                                ColumnID = x.ColumnID,
                                SeatRowNumber = x.RowNumber
                            }
                        }).ToList()
                    };
                    totalSeatPrice = selectedPricedOffers.TotalOfferPriceAmount.Text.Parse<double>();

                    instance.Request.ChangeOrderChoice.AcceptSelectedQuotedOfferList.SelectedPricedOffer.Add(selectedPricedOffers);
                }
            }

            AddingPaymentDetailsOrderChangeSeatsRq(currencyCode, totalSeatPrice, instance, paymentMethod, accountNumber);

            XmlDocument updatedRequest = new XmlDocument();

            updatedRequest.LoadXml(XMLSerialize.Serialize(instance));

            if (seat.Equals(SeatPriceType.Free))
            {
                instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.OfflinePayment.PaymentTypeCode = "OT";

                instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.OfflinePayment.PaymentBrandCode = "OT";
            }

            if (paymentMethod.Has("Offline"))
            {
                var secureProgram = updatedRequest.SelectSingleNode("//def:PaymentFunctions//def:PaymentProcessingDetails//def:PaymentCard//def:SecureProgram", airShoppingRequestXmlnsManager);

                secureProgram.ParentNode.RemoveChild(secureProgram);

                return updatedRequest.OuterXml;
            }

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "OrderChangeSeat", seat: seat, accountNumber: accountNumber, paymentMethod: paymentMethod);

        }

        public static string GenerateServiceListRequest(string airShoppingRequest, string offerId = "", string orderID = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_ServiceListRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument orderCreateResponseXmlDocument = new XmlDocument();

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            airShoppingRequestXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.ServiceList.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//def:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.ServiceList.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.ServiceList.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding selected offers
            instance.Request.ServiceListCoreRequest.OfferRequest.Offer.OfferID = offerId;
            instance.Request.ServiceListCoreRequest.OfferRequest.Offer.OwnerCode = "VS";

            //Adding Order ID
            instance.Request.ServiceListCoreRequest.OrderRequest.Order.OrderID = orderID;
            instance.Request.ServiceListCoreRequest.OrderRequest.Order.OwnerCode = "VS";

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "ServiceList", orderId: orderID, offerId: offerId);
            // return XMLSerialize.Serialize(instance);
        }

        public static string GenerateOrderChangeServiceRequest(string airShoppingRequest, string serviceListRS, string currencyCode, string bookingId, string orderId, string paymentMethod = "", string gst = "", string passport = "", string greencard = "", string visa = "", string knowntraveller = "", string redress = "", string apiInfo = "", string serviceList = "", int serviceCount = 1, string selectedService = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderChangeServiceRequest();

            XmlDocument ServicelistRS = new XmlDocument();
            ServicelistRS.LoadXml(serviceListRS);

            XmlDocument requestXmlDocument = new XmlDocument();
            requestXmlDocument.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = new XmlNamespaceManager(requestXmlDocument.NameTable);
            airShoppingRequestXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            requestXmlDocument.LoadXml(airShoppingRequest);

            airShoppingRequestXmlnsManager.AddNamespace("ct", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage");

            // Extract existing DistributionChain from airShoppingRequest
            var agentNode = requestXmlDocument.SelectSingleNode("//ct:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;

            // Deserialize the agentNode into DistributionChain
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderChangeService.DistributionChain>(agentNode);

            // Ensure there is at least one DistributionChainLink
            if (!instance.DistributionChain.DistributionChainLink.Any())
            {
                instance.DistributionChain.DistributionChainLink.Add(new Models.OrderChangeService.DistributionChainLink());
            }

            // Get the first DistributionChainLink
            var distributionChainLink = instance.DistributionChain.DistributionChainLink.First();

            // Add ContactInfo to the existing DistributionChainLink
            distributionChainLink.ContactInfo = new Models.OrderChangeService.ContactInfo
            {
                ContactInfoID = "AgentContactInfoID",
                EmailAddress = new Models.OrderChangeService.EmailAddress
                {
                    EmailAddressText = "agentemail@gmail.com"
                }
            };

            // Ensure serialization puts ContactInfo before Ordinal (reordering logic)
            distributionChainLink.Ordinal = 1.ToString(); // Set Ordinal value explicitly, but it will appear after ContactInfo
            // Dynamically extract OrgID from airShoppingRequest or orderQuoteResult
            string orgId = ""; // Initialize OrgID variable

            // Attempt to extract OrgID from airShoppingRequest
            var orgIdNode = requestXmlDocument.SelectSingleNode("//def:OrgID", airShoppingRequestXmlnsManager);
            if (orgIdNode != null)
            {
                orgId = orgIdNode.InnerText;
            }

            distributionChainLink.ParticipatingOrg = new Models.OrderChangeService.ParticipatingOrg
            {
                Name = "Skyscanner",
                OrgID = orgId,
            };

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//def:DistributionChain//def:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderChangeService.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderChangeService.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding Booking ID & offer
            instance.Request.BookingRef.BookingID = bookingId;

            //Adding Order details
            instance.Request.Order.OrderID = orderId;
            instance.Request.Order.OwnerCode = "VS";

            var paxNode = requestXmlDocument.SelectSingleNode("//def:PaxList", airShoppingRequestXmlnsManager).OuterXml;

            instance.Request.DataLists.PaxList = XMLSerialize.Deserialize<Models.AirShopping.PaxList, Models.OrderChangeService.PaxList>(paxNode);

            foreach (var p in instance.Request.DataLists.PaxList.Pax)
            {
                if (!string.IsNullOrWhiteSpace(gst))
                {
                    p.Employer = new()
                    {
                        Name = "TCS",
                        SalesTaxRegistration = new()
                        {
                            CityName = "Chennai",
                            CountryCode = "IND",
                            CountryDialingCode = "91",
                            EmailAddressText = "abc@tcs.com",
                            PhoneNumber = "12345",
                            PostalCode = "6000",
                            SalesTaxRegistrationID = "11223",
                            StreetText = new List<string> { "Navaluroad", "Siruseri" },
                        },
                        TaxDeductedatSourceCapabilityInd = "true",
                    };
                }

                if (!string.IsNullOrWhiteSpace(passport) && !string.IsNullOrWhiteSpace(visa))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChangeService.IdentityDoc()
                        {
                            CitizenshipCountryCode="USA",
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="12345678",
                            IdentityDocTypeCode="PT",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="Smith",
                            Visa=new Models.OrderChangeService.Visa{ExpiryDate="2026-06-03",HostCountryCode="USA",IssuingCountryCode="USA",VisaID="001001"},

                        },

                    });
                }

                if (!string.IsNullOrWhiteSpace(passport))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChangeService.IdentityDoc()
                        {
                            CitizenshipCountryCode="USA",
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="12345678",
                            IdentityDocTypeCode="PT",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="Smith",

                        },

                    });

                }

                if (!string.IsNullOrWhiteSpace(greencard))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChangeService.IdentityDoc()
                        {
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="USA0001234",
                            IdentityDocTypeCode="GC",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname="Smith"
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(knowntraveller))
                {
                    p.IdentityDoc.AddRange(new[]
                    {
                        new Models.OrderChangeService.IdentityDoc()
                        {
                            ExpiryDate="2026-06-03",
                            GivenName="David",
                            IdentityDocID="000011111",
                            IdentityDocTypeCode="CR",
                            IssueDate="2016-06-03",
                            IssuingCountryCode="USA",
                            Surname="Smith"
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(redress))
                {
                    p.RedressCase = new Models.OrderChangeService.RedressCase { CountryCode = "USA", RedressCaseID = "123456" };
                }

            }
            ;

            //Adding ServiceList
            var serviceOfferID = ServicelistRS.SelectSingleNode("//def:ALaCarteOffer//def:OfferID", airShoppingRequestXmlnsManager).InnerText;

            XmlNodeList serviceListOfferItemIDs = ServicelistRS.SelectNodes("//def:OfferItem", airShoppingRequestXmlnsManager);

            var serviceDefinitionID = ServicelistRS.SelectSingleNode($"//def:DataLists//def:ServiceDefinitionList//def:ServiceDefinition[def:ServiceCode='{selectedService}']//def:ServiceDefinitionID", airShoppingRequestXmlnsManager)?.InnerText;

            var selectedOfferItemID = ServicelistRS.SelectSingleNode($"//def:ALaCarteOffer//def:OfferItem[def:Service//def:ServiceDefinitionRefID='{serviceDefinitionID}']//def:OfferItemID", airShoppingRequestXmlnsManager)?.InnerText;

            List<Models.OrderChangeService.SelectedOfferItem> sOff = new List<Models.OrderChangeService.SelectedOfferItem>();

            if (serviceListOfferItemIDs.Count >= serviceCount)
            {
                foreach (XmlNode offerItem in serviceListOfferItemIDs)
                {
                    serviceCount--;
                    sOff.Add(
                    new Models.OrderChangeService.SelectedOfferItem()
                    {
                        OfferItemRefID = selectedOfferItemID ?? offerItem.SelectSingleNode("def:OfferItemID", airShoppingRequestXmlnsManager)?.InnerText,
                        PaxRefID = offerItem.SelectNodes("def:Eligibility//def:PaxRefID", airShoppingRequestXmlnsManager).OfType<XmlNode>().Select(x => x.InnerText).ToList(),
                        SelectedALaCarteOfferItem = new()
                        {
                            OfferFlightAssociations = new Models.OrderChangeService.OfferFlightAssociations()
                            {
                                PaxSegmentReferences = new() { PaxSegmentRefID = offerItem.SelectNodes("def:Eligibility//def:OfferFlightAssociations//def:PaxSegmentReferences//def:PaxSegmentRefID", airShoppingRequestXmlnsManager).OfType<XmlNode>().Select(x => x.InnerText).ToList() },
                            },
                            Qty = "1"
                        }
                    }
                    );
                    if (serviceCount == 0)
                        break;
                }
                instance.Request.ChangeOrderChoice.AcceptSelectedQuotedOfferList.SelectedPricedOffer.Add(new()
                {
                    OfferRefID = serviceOfferID,
                    OwnerCode = "VS",
                    SelectedOfferItem = sOff
                });
            }
            return XMLSerialize.Serialize(instance);
        }

        public static string GenerateOrderChangeCancelRequest(string airShoppingRequest, string orderId, string reshopRS)
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderChangeCancelRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            requestXmlDocument.LoadXml(airShoppingRequest);

            XmlDocument responseXmlDocument = new XmlDocument();
            responseXmlDocument.LoadXml(reshopRS);

            XmlNamespaceManager airShoppingRequestXmlnsManager = responseXmlDocument.CreateXmlNamespaceManager();

            var agentNode = requestXmlDocument.SelectSingleNode("//msg:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderChangeCancel.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//msg:DistributionChain//ct:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderChangeCancel.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderChangeCancel.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding Order details
            instance.Request.Order.OrderID = orderId;
            instance.Request.Order.OwnerCode = "VS";

            //Adding ServiceList
            var serviceOfferID = responseXmlDocument.SelectSingleNode("//ct:ReshopResults//ct:ReshopOffers//ct:Offer//ct:OfferID", airShoppingRequestXmlnsManager).InnerText;
            instance.Request.ChangeOrderChoice.AcceptCancelledOffer.OfferID = serviceOfferID;
            instance.Request.ChangeOrderChoice.AcceptCancelledOffer.OwnerCode = "VS";

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "OrderChangeCancel");

        }

        public static string GenerateOrderChangeSplitPnrRequest(string airShoppingRequest, string ordercreateRS, List<string> paxID)
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderChangeSplitPnrRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            requestXmlDocument.LoadXml(airShoppingRequest);

            XmlDocument responseXmlDocument = new XmlDocument();
            responseXmlDocument.LoadXml(ordercreateRS);

            XmlNamespaceManager airShoppingRequestXmlnsManager = responseXmlDocument.CreateXmlNamespaceManager();

            var agentNode = requestXmlDocument.SelectSingleNode("//msg:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;

            instance.AugmentationPoint.ChangeOrderChoice.UpdatePax = paxID
                .Select(id => new UpdatePax
                {
                    PaxSharingPref = new PaxSharingPref
                    {
                        PaxRefID = id
                    }
                })
                .ToList();

            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderchangeSplitPnr.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//ct:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderchangeSplitPnr.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderchangeSplitPnr.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding Order details
            instance.Request.Order.OrderID = "VS932" + FindAndReturnDataHelper.ReturnBookingId(ordercreateRS) + FindAndReturnDataHelper.GetYearSuffix();
            instance.Request.BookingRef.BookingID = FindAndReturnDataHelper.ReturnBookingId(ordercreateRS);
            instance.Request.Order.OwnerCode = "VS";

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "OrderChangeSplitPnr");

        }

        public static string GenerateOrderChangeNameCorrectionRequest(string airShoppingRequest, string reshopResult, string bookingID, string orderChangePayment = "CPDVisa", string accountNumber = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderChangeRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            XmlDocument orderReshopResponseXmlDocument = new XmlDocument();

            orderReshopResponseXmlDocument.LoadXml(reshopResult);

            XmlNamespaceManager xmlnsManager = orderReshopResponseXmlDocument.CreateXmlNamespaceManager();

            requestXmlDocument.LoadXml(airShoppingRequest);

            var agentNode = requestXmlDocument.SelectSingleNode("//msg:DistributionChain", xmlnsManager).OuterXml;
            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderChange.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//msg:DistributionChain//ct:DistributionChainLink", xmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderChange.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderChange.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            var offerID = orderReshopResponseXmlDocument.SelectSingleNode("//ct:ReshopResults//ct:ReshopOffers//ct:Offer//ct:OfferID", xmlnsManager).InnerText;

            var currencyCode = orderReshopResponseXmlDocument.SelectSingleNode("//ct:ReshopResults//ct:ReshopOffers//ct:Offer//ct:OfferItem//ct:Price//ct:TotalAmount[@CurCode]", xmlnsManager).Attributes["CurCode"].Value;

            //Adding Booking ID & offer
            instance.Request.BookingRef.BookingID = bookingID;
            instance.Request.ChangeOrderChoice.AcceptSelectedQuotedOfferList.SelectedPricedOffer.OfferRefID = offerID;
            instance.Request.ChangeOrderChoice.AcceptSelectedQuotedOfferList.SelectedPricedOffer.OwnerCode = "VS";

            //Adding Order details
            instance.Request.Order.OrderID = "VS932" + bookingID + FindAndReturnDataHelper.GetYearSuffix();
            instance.Request.Order.OwnerCode = "VS";

            //Adding PAX 

            var paxRefID = orderReshopResponseXmlDocument.SelectSingleNode("msg:IATA_OrderReshopRS/msg:Response/ct:ReshopResults/ct:ReshopOffers/ct:Offer/ct:OfferItem/ct:PaxNameChange/ct:PaxRefID", xmlnsManager)?.InnerText;

            var paxNode = requestXmlDocument.SelectSingleNode($"//ct:PaxList[ct:Pax/ct:PaxID='{paxRefID}']", xmlnsManager).OuterXml;

            instance.Request.DataLists.PaxList = XMLSerialize.Deserialize<Models.AirShopping.PaxList, Models.OrderChange.PaxList>(paxNode);

            instance.Request.DataLists.PaxList.Pax.RemoveAll(p => p.PaxID != paxRefID);

            //Adding Multiple selected offers
            instance.Request.ChangeOrderChoice.AcceptSelectedQuotedOfferList.SelectedPricedOffer.SelectedOfferItem =
            [
                new() {
                    OfferItemRefID = offerID + "-1",
                    PaxRefID = [paxRefID]
                }
            ];

            AddingPaymentDetailsOrderChangeRq(currencyCode, "0.00", instance, orderChangePayment, accountNumber);

            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.OfflinePayment.PaymentTypeCode = "OT";

            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.OfflinePayment.PaymentBrandCode = "OT";

            string finalXml = XMLSerialize.Serialize(instance);

            XmlDocument finalXmlDoc = new XmlDocument();
            finalXmlDoc.LoadXml(finalXml);

            XmlNode contact = finalXmlDoc.SelectSingleNode("//ct:DataLists//ct:ContactInfoList", xmlnsManager);

            contact.ParentNode.RemoveChild(contact);

            finalXml = finalXmlDoc.OuterXml;

            return GenerateUpdatedRequest(finalXml, apiRequest: "OrderChange", paymentMethod: orderChangePayment, exchangeType: "EvenExchange");
        }

        public static string GenerateOrderListRequest(string airShoppingRequest, string creationDateTimeFrom = "", string creationDateTimeTo = "", string lastModifiedDateTimeFrom = "", string lastModifiedDateTimeTo = "", string orderCancellationExpiryDate = "", string paymentStatusCode = "", string travelEffectiveDate = "", string travelExpirydate = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderListRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            requestXmlDocument.LoadXml(airShoppingRequest);

            XmlNamespaceManager airShoppingRequestXmlnsManager = requestXmlDocument.CreateXmlNamespaceManager();

            var agentNode = requestXmlDocument.SelectSingleNode("//msg:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;

            instance.DistributionChain = XMLSerialize.Deserialize<Models.OrderList.DistributionChain, Models.OrderList.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//ct:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderList.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderList.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });
            if (!string.IsNullOrEmpty(creationDateTimeFrom) && !string.IsNullOrEmpty(creationDateTimeTo))
            {
                instance.Request.OrderSearchFilterCriteria.CreationDateTimeFilterCriteria.FromDateTime = creationDateTimeFrom;
                instance.Request.OrderSearchFilterCriteria.CreationDateTimeFilterCriteria.ToDateTime = creationDateTimeTo;
            }
            if (!string.IsNullOrEmpty(lastModifiedDateTimeFrom) && !string.IsNullOrEmpty(lastModifiedDateTimeTo))
            {
                instance.Request.OrderSearchFilterCriteria.LastModifiedDateTimeFilterCriteria.FromDateTime = lastModifiedDateTimeFrom;
                instance.Request.OrderSearchFilterCriteria.LastModifiedDateTimeFilterCriteria.ToDateTime = lastModifiedDateTimeTo;
            }
            if (!string.IsNullOrEmpty(orderCancellationExpiryDate))
            {
                instance.Request.OrderSearchFilterCriteria.OrderCancellationDateFilterCriteria.ExpirationDate = orderCancellationExpiryDate;
            }
            if (!string.IsNullOrEmpty(paymentStatusCode))
            {
                instance.Request.OrderSearchFilterCriteria.PaymentFilterCriteria.PaymentStatusCode = paymentStatusCode;
            }
            if (!string.IsNullOrEmpty(travelEffectiveDate))
            {
                instance.Request.OrderSearchFilterCriteria.TravelDateFilterCriteria.EffectiveDate = travelEffectiveDate;
            }

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "OrderList");
        }

        private static void AddingPaymentDetailsOrderCreateRq(string currencyCode, double totalAmount, IATA_OrderCreateRQ instance, string paymentMethod, string accountNumber, string offerRefId)
        {
            var cardDetails = new NdcCardDetails();

            //Adding PaymentFunctions blocks
            instance.Request.PaymentFunctions.PaymentMethodCriteria.PaymentTypeCode = cardDetails.PaymentTypeCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.CurCode = currencyCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.Text = totalAmount.ToString();

            instance.Request.PaymentFunctions.OfferAssociation.OfferRefID = offerRefId;
            instance.Request.PaymentFunctions.OfferAssociation.OwnerCode = "VS";

            //Default values as they are the same in all cases
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardSecurityCode = cardDetails.CardSecurityCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.ExpirationDate = cardDetails.ExpirationDate;

            switch (paymentMethod)
            {
                case "Tokenized":

                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "Cash":
                    instance.Request.PaymentFunctions.PaymentMethodCriteria.PaymentTypeCode = "CA";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.SettlementPlan.PaymentTypeCode = "CA";
                    break;

                case "EasyPay":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.IATA_EasyPay.AccountNumber = accountNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.IATA_EasyPay.ExpirationDate = "1226";
                    break;

                case "TokenizedVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "42639X0MNOPQ5262";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4263970000005262";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "TokenizedAmex":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "37144Z6FIHO8431";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "AX";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedAmex":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "371449635398431";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "AX";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "TokenizedDiner":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "30569T0LNG5904";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "DC";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedDiner":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "30569309025904";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "DC";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4444333322221111";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDMaster":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "2221008123677736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDDiner":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "3612532972865030";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = "DC";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDDiscover":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "6509824493148858";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = "DS";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "Untokenized":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "5201281505129736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UnsecureTokenized":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                case "UnsecureUntokenized":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "5201281505129736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                case "UnsecureOfflineCPDVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4444333322221111";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = "TO";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = "VI";
                    break;

                case "UnsecureOfflineCPDMaster":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "2221008123677736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = "TO";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                case "UnsecureOfflineCPDDiner":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "3612532972865030";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = "TO";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = "DC";
                    break;

                case "UnsecureOfflineCPDDiscover":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "6509824493148858";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = "TO";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = "DS";
                    break;

                default:
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;
            }

            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.StreetText.AddRange(cardDetails.StreetText);
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.PostalCode = cardDetails.PostalCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CityName = cardDetails.CityName;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CountryCode = cardDetails.CountryCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.GivenName = cardDetails.GivenName;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.Surname = cardDetails.Surname;
        }

        private static void AddingPaymentDetailsOrderChangeSeatsRq(string currencyCode, double totalAmount, Models.OrderChangeSeat.IATA_OrderChangeSeatRQ instance, string paymentMethod, string accountNumber)
        {
            var cardDetails = new NdcCardDetails();

            //Adding PaymentFunctions blocks
            instance.Request.PaymentFunctions.PaymentMethodCriteria.PaymentTypeCode = cardDetails.PaymentTypeCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.CurCode = currencyCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.Text = totalAmount.ToString();

            //Default values are they are the same in all cases
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardSecurityCode = cardDetails.CardSecurityCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.ExpirationDate = cardDetails.ExpirationDate;


            switch (paymentMethod)
            {
                case "Tokenized":

                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "Cash":
                    instance.Request.PaymentFunctions.PaymentMethodCriteria.PaymentTypeCode = "CA";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.SettlementPlan.PaymentTypeCode = "CA";
                    break;

                case "EasyPay":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.IATA_EasyPay.AccountNumber = accountNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.IATA_EasyPay.ExpirationDate = "1226";
                    break;

                case "TokenizedVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "42639X0MNOPQ5262";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4263970000005262";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "TokenizedAmex":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "37144Z6FIHO8431";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "AX";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedAmex":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "371449635398431";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "AX";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "TokenizedDiner":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "30569T0LNG5904";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "DC";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedDiner":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "30569309025904";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "DC";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4444333322221111";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDMaster":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "2221008123677736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "Untokenized":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "5201281505129736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UnsecureTokenized":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                case "UnsecureUntokenized":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "5201281505129736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                case "UnsecureOfflineCPD":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4444333322221111";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                default:
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;
            }

            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.StreetText.AddRange(cardDetails.StreetText);
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.PostalCode = cardDetails.PostalCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CityName = cardDetails.CityName;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CountryCode = cardDetails.CountryCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.GivenName = cardDetails.GivenName;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.Surname = cardDetails.Surname;

            //instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerEmailAddress.EmailAddressText = "Test@fly.virgin.com";
            //instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerPhoneNumber.ContactTypeText = "PER";
            //instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerPhoneNumber.CountryDialingCode = "44";
            //instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerPhoneNumber.PhoneNumber = "9999999999";
            //instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PaymentAddress.PostalAddress.StreetText.AddRange(
            //    new string[] { "8", "Cobbles Crescent" });
            //instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PaymentAddress.PostalAddress.PostalCode = "RH108HA";
            //instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PaymentAddress.PostalAddress.CityName = "Crawley";
            //instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PaymentAddress.PostalAddress.CountryCode = "GB";
        }

        private static void AddingPaymentDetailsOrderChangeRq(string currencyCode, string totalAmount, Models.OrderChange.IATA_OrderChangeRQ instance, string paymentMethod, string accountNumber)
        {
            var cardDetails = new NdcCardDetails();

            //Adding PaymentFunctions blocks
            instance.Request.PaymentFunctions.PaymentMethodCriteria.PaymentTypeCode = cardDetails.PaymentTypeCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.CurCode = currencyCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.Text = totalAmount.ToString();

            //Default values are they are the same in all cases
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardSecurityCode = cardDetails.CardSecurityCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.ExpirationDate = cardDetails.ExpirationDate;

            switch (paymentMethod)
            {
                case "Tokenized":

                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "Cash":
                    instance.Request.PaymentFunctions.PaymentMethodCriteria.PaymentTypeCode = "CA";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.SettlementPlan.PaymentTypeCode = "CA";
                    break;

                case "EasyPay":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.IATA_EasyPay.AccountNumber = accountNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.IATA_EasyPay.ExpirationDate = "1226";
                    break;

                case "TokenizedVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "42639X0MNOPQ5262";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4263970000005262";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "TokenizedAmex":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "37144Z6FIHO8431";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "AX";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedAmex":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "371449635398431";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "AX";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "TokenizedDiner":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "30569T0LNG5904";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "DC";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedDiner":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "30569309025904";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "DC";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4444333322221111";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDMaster":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "2221008123677736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "Untokenized":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "5201281505129736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UnsecureTokenized":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                case "UnsecureUntokenized":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "5201281505129736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                case "UnsecureOfflineCPD":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4444333322221111";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                default:
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;
            }

            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.StreetText.AddRange(cardDetails.StreetText);
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.PostalCode = cardDetails.PostalCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CityName = cardDetails.CityName;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CountryCode = cardDetails.CountryCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.GivenName = cardDetails.GivenName;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.Surname = cardDetails.Surname;

        }

        private static void AddingPaymentDetailsOrderChangeRequestHoldBookingRq(string currencyCode, double totalAmount, Models.OrderChange.IATA_OrderChangeRQ instance, string paymentMethod, string accountNumber)
        {
            var cardDetails = new NdcCardDetails();

            //Adding PaymentFunctions blocks
            instance.Request.PaymentFunctions.PaymentMethodCriteria.PaymentTypeCode = cardDetails.PaymentTypeCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.CurCode = currencyCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Amount.Text = totalAmount.ToString();

            //Default values are they are the same in all cases
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardSecurityCode = cardDetails.CardSecurityCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.ExpirationDate = cardDetails.ExpirationDate;

            switch (paymentMethod)
            {
                case "Tokenized":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "Cash":
                    instance.Request.PaymentFunctions.PaymentMethodCriteria.PaymentTypeCode = "CA";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.SettlementPlan.PaymentTypeCode = "CA";
                    break;

                case "EasyPay":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.IATA_EasyPay.AccountNumber = accountNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.IATA_EasyPay.ExpirationDate = "1226";
                    break;

                case "TokenizedVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "42639X0MNOPQ5262";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4263970000005262";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "TokenizedAmex":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "37144Z6FIHO8431";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "AX";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedAmex":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "371449635398431";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "AX";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "TokenizedDiner":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "30569T0LNG5904";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "DC";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UntokenizedDiner":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "30569309025904";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "DC";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDVisa":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4444333322221111";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText = "VI";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd = "05";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "CPDMaster":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "2221008123677736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "Untokenized":
                    //Un-tonkenized when NOT on the virgin network
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "5201281505129736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;

                case "UnsecureTokenized":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                case "UnsecureUntokenized":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "5201281505129736";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                case "UnsecureOfflineCPD":
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber = "4444333322221111";
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.PaymentTrxChannelCode = cardDetails.PaymentTrxChannelCode;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    break;

                default:
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardNumber = cardDetails.CardNumber;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardTypeText = cardDetails.CardTypeText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.AuthenticationValue = cardDetails.AuthenticationValue;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.TrxStatusText = cardDetails.TrxStatusText;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.ElectronicCommerceInd = cardDetails.ElectronicCommerceInd;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecurePaymentVersion2.DirectoryServerTrxID = cardDetails.DirectoryServerTrxID;
                    instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.SecureProgram.EnrollmentStatusText = cardDetails.EnrollmentStatusText;
                    break;
            }

            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.StreetText.AddRange(cardDetails.StreetText);
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.PostalCode = cardDetails.PostalCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CityName = cardDetails.CityName;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.PaymentMethod.PaymentCard.CardholderAddress.CountryCode = cardDetails.CountryCode;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.GivenName = cardDetails.GivenName;
            instance.Request.PaymentFunctions.PaymentProcessingDetails.Payer.PayerName.IndividualName.Surname = cardDetails.Surname;
        }

        public static string GenerateOrderChangeAmendAPIsRequest(string airShoppingRequest, string pnr, string reshopRS, string paxType, string pTC, string passport = "", string greencard = "", string visa = "", string knowntraveller = "", string redress = "")
        {
            //Creating the instance of the object
            var instance = XmlRequestBuilder.IATA_OrderChangeAmendAPIsRequest();

            XmlDocument requestXmlDocument = new XmlDocument();
            requestXmlDocument.LoadXml(airShoppingRequest);

            XmlDocument responseXmlDocument = new XmlDocument();
            responseXmlDocument.LoadXml(reshopRS);

            XmlNamespaceManager airShoppingRequestXmlnsManager = responseXmlDocument.CreateXmlNamespaceManager();

            var agentNode = requestXmlDocument.SelectSingleNode("//msg:DistributionChain", airShoppingRequestXmlnsManager).OuterXml;

            instance.DistributionChain = XMLSerialize.Deserialize<Models.AirShopping.DistributionChain, Models.OrderChangeAmendAPIs.DistributionChain>(agentNode);

            var distributionChainLinkNodes = requestXmlDocument.SelectNodes("//ct:DistributionChain//ct:DistributionChainLink", airShoppingRequestXmlnsManager).Count;

            var ordinalValue = distributionChainLinkNodes > 1 ? "3" : "2";

            instance.DistributionChain.DistributionChainLink.AddRange(new[]
            {
                 new Models.OrderChangeAmendAPIs.DistributionChainLink()
                 {
                    Ordinal = ordinalValue,
                    OrgRole = "Carrier",
                    ParticipatingOrg = new Models.OrderChangeAmendAPIs.ParticipatingOrg()
                    {
                         Name = "VIRGIN ATLANTIC",
                         OrgID = "VS"
                    }
                 }
            });

            //Adding Order details
            instance.Request.Order.OrderID = $"VS932{pnr}" + FindAndReturnDataHelper.GetYearSuffix();
            instance.Request.Order.OwnerCode = "VS";
            instance.Request.BookingRef.BookingID = pnr;

            //Adding ServiceList
            instance.Request.ChangeOrderChoice.UpdatePax.Current.PaxRefID = $"{paxType}";
            instance.Request.ChangeOrderChoice.UpdatePax.New.PaxRefID = $"{paxType[..^2]}_TEMP_{paxType[^1]}";

            instance.Request.DataLists.PaxList.Pax.Add(new Models.OrderChangeAmendAPIs.Pax()
            {
                PaxID = paxType,
                PTC = pTC
            });

            instance.Request.DataLists.PaxList.Pax.Add(new Models.OrderChangeAmendAPIs.Pax()
            {
                PaxID = $"{paxType[..^2]}_TEMP_{paxType[^1]}",
                PTC = pTC
            });


            instance.Request.DataLists.PaxList.Pax[1].IdentityDoc = new List<Models.OrderChangeAmendAPIs.IdentityDoc>();

            var givenName = responseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS//msg:Response//ct:DataLists//ct:PaxList//ct:Pax[ct:PaxID='{paxType}']//ct:Individual//ct:GivenName", airShoppingRequestXmlnsManager).InnerText;

            var surname = responseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS//msg:Response//ct:DataLists//ct:PaxList//ct:Pax[ct:PaxID='{paxType}']//ct:Individual//ct:Surname", airShoppingRequestXmlnsManager).InnerText;

            var birthDate = responseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS//msg:Response//ct:DataLists//ct:PaxList//ct:Pax[ct:PaxID='{paxType}']//ct:Individual//ct:Birthdate", airShoppingRequestXmlnsManager).InnerText;

            var genderCode = responseXmlDocument.SelectSingleNode($"msg:IATA_OrderViewRS//msg:Response//ct:DataLists//ct:PaxList//ct:Pax[ct:PaxID='{paxType}']//ct:Individual//ct:GenderCode", airShoppingRequestXmlnsManager).InnerText;

            if (!string.IsNullOrWhiteSpace(passport) && !string.IsNullOrWhiteSpace(visa))
            {
                instance.Request.DataLists.PaxList.Pax[1].IdentityDoc.AddRange(new[]
                {
                        new Models.OrderChangeAmendAPIs.IdentityDoc()
                        {
                            Birthdate = $"{birthDate}",
                            CitizenshipCountryCode="IND",
                            ExpiryDate="2036-08-09",
                            GenderCode = $"{genderCode}",
                            GivenName=$"{givenName}",
                            IdentityDocID="11223344",
                            IdentityDocTypeCode="PT",
                            IssueDate="2024-06-03",
                            IssuingCountryCode="IND",
                            ResidenceCountryCode="IND",
                            Surname=$"{surname}",
                            Visa=new Models.OrderChangeAmendAPIs.Visa{ExpiryDate="2036-06-03",HostCountryCode="USA",IssuingCountryCode="USA",VisaID="123456"},
                        },

                 });
            }

            if (!string.IsNullOrWhiteSpace(passport) && string.IsNullOrWhiteSpace(visa))
            {
                instance.Request.DataLists.PaxList.Pax[1].IdentityDoc.AddRange(new[]
                {
                        new Models.OrderChangeAmendAPIs.IdentityDoc()
                        {
                            Birthdate = $"{birthDate}",
                            CitizenshipCountryCode="IND",
                            ExpiryDate="2035-08-09",
                            GenderCode = $"{genderCode}",
                            GivenName=$"{givenName}",
                            IdentityDocID="11223344",
                            IdentityDocTypeCode="PT",
                            IssueDate="2024-06-03",
                            IssuingCountryCode="IND",
                            ResidenceCountryCode="IND",
                            Surname=$"{surname}",
                        },

                    });

            }

            if (!string.IsNullOrWhiteSpace(greencard))
            {
                instance.Request.DataLists.PaxList.Pax[1].IdentityDoc.AddRange(new[]
                {
                        new Models.OrderChangeAmendAPIs.IdentityDoc()
                        {
                            ExpiryDate="2029-08-09",
                            GivenName=$"{givenName}",
                            IdentityDocID="USA1231234",
                            IdentityDocTypeCode="GC",
                            IssueDate="2024-06-03",
                            IssuingCountryCode="USA",
                            ResidenceCountryCode="USA",
                            Surname=$"{surname}"
                        }
                    });
            }

            if (!string.IsNullOrWhiteSpace(knowntraveller))
            {
                instance.Request.DataLists.PaxList.Pax[1].IdentityDoc.AddRange(new[]
                {
                        new Models.OrderChangeAmendAPIs.IdentityDoc()
                        {
                            ExpiryDate="2036-06-03",
                            GivenName=$"{givenName}",
                            IdentityDocID="123456789",
                            IdentityDocTypeCode="CR",
                            IssueDate="2024-06-03",
                            IssuingCountryCode="USA",
                            Surname=$"{surname}"
                        }
                    });
            }

            if (!string.IsNullOrWhiteSpace(redress))
            {
                instance.Request.DataLists.PaxList.Pax[1].RedressCase = new Models.OrderChangeAmendAPIs.RedressCase { CountryCode = "USA", RedressCaseID = "123456" };
            }

            return GenerateUpdatedRequest(XMLSerialize.Serialize(instance).ToString(), apiRequest: "OrderChangeAmendAPIs");

        }

    }
}
