using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance;
using VAA.EAPI.Common.Serialisation;
using InsuranceProxy = VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase;
using System;
using System.Collections.Generic;
using Purchase = VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance.Purchase;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper
{
    public class InsurancePurchaseMapper
    {
        public static InsurancePurchaseRequest PurchaseRequestMapper(PurchaseRequest request)
        {
            return new InsurancePurchaseRequest()
            {
                ProductCode = Constants.Application.InsuranceProductCode,
                ProductRequestXml = Xml.Serialize(GetPurchases(request))
            };
        }

        private static SendPurchases GetPurchases(PurchaseRequest request)
        {
            return new SendPurchases()
            {
                Partner = new Partner()
                {
                    UserID = ConfigReader.Try(Constants.Environment.DlPurchaseInsuranceUsername),
                    Password = ConfigReader.Try(Constants.Environment.DlPurchaseInsurancePassword)
                },
                PurchasesRequest = new PurchasesRequest()
                {
                    Accam = Constants.Application.AllianzAccam,
                    BusinessUnit = request.BusinessUnit,
                    TransactionID = request.TransactionID,
                    Purchases = request.Purchases.Select(p => new InsuranceProxy.Purchase()
                    {
                        PartnerBookingID = p.PartnerBookingID,
                        GadgetDetails = new()
                        {
                            BrowserType = "Unknown",
                            OperatingSystem = "Unknown",
                            RequestingPlatform = "None"
                        },
                        InsuranceBookingDetails = new()
                        {
                            Currency = p.InsuranceBookingDetails.Currency,
                            FulfillmentMethod = p.InsuranceBookingDetails.FulfillmentMethod,
                            Method = p.InsuranceBookingDetails.Method,
                            PaymentMethod = p.InsuranceBookingDetails.PaymentMethod,
                            PurchaseDateTime = p.InsuranceBookingDetails.PurchaseDateTime,
                            StreetRate = p.InsuranceBookingDetails.StreetRate,
                            Status = p.InsuranceBookingDetails.Status,
                            TotalCoverageAmount = p.InsuranceBookingDetails.TotalCoverageAmount,
                            Purchaser = new()
                            {
                                FirstName = p.InsuranceBookingDetails.Purchaser.FirstName,
                                LastName = p.InsuranceBookingDetails.Purchaser.LastName,
                                EmailAddress = p.InsuranceBookingDetails.Purchaser.EmailAddress,
                                PhoneNumber = p.InsuranceBookingDetails.Purchaser.PhoneNumber
                            },
                            TotalPremiumPrice = p.InsuranceBookingDetails.TotalPremiumPrice,
                            TotalPrice = p.InsuranceBookingDetails.TotalPrice,
                            Type = p.InsuranceBookingDetails.Type,
                        },
                        TravelBookingDetails = new()
                        {
                            Currency = p.TravelBookingDetails.Currency,
                            Method = p.TravelBookingDetails.Method,
                            Path = request.BusinessUnit == "GB" ? p.TravelBookingDetails.Path : "",
                            TotalTripCost = p.TravelBookingDetails.TotalTripCost
                        },
                        TravelDetails = new()
                        {
                            Origin = new()
                            {
                                Code = p.TravelDetails.Origin.Code,
                                Country = p.TravelDetails.Origin.Country
                            },
                            Destination = new()
                            {
                                Code = p.TravelDetails.Destination.Code,
                                Country = p.TravelDetails.Destination.Country
                            },
                            DepartureDate = p.TravelDetails.DepartureDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            ReturnDate = p.TravelDetails.ReturnDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                            Providers = [new Provider() { Name = ConfigReader.Try(Constants.Environment.AllianzProviderName) }]
                        },
                        ProductDetails = GetProductDetails(p.ProductDetails, request.BusinessUnit, p.InsuranceBookingDetails.TotalCoverageAmount, p.InsuranceBookingDetails.TotalPremiumPrice)
                    }).ToArray()
                }
            };
        }

        private static InsuranceProxy.Product[] GetProductDetails(ProductDetails requestProducts, string pos, decimal totalCoverageAmount, decimal totalPremiumPrice)
        {
            var totalInsured = requestProducts.Product.Sum(i => i.InsuredDetails.Insured.Count());
            if (pos == "GB")
            {
                return [ new InsuranceProxy.Product()
                {
                    ProductID = requestProducts.Product.First().ProductID,
                    CoverageAmount = totalCoverageAmount,
                    PremiumPrice = totalPremiumPrice,
                    Price = totalPremiumPrice,
                    CharitableContribution =(decimal)requestProducts.Product.First().CharitableContribution,
                    QuoteID = requestProducts.Product.First().QuoteID,
                    Sequence = 1,
                    EndDate = requestProducts.Product.First().EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    StartDate = requestProducts.Product.First().StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    InsuredDetails = GetInsured(requestProducts, totalCoverageAmount, totalInsured),
                } ];
            }
            else
            {
                return requestProducts.Product.Select(pd => new InsuranceProxy.Product()
                {
                    ProductID = pd.ProductID,
                    CoverageAmount = totalCoverageAmount / totalInsured,
                    PremiumPrice = totalPremiumPrice / totalInsured,
                    Price = totalPremiumPrice / totalInsured,
                    CharitableContribution = pd.CharitableContribution,
                    QuoteID = pd.QuoteID,
                    Sequence = (byte)pd.Sequence,
                    EndDate = pd.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    StartDate = pd.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                    InsuredDetails = pd.InsuredDetails.Insured.Select(id => new InsuranceProxy.Insured()
                    {
                        FirstName = id.FirstName,
                        LastName = id.LastName,
                        InsuredType = id.InsuredType,
                        Birthdate = id.Birthdate.ToString("yyyy-MM-dd"),
                        InsuredSequence = (byte)id.InsuredSequence,
                        PhoneNumber = id.PhoneNumber,
                        TripCost = totalCoverageAmount / totalInsured,
                    }).ToArray()
                }).ToArray();
            }

        }

        private static InsuranceProxy.Insured[] GetInsured(ProductDetails requestProducts, decimal totalTripCost, int totalInsured)
        {
            return requestProducts.Product.Select(pd => pd.InsuredDetails.Insured.First()).Select(id => new InsuranceProxy.Insured()
            {
                FirstName = id.FirstName,
                LastName = id.LastName,
                Birthdate = id.Birthdate.ToString("yyyy-MM-dd"),
                InsuredSequence = (byte)id.InsuredSequence,
                InsuredType = id.InsuredType,
                PhoneNumber = id.PhoneNumber,
                TripCost = totalTripCost / totalInsured,
            }).ToArray();
        }

        public static T MaskInfo<T>(T input, PurchaseRequest purchaseRequest = null)
        {
            if (input == null) return default(T);

            string maskedString = string.Empty;
            if(input is PurchaseRequest request)
            {
                MaskPurchaser(request.Purchases, maskedString);
                MaskInsured(request.Purchases.FirstOrDefault().ProductDetails, maskedString);
            }  
            
            if(input is InsurancePurchaseRequest insurancePurchaseRequest && purchaseRequest is not null)
            {
                MaskPurchaser(purchaseRequest.Purchases, maskedString);
                MaskInsured(purchaseRequest.Purchases.FirstOrDefault().ProductDetails, maskedString);
                var sendPurchases = GetPurchases(purchaseRequest);
                sendPurchases.Partner.UserID = !string.IsNullOrEmpty(sendPurchases.Partner.UserID) ? maskedString.PadLeft(sendPurchases.Partner.UserID.Length, 'X') : null;
                sendPurchases.Partner.Password = !string.IsNullOrEmpty(sendPurchases.Partner.Password) ? maskedString.PadLeft(sendPurchases.Partner.Password.Length, 'X') : null;
                insurancePurchaseRequest.ProductRequestXml = Xml.Serialize(sendPurchases);
            }

            return input;
        }

        private static void MaskPurchaser(IEnumerable<Purchase> purchases, string maskedString)
        {
            foreach (Purchase purchase in purchases)
            {
                purchase.InsuranceBookingDetails.Purchaser.EmailAddress = !string.IsNullOrEmpty(purchase.InsuranceBookingDetails.Purchaser.EmailAddress) ? maskedString.PadLeft(purchase.InsuranceBookingDetails.Purchaser.EmailAddress.Length, 'X') : null;
                purchase.InsuranceBookingDetails.Purchaser.FirstName = !string.IsNullOrEmpty(purchase.InsuranceBookingDetails.Purchaser.FirstName) ? maskedString.PadLeft(purchase.InsuranceBookingDetails.Purchaser.FirstName.Length, 'X') : null;
                purchase.InsuranceBookingDetails.Purchaser.LastName = !string.IsNullOrEmpty(purchase.InsuranceBookingDetails.Purchaser.LastName) ? maskedString.PadLeft(purchase.InsuranceBookingDetails.Purchaser.LastName.Length, 'X') : null;
                purchase.InsuranceBookingDetails.Purchaser.PhoneNumber = !string.IsNullOrEmpty(purchase.InsuranceBookingDetails.Purchaser.PhoneNumber) ? maskedString.PadLeft(purchase.InsuranceBookingDetails.Purchaser.PhoneNumber.Length, 'X') : null;
            }
        }

        private static void MaskInsured(ProductDetails products, string maskedString)
        {
            foreach (var product in products.Product)
            {
                foreach(var insured in product.InsuredDetails.Insured)
                {
                    insured.PhoneNumber = !string.IsNullOrEmpty(insured.PhoneNumber) ? maskedString.PadLeft(insured.PhoneNumber.Length, 'X') : null;
                    insured.Birthdate = DateTime.MinValue;
                    insured.FirstName = !string.IsNullOrEmpty(insured.FirstName) ? maskedString.PadLeft(insured.FirstName.Length, 'X') : null;
                    insured.LastName = !string.IsNullOrEmpty(insured.LastName) ? maskedString.PadLeft(insured.LastName.Length, 'X') : null;

                }
            }
        }

        public static PurchaseResponse PurchaseResponseMapper(string responseXml)
        {
            var response = Xml.Deserialize<SendPurchasesResponse>(responseXml);
            return new PurchaseResponse()
            {
                Status = new() 
                { 
                    Code = response.PurchasesResponse.Status.Code, 
                    Message = response.PurchasesResponse.Status.Message 
                },
                TransactionID = response.PurchasesResponse.TransactionID,
                Purchases = new()
                {
                    Purchase = new Purchase()
                    {
                        PartnerBookingID = response.PurchasesResponse.Purchases.Purchase.PartnerBookingID,
                        Status = response.PurchasesResponse.Purchases.Purchase.Status,
                        OrderID = response.PurchasesResponse.Purchases.Purchase.OrderID,
                        ProductDetails = new()
                        {
                            Product = [ new() {
                            ProductID = response.PurchasesResponse.Purchases.Purchase.ProductDetails.Product.ProductID,
                            Sequence = response.PurchasesResponse.Purchases.Purchase.ProductDetails.Product.Sequence,
                            ProductBookingID = response.PurchasesResponse.Purchases.Purchase.ProductDetails.Product.ProductBookingID
                        }]


                        }
                    }
                }

            };
        }
    }
}
