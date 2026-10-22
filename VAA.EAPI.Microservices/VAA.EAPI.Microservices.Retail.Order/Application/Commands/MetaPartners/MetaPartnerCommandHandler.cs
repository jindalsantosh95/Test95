using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.MetaPartners
{
    public sealed class MetaPartnerCommandHandler(IHttpConnector httpConnector) : IMetaPartnerCommandHandler
    {
        // sky scanner configuration
        internal static class SkyScannerConfiguration
        {
            public const string PartnerId = "SkyScanner";
            public const string EventType = "conversion";

            public readonly static string Url = ConfigReader.Get("SkyScannerConfiguration:Url");
            public readonly static string TrackingSourceId = ConfigReader.Get("SkyScannerConfiguration:TrackingSourceId");
            public readonly static string AuthorizationSchema = ConfigReader.Get("SkyScannerConfiguration:AuthorizationSchema");
            public readonly static string Secret = ConfigReader.Get("SkyScannerConfiguration:Secret");

            public static readonly Dictionary<string, string> ClassBasedPayments = new()
            {
                { "VSLT", "0.00$" },
                { "VSCL", "1.00$" },
                { "VSFL", "1.00$" },
                { "VSDT", "3.00$" },
                { "VSFD", "3.00$" },
                { "VSPE", "4.00$" },
                { "VSFW", "4.00$" },
                { "VSUP", "5.00$" },
                { "VSFC", "5.00$" }
            };
            public static string GetSkyScannerCommission(string cabinClass)
            {
                return ClassBasedPayments.TryGetValue(cabinClass, out string value) ? value : "0.00$";
            }
        }

        // kayak configuration
        internal static class KayakConfiguration
        {
            public const string PartnerId = "Kayak";

            public readonly static string Url = ConfigReader.Get("KayakConfiguration:Url");
            public readonly static string PartnerCode = ConfigReader.Try("KayakConfiguration:PartnerCode", "VS");
            public readonly static double CommissionPercentage = ConfigReader.Try("KayakConfiguration:CommissionPercentage", 0.02); // Refer user story -687524
        }

        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task Handle(MetaPartnerCommand request, CancellationToken cancellationToken)
        {
            HttpRequestMessage httpRequestMessage;

            switch (request.MetaPartnerId)
            {
                case string metaPartnerId when metaPartnerId.Is(SkyScannerConfiguration.PartnerId):

                    string skyscannerRedirectId = request.Attributes["skyscanner_redirectid"];

                    string flightFare = request.Attributes["FlightFare"];
                    string paymentFee = request.Attributes["PaymentFee"];
                    string paymentMethod = request.Attributes["PaymentMethod"];
                    string fareBasisCode = request.Attributes["FareBasisCode"];
                    string marketingCarrier = request.Attributes["MarketingCarrier"];
                    string cabinClass = request.Attributes["CabinClass"];
                    string normalizedCabinClass = MapSkyScannerCabinClass(cabinClass);
                    string routeType = request.Attributes["RouteType"];
                    string haulType = request.Attributes["HaulType"];

                    string adultCount = request.Attributes["AdultCount"];
                    string youngAdultCount = request.Attributes["TeenCount"];
                    string childCount = request.Attributes["ChildCount"];
                    string infantCount = request.Attributes["InfantCount"];

                    // Calculate the commission based on the cabin class
                    string skyScannerCommission = SkyScannerConfiguration.GetSkyScannerCommission(cabinClass);


                    var payload = new
                    {
                        skyscanner_redirectid = skyscannerRedirectId,
                        tracking_source_id = SkyScannerConfiguration.TrackingSourceId,
                        event_type = SkyScannerConfiguration.EventType,
                        events = new object[]
                        {
                            new
                            {
                                occurred_at = request.BookedOn.Parse<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss+00:00"),
                                conversion_id = request.BookingId,
                                transaction_total = request.Total,
                                flight_fare = flightFare,
                                payment_fee = paymentFee,
                                currency = request.Currency,
                                payment_method = paymentMethod,
                                fare_basis_code = fareBasisCode,
                                marketing_carrier = marketingCarrier,
                                cabin_class = normalizedCabinClass,
                                sky_scanner_commission = skyScannerCommission,
                                route_type = routeType,
                                haul_type = haulType,
                                platform = "web",
                                adult_number = (adultCount.Parse<int>() + youngAdultCount.Parse<int>()).ToString(),
                                child_number = childCount,
                                infant_number = infantCount
                            }
                        }
                    };

                    httpRequestMessage = new(HttpMethod.Post, SkyScannerConfiguration.Url)
                    {
                        Content = new StringContent(Json.Serialize(payload), System.Text.Encoding.UTF8, "application/json")
                    };

                    httpRequestMessage.Headers.Authorization = new(SkyScannerConfiguration.AuthorizationSchema, SkyScannerConfiguration.Secret);

                    break;
                case string metaPartnerId when metaPartnerId.Is(KayakConfiguration.PartnerId):

                    request.Attributes.TryGetValue("kayakclickid", out string kayakClickId);
                    kayakClickId ??= string.Empty;
                    double commission = request.Total.Parse<double>() * KayakConfiguration.CommissionPercentage;
                    string commissionCurrency = request.Attributes["CommissionCurrency"];

                    NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

                    queryString.Add("partnercode", KayakConfiguration.PartnerCode);
                    queryString.Add("bookingid", request.BookingId);
                    queryString.Add("bookedon", request.BookedOn.Parse<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss+00:00"));
                    queryString.Add("price", request.Total);
                    queryString.Add("kayakcommission", commission.ToString("F2"));
                    queryString.Add("currency", request.Currency);
                    queryString.Add("commissioncurrency", commissionCurrency);
                    queryString.Add("kayakclickid", kayakClickId);
                    queryString.Add("bookingtype", "flight");

                    string url = $"{KayakConfiguration.Url}?{queryString}";

                    httpRequestMessage = new(HttpMethod.Get, url);

                    break;
                default:

                    return;
            }

            HttpResponseMessage responseMessage = await _httpConnector.SendAsync(httpRequestMessage, cancellationToken);

            if (!responseMessage.IsSuccessStatusCode)
            {
                string requestContent = httpRequestMessage.Content is not null ? await httpRequestMessage.Content?.ReadAsStringAsync(cancellationToken) : string.Empty;
                string response = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

                throw new Exception($"An error occurred sending event to '{request.MetaPartnerId}'{Environment.NewLine}{httpRequestMessage.RequestUri}{Environment.NewLine}{requestContent}{Environment.NewLine}{response}");
            }
        }

        public static string MapSkyScannerCabinClass(string cabinCode)
        {
            if (cabinCode.IsNullOrWhiteSpace()) return string.Empty;
            return cabinCode switch
            {
                "VSLT" or "VSCL" or "VSDT" or "VSFL" or "VSFD" or "BE" or "MAIN" or "DCP" or "KLLT" => "economy",
                "VSPE" or "VSFW" or "DPPS" => "premium-economy",
                "VSUP" or "VSFC" or "FIRST" or "D1" or "D1S" => "business",
                _ => cabinCode
            };
        }
    }
}
