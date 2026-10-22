using Microsoft.CodeAnalysis;
using System.Data;
using System.Xml;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.NDC.Helpers;
using VAA.EAPI.Automation.NDC.Requests;
using VAA.EAPI.Automation.NDC.Requests.Endpoints;
using VAA.EAPI.Automation.Utility.PNRCreation.Models.OrderSearch;
using VAA.EAPI.Infrastructure.AzureStorage;
using Xunit;
using Xunit.Abstractions;
using ConfigReader = VAA.EAPI.Automation.Core.Helpers.ConfigReader;

namespace VAA.EAPI.Automation.Utility.PNRCreation
{
    public class Program
    {
        private static Random rnd = new();

        static void Main(string[] args)
        {
            ConfigReader.LoadEnvironmentSettingsFile();

            string scenario = string.Empty;
            string ordinal = string.Empty;
            string orgRole = string.Empty;
            string participatingOrgName = string.Empty;
            string participatingOrgOrgID = string.Empty;
            JourneyType journeyType = JourneyType.OneWay;
            LocationType locationOne = LocationType.None;
            LocationType locationTwo = LocationType.None;
            LocationType locationThree = LocationType.None;
            LocationType locationFour = LocationType.None;
            LocationType locationFive = LocationType.None;
            LocationType locationSix = LocationType.None;
            LocationType locationSeven = LocationType.None;
            LocationType locationEight = LocationType.None;
            LocationType locationNine = LocationType.None;
            LocationType locationTen = LocationType.None;
            LocationType locationEleven = LocationType.None;
            LocationType locationTwelve = LocationType.None;


            string cabinTypeCodeOne = string.Empty;
            string cabinTypeCodeTwo = string.Empty;
            string cabinTypeCodeThree = string.Empty;
            string cabinTypeCodeFour = string.Empty;
            string cabinTypeCodeFive = string.Empty;
            string cabinTypeCodeSix = string.Empty;

            int numberOfadults = 0;
            int numberOfchildren = 0;
            int numberOfyouths = 0;
            int numberOfinfants = 0;
            CabinType cabinType = CabinType.EconomyClassic;
            FlightScenarioType flightScenarioType = FlightScenarioType.Virgin;
            string paymentMethod = ConfigReader.Get("paymentMethod", "UnsecureTokenized"); //Update Local setttings paymentMethod to UnsecureUnTokenized if utility is running outside VAA network
            string departureDate = string.Empty;

            var testOutput = new TestOutput(Console.Out);
            var fixture = new TokenFixture();

            fixture.AddTokenForOrderCreateFromConfig("orderCreate");
            fixture.AddTokenForAirShoppingConfig("Airshopping");

            TableStorageService<TestCaseScenariosTableEntity> tableStorageService = new TableStorageService<TestCaseScenariosTableEntity>();

            tableStorageService.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=avateunstordersearchms;AccountKey=evS/2cFwo7PrMeE1MF6uqpLsCay3wq55mZ8BQ9H8DfIOp4f7uGOimqzWra7LdUmsnvdrqtYI5ZO+0sbGnKD/pA==;EndpointSuffix=core.windows.net";
            tableStorageService.TableName = "TestCaseScenarios";

            Console.WriteLine("PNR creation utility");
            Console.WriteLine();

            if (args.ArgumentExists("-create"))
            {
                if (!ValidateCommand("-create", args, out string message))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(message);

                    return;
                }

                ordinal = args.Argument(nameof(ordinal), "0");
                orgRole = args.Argument(nameof(orgRole), "Seller");
                participatingOrgName = args.Argument(nameof(participatingOrgName), "Skyscanner");
                participatingOrgOrgID = args.Argument(nameof(participatingOrgOrgID), "91281035");
                journeyType = args.Argument<JourneyType>(nameof(journeyType));

                switch (journeyType)
                {
                    case JourneyType.OneWay:

                        locationOne = args.Argument<LocationType>(nameof(locationOne));
                        locationTwo = args.Argument<LocationType>(nameof(locationTwo));
                        cabinTypeCodeOne = args.Argument<string>(nameof(cabinTypeCodeOne));

                        break;
                    case JourneyType.Return:

                        locationOne = args.Argument<LocationType>(nameof(locationOne));
                        locationTwo = args.Argument<LocationType>(nameof(locationTwo));
                        locationThree = args.Argument<LocationType>(nameof(locationThree));
                        locationFour = args.Argument<LocationType>(nameof(locationFour));
                        cabinTypeCodeOne = args.Argument<string>(nameof(cabinTypeCodeOne));
                        cabinTypeCodeTwo = args.Argument<string>(nameof(cabinTypeCodeTwo));

                        break;
                    case JourneyType.MultiCity:

                        locationOne = args.Argument<LocationType>(nameof(locationOne));
                        locationTwo = args.Argument<LocationType>(nameof(locationTwo));
                        locationThree = args.Argument<LocationType>(nameof(locationThree));
                        locationFour = args.Argument<LocationType>(nameof(locationFour));
                        locationFive = args.Argument<LocationType>(nameof(locationFive));
                        locationSix = args.Argument<LocationType>(nameof(locationSix));
                        cabinTypeCodeOne = args.Argument<string>(nameof(cabinTypeCodeOne));
                        cabinTypeCodeTwo = args.Argument<string>(nameof(cabinTypeCodeTwo));
                        cabinTypeCodeThree = args.Argument<string>(nameof(cabinTypeCodeThree));

                        break;
                }

                numberOfadults = args.Argument<int>(nameof(numberOfadults));
                numberOfchildren = args.Argument<int>(nameof(numberOfchildren));
                numberOfyouths = args.Argument<int>(nameof(numberOfyouths));
                numberOfinfants = args.Argument<int>(nameof(numberOfinfants));
                cabinType = args.Argument<CabinType>(nameof(cabinType));
                flightScenarioType = args.Argument<FlightScenarioType>(nameof(flightScenarioType));

                NDC_OrderCreateExample(testOutput, Environment.GetEnvironmentVariable("env"), fixture, tableStorageService, ordinal, orgRole, participatingOrgName, participatingOrgOrgID, journeyType, locationOne, locationTwo, locationThree, locationFour, locationFive, locationSix,
                    locationSeven, locationEight, locationNine, locationTen, locationEleven, locationTwelve, cabinTypeCodeOne, cabinTypeCodeTwo, cabinTypeCodeThree, cabinTypeCodeFour, cabinTypeCodeFive, cabinTypeCodeSix, numberOfadults, numberOfchildren, numberOfyouths, numberOfinfants, cabinType, flightScenarioType, scenario);
            }

            if (args.ArgumentExists("-load:"))
            {
                if (!ValidateCommand("-load", args, out string message))
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(message);

                    return;
                }

                var filePath = args.Argument("-load");

                filePath = filePath.Substring(filePath.IndexOf(':') + 1);

                try
                {
                    using (var dataTable = Core.Helpers.DataGenerator.LoadDataSource(filePath))
                        foreach (DataRow row in dataTable.Rows)
                        {
                            try
                            {
                                ordinal = row.Get(nameof(ordinal), "0");
                                orgRole = row.Get(nameof(orgRole), "Seller");
                                participatingOrgName = row.Get(nameof(participatingOrgName), "Skyscanner");
                                participatingOrgOrgID = row.Get(nameof(participatingOrgOrgID), "91281035");
                                scenario = row.Get<string>(nameof(scenario));
                                journeyType = row.Get<JourneyType>(nameof(journeyType));

                                switch (journeyType)
                                {
                                    case JourneyType.OneWay:

                                        locationOne = row.Get<LocationType>(nameof(locationOne));
                                        locationTwo = row.Get<LocationType>(nameof(locationTwo));
                                        cabinTypeCodeOne = row.Get<string>(nameof(cabinTypeCodeOne));

                                        break;
                                    case JourneyType.Return:

                                        locationOne = row.Get<LocationType>(nameof(locationOne));
                                        locationTwo = row.Get<LocationType>(nameof(locationTwo));
                                        locationThree = row.Get<LocationType>(nameof(locationThree));
                                        locationFour = row.Get<LocationType>(nameof(locationFour));
                                        cabinTypeCodeOne = row.Get<string>(nameof(cabinTypeCodeOne));
                                        cabinTypeCodeTwo = row.Get<string>(nameof(cabinTypeCodeTwo));

                                        break;
                                    case JourneyType.MultiCity:

                                        locationOne = row.Get<LocationType>(nameof(locationOne));
                                        locationTwo = row.Get<LocationType>(nameof(locationTwo));
                                        locationThree = row.Get<LocationType>(nameof(locationThree));
                                        locationFour = row.Get<LocationType>(nameof(locationFour));
                                        locationFive = row.Get<LocationType>(nameof(locationFive));
                                        locationSix = row.Get<LocationType>(nameof(locationSix));
                                        cabinTypeCodeOne = row.Get<string>(nameof(cabinTypeCodeOne));
                                        cabinTypeCodeTwo = row.Get<string>(nameof(cabinTypeCodeTwo));
                                        cabinTypeCodeThree = row.Get<string>(nameof(cabinTypeCodeThree));

                                        break;
                                }

                                numberOfadults = row.Get<int>(nameof(numberOfadults));
                                numberOfchildren = row.Get<int>(nameof(numberOfchildren));
                                numberOfyouths = row.Get<int>(nameof(numberOfyouths));
                                numberOfinfants = row.Get<int>(nameof(numberOfinfants));
                                cabinType = row.Get<CabinType>(nameof(cabinType));
                                flightScenarioType = row.Get<FlightScenarioType>(nameof(flightScenarioType));
                                paymentMethod = row.Get<string>(nameof(paymentMethod));
                                departureDate = scenario.Has("Zulu_SSBD") ? DateTime.UtcNow.AddDays(rnd.Next(0, 2)).ToString("yyyy-MM-dd") : "";

                                NDC_OrderCreateExample(testOutput, Environment.GetEnvironmentVariable("env"), fixture, tableStorageService, ordinal, orgRole, participatingOrgName, participatingOrgOrgID, journeyType, locationOne, locationTwo, locationThree, locationFour, locationFive, locationSix,
                                    locationSeven, locationEight, locationNine, locationTen, locationEleven, locationTwelve, cabinTypeCodeOne, cabinTypeCodeTwo, cabinTypeCodeThree, cabinTypeCodeFour, cabinTypeCodeFive, cabinTypeCodeSix, numberOfadults, numberOfchildren, numberOfyouths, numberOfinfants, cabinType, flightScenarioType, row.Get("scenario", string.Empty), row.Get("countryDialingCode", string.Empty), paymentMethod, departureDate);
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.DarkRed;
                                Console.WriteLine(ex.Message);
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                        }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"An error occured whilst processing '{filePath}'.{Environment.NewLine}{ex.Message}");
                }

                return;
            }

            scenario = Helpers.Ask("Enter scenario (Leave empty for auto generated):", false, "");
            ordinal = Helpers.Ask("Enter ordinal[0]:", false, "0");
            orgRole = Helpers.Ask("Enter org role[Seller] :", false, "Seller");
            participatingOrgName = Helpers.Ask("Enter participating org name[Skyscanner] :", false, "Skyscanner");
            participatingOrgOrgID = Helpers.Ask("Enter participating org id[SELLER0001] :", false, "91281035");
            journeyType = Helpers.Ask<JourneyType>("Enter journey type[OneWay, Return, MultiCity] :");
            departureDate = Helpers.Ask("Enter the DepartureDate in yyyy-MM-dd Format (Leave empty for random futureDate):", false, "");
            locationOne = LocationType.None;
            locationTwo = LocationType.None;
            locationThree = LocationType.None;
            locationFour = LocationType.None;
            locationFive = LocationType.None;
            locationSix = LocationType.None;
            cabinTypeCodeOne = string.Empty;
            cabinTypeCodeTwo = string.Empty;
            cabinTypeCodeThree = string.Empty;

            switch (journeyType)
            {
                case JourneyType.OneWay:

                    locationOne = Helpers.Ask<LocationType>("Enter location one :");
                    locationTwo = Helpers.Ask<LocationType>("Enter location two :");
                    cabinTypeCodeOne = Helpers.Ask<string>("Enter Cabin Type for journey 1 [2 = Upper, 4 = Premium, 5 = Economy]: ");

                    break;
                case JourneyType.Return:

                    locationOne = Helpers.Ask<LocationType>("Enter location one :");
                    locationTwo = Helpers.Ask<LocationType>("Enter location two :");
                    locationThree = Helpers.Ask<LocationType>("Enter location three :");
                    locationFour = Helpers.Ask<LocationType>("Enter location four :");
                    cabinTypeCodeOne = Helpers.Ask<string>("Enter Cabin Type for journey 1 [2 = Upper, 4 = Premium, 5 = Economy]: ");
                    cabinTypeCodeTwo = Helpers.Ask<string>("Enter Cabin Type for journey 2 [2 = Upper, 4 = Premium, 5 = Economy]: ");

                    break;
                case JourneyType.MultiCity:

                    locationOne = Helpers.Ask<LocationType>("Enter location one :");
                    locationTwo = Helpers.Ask<LocationType>("Enter location two :");
                    locationThree = Helpers.Ask<LocationType>("Enter location three :");
                    locationFour = Helpers.Ask<LocationType>("Enter location four :");
                    locationFive = Helpers.Ask<LocationType>("Enter location five :");
                    locationSix = Helpers.Ask<LocationType>("Enter location six :");
                    cabinTypeCodeOne = Helpers.Ask<string>("Enter Cabin Type for journey 1 [2 = Upper, 4 = Premium, 5 = Economy]: ");
                    cabinTypeCodeTwo = Helpers.Ask<string>("Enter Cabin Type for journey 2 [2 = Upper, 4 = Premium, 5 = Economy]: ");
                    cabinTypeCodeThree = Helpers.Ask<string>("Enter Cabin Type for journey 3 [2 = Upper, 4 = Premium, 5 = Economy]: ");

                    break;
            }

            numberOfadults = Helpers.Ask<int>("Enter required number of Adults: ");
            numberOfchildren = Helpers.Ask<int>("Enter required number of Children: ");
            numberOfyouths = Helpers.Ask<int>("Enter required number of Youths: ");
            numberOfinfants = Helpers.Ask<int>("Enter required number of Infants: ");

            cabinType = Helpers.Ask<CabinType>("Enter cabin type[EconomyClassic, EconomyClassicFlex, EconomyDelight, EconomyDelightFlex, Premium, PremiumFlex, Upper, UpperFlex] :", true);
            flightScenarioType = Helpers.Ask<FlightScenarioType>("Enter flight scenario type[Virgin, NonVirgin, VirginAndNonVirgin] :", true);
            paymentMethod = Helpers.Ask<string>($"Enter payment method [{paymentMethod}] (UnsecureTokenized, UnsecureUnTokenized): ", false, paymentMethod);

            NDC_OrderCreateExample(testOutput, Environment.GetEnvironmentVariable("env"), fixture, tableStorageService, ordinal, orgRole, participatingOrgName, participatingOrgOrgID, journeyType, locationOne, locationTwo, locationThree, locationFour, locationFive, locationSix,
                locationSeven, locationEight, locationNine, locationTen, locationEleven, locationTwelve, cabinTypeCodeOne, cabinTypeCodeTwo, cabinTypeCodeThree, cabinTypeCodeFour, cabinTypeCodeFive, cabinTypeCodeSix, numberOfadults, numberOfchildren, numberOfyouths, numberOfinfants, cabinType, flightScenarioType, scenario, paymentMethod: paymentMethod, departureDate: departureDate);

            Console.WriteLine();
            Console.WriteLine("Completed running - PRESS ENTER");
            Console.ReadLine();
        }


        public static void NDC_OrderCreateExample(ITestOutputHelper output, string env, TokenFixture tokenFixture, TableStorageService<TestCaseScenariosTableEntity> tableStorageService, string ordinal, string orgRole, string participatingOrgName, string participatingOrgOrgID,
          JourneyType journeyType, LocationType locationOne, LocationType locationTwo, LocationType locationThree, LocationType locationFour, LocationType locationFive, LocationType locationSix, LocationType locationSeven, LocationType locationEight, LocationType locationNine, LocationType locationTen, LocationType locationEleven, LocationType locationTwelve, string cabinTypeCodeOne, string cabinTypeCodeTwo,
          string cabinTypeCodeThree, string cabinTypeCodeFour, string cabinTypeCodeFive, string cabinTypeCodeSix, int numberOfadults, int numberOfchildren, int numberOfyouths, int numberOfinfants, CabinType cabinType, FlightScenarioType flightScenarioType, string scenario = "", string countryDialingCode = "", string paymentMethod = "", string departureDate = "")
        {
            string deptDate = string.IsNullOrEmpty(departureDate) ? DateTime.Now.AddMonths(rnd.Next(5, 10)).ToString("yyyy-MM-dd") : departureDate;

            //Generate Air Shopping request
            var airShoppingRequest = RequestHelper.GenerateAirshoppingRequest(ordinal, orgRole, participatingOrgName, participatingOrgOrgID,
            journeyType, locationOne, locationTwo, locationThree, locationFour, locationFive, locationSix, locationSeven, locationEight, locationNine, locationTen, locationEleven, locationTwelve,
            cabinTypeCodeOne, cabinTypeCodeTwo, cabinTypeCodeThree, cabinTypeCodeFour, cabinTypeCodeFive, cabinTypeCodeSix, numberOfadults, numberOfchildren, numberOfyouths, numberOfinfants, deptDate);

            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            output.WriteLine("Hitting Airshopping with request");


            //Post Air Shopping request
            var airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(env, airShoppingRequest);

            stopwatch.Stop();
            output.WriteLine($"response time for Airshopping is : {stopwatch.ElapsedMilliseconds} ms");

            if (!airShoppingResponse.IsSuccessStatusCode)
            {
                int retry = 5;
                do
                {
                    System.Threading.Thread.Sleep(1000);
                    airShoppingResponse = NDCTransformationEndpoints.AirShoppingRequest(env, airShoppingRequest);
                    output.WriteLine($"Success status code was not returned for Air Shopping retrying");
                    retry--;

                } while (retry > 0 && !airShoppingResponse.IsSuccessStatusCode);
            }

            stopwatch.Stop();
            output.WriteLine($"response time for Airshopping is : {stopwatch.ElapsedMilliseconds} ms");

            //Getting the Air Shopping response and converting to XML Document
            string airShoppingResult = airShoppingResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            XmlDocument airShoppingResponseXmlDocument = new XmlDocument();
            airShoppingResponseXmlDocument.LoadXml(airShoppingResult);
            Assert.True(airShoppingResponse.IsSuccessStatusCode, $"Success status code was not returned for Air Shopping - response was {airShoppingResult} and the scenario name is {scenario}");

            output.WriteLine("Successfully returned Airshopping Response with 200 Status Code returned");

            //Adding namespace manager
            XmlNamespaceManager airShoppingResponseXmlnsManager = new XmlNamespaceManager(airShoppingResponseXmlDocument.NameTable);
            airShoppingResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

            output.WriteLine($"Searching offer for parameters {cabinType} and {flightScenarioType}");

            //Find offer based on Cabin Type and Flight Scenraio
            var offers = FindAndReturnDataHelper.FindOffers(airShoppingResult, cabinType, flightScenarioType);

            if (offers.Count() == 0)
                output.WriteLine($"No offers found for for parameters {cabinType} and {flightScenarioType} and the scenario is {scenario}");

            foreach (var offerDetails in offers.Take(5))
            {
                try
                {
                    Assert.True(offerDetails.Count > 0, "OfferDetails did not return any Offers");

                    var offerRefId = offerDetails["OfferRefID"];
                    var totalAmount = offerDetails["TotalAmount"];
                    var shoppingRefId = offerDetails["ShoppingResponseRefID"];
                    var currencyCode = offerDetails["CurrencyCode"];

                    output.WriteLine($"Successfully found offer with Offer RefID = {offerRefId}");

                    //Generate the Offer Price request based on the Air Shopping initial request and the Air Shopping response data
                    var offerPriceRequest = RequestHelper.GenerateOfferPriceRequest(airShoppingRequest, airShoppingResult, offerRefId);

                    output.WriteLine($"Hitting the Offer Price endpoint");

                    stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    //Post Offer Price request
                    var offerPriceResponse = NDCTransformationEndpoints.OfferPrice(tokenFixture.GetToken("Airshopping"), offerPriceRequest);

                    stopwatch.Stop();
                    output.WriteLine($"response time for Offer Price endpoint is : {stopwatch.ElapsedMilliseconds} ms");

                    //Getting the Offer Price response and converting to XML Document
                    string offerPriceResult = offerPriceResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    XmlDocument offerPriceResponseXmlDocument = new XmlDocument();
                    offerPriceResponseXmlDocument.LoadXml(offerPriceResult);

                    Assert.True(offerPriceResponse.IsSuccessStatusCode, $"Success status code was not returned for Offer Price - response was {offerPriceResult} and the scenario is {scenario}");

                    output.WriteLine($"Offer Price endpoint returned successful response with 200 status code.");

                    //Adding namespace manager
                    XmlNamespaceManager offerPriceResponseXmlnsManager = new XmlNamespaceManager(offerPriceResponseXmlDocument.NameTable);
                    offerPriceResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                    Assert.True(offerPriceResponseXmlDocument.SelectNodes("//def:PricedOffer", offerPriceResponseXmlnsManager).Count > 0, "Priced Offers returned were not > 0");


                    output.WriteLine($"Hitting the Order Create endpoint-----");
                    //Generating the Order Create Request
                    var orderCreateRequest = RequestHelper.GenerateOrderCreateRequest(airShoppingRequest, airShoppingResult, offerRefId, currencyCode, totalAmount, countryDialingCode, paymentMethod);
                    stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    //Post Order Create request
                    var orderCreateResponse = NDCTransformationEndpoints.OrderCreate(env, orderCreateRequest);

                    stopwatch.Stop();
                    output.WriteLine($"response time for Order Create endpoint is : {stopwatch.ElapsedMilliseconds} ms");

                    //Getting the Order Create response and converting to XML Document
                    string orderCreateResult = orderCreateResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    XmlDocument orderCreateResponseXmlDocument = new XmlDocument();
                    orderCreateResponseXmlDocument.LoadXml(orderCreateResult);

                    Assert.True(orderCreateResponse.IsSuccessStatusCode, $"Success status code was not returned for Order Create - response was {orderCreateResult} and scenario is {scenario}");
                    output.WriteLine($"Order Create returned with a successful response with 200 status code.");

                    //Adding namespace manager
                    XmlNamespaceManager orderCreateResponseXmlnsManager = new XmlNamespaceManager(orderCreateResponseXmlDocument.NameTable);
                    orderCreateResponseXmlnsManager.AddNamespace("def", "http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes");

                    var ptc = orderCreateResponseXmlDocument.SelectSingleNode("//def:BookingID", orderCreateResponseXmlnsManager).InnerText;
                    var firstName = orderCreateResponseXmlDocument.SelectSingleNode("//def:GivenName", orderCreateResponseXmlnsManager).InnerText;
                    var lastName = orderCreateResponseXmlDocument.SelectSingleNode("//def:Surname", orderCreateResponseXmlnsManager).InnerText;

                    output.WriteLine($"GivenName - {firstName}");
                    output.WriteLine($"LastName - {lastName}");
                    output.WriteLine($"PNR - {ptc}  | Scenario={scenario}");
                    var locations = string.Empty;

                    if (journeyType == JourneyType.MultiCity)
                    {
                        locations = $"{locationOne}_{locationTwo}_{locationThree}_{locationFour}_{locationFive}_{locationSix}";
                    }
                    else
                    {
                        locations = $"{locationOne}_{locationTwo}";
                    }

                    var rowKey = "";
                    if (!string.IsNullOrEmpty(scenario))
                    {
                        rowKey = scenario;
                    }
                    else
                    {
                        rowKey = $"{locations}_{journeyType}_Adults_{numberOfadults}_Children_{numberOfchildren}_Youths_{numberOfyouths}_Infants_{numberOfinfants}_{cabinType}_{flightScenarioType}_{ptc}";
                    }

                    var result = tableStorageService.InsertOrMergeAsync(new TestCaseScenariosTableEntity()
                    {
                        PartitionKey = "TestCaseScenario",
                        RowKey = rowKey,
                        PNR = ptc,
                        FirstName = firstName,
                        LastName = lastName,
                        DepartureDate = deptDate

                    }).GetAwaiter().GetResult();
                    break;
                }
                catch (Exception ex)
                {
                    output.WriteLine(ex.Message);
                }
            }
        }

        private static bool ValidateCommand(string command, string[] args, out string message)
        {
            message = string.Empty;

            if (!args.Any()) return false;

            if (args.AllArgumentsExist("-create", "-load"))
            {
                message = "Invalid command syntax -create & -load cannot be used together.";

                return false;
            }

            if (command.Equals("-create", StringComparison.OrdinalIgnoreCase) && args.ArgumentExists("-create"))
            {
                if (!args.ArgumentExists("journeyType", out message)) return false;

                var journeyType = args.Argument<JourneyType>("journeyType");

                switch (journeyType)
                {
                    case JourneyType.OneWay:

                        if (!args.AllArgumentsExist(out message, "locationOne", "locationTwo")) return false;
                        if (!args.ArgumentExists("cabinTypeCodeOne", out message)) return false;

                        break;
                    case JourneyType.Return:

                        if (!args.AllArgumentsExist(out message, "locationOne", "locationTwo", "locationThree", "locationFour")) return false;
                        if (!args.AllArgumentsExist(out message, "cabinTypeCodeOne", "cabinTypeCodeTwo")) return false;

                        break;
                    case JourneyType.MultiCity:

                        if (!args.AllArgumentsExist(out message, "locationOne", "locationTwo", "locationThree", "locationFour", "locationFive", "locationSix")) return false;
                        if (!args.AllArgumentsExist(out message, "cabinTypeCodeOne", "cabinTypeCodeTwo", "cabinTypeCodeThree")) return false;

                        break;
                }

                if (!args.AnyArgumentExists(out message, "numberOfadults", "numberOfchildren", "numberOfyouths", "numberOfinfants")) return false;
                if (!args.ArgumentExists("cabinType", out message)) return false;
                if (!args.ArgumentExists("flightScenarioType", out message)) return false;

                return true;
            }

            if (command.Equals("-load", StringComparison.OrdinalIgnoreCase) && args.ArgumentExists("-load"))
            {
                var filePath = args.Argument("-load");

                filePath = filePath.Substring(filePath.IndexOf(':') + 1);

                if (!File.Exists(filePath))
                {
                    message = $"Invalid command argument value (-load), unable to locate file '{filePath}'";

                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
