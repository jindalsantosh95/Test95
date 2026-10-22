using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCArcDocumentUpdate;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public class OrderArcDocumentUpdateMapper
    {
        public static ARCDocument Update(OrderDocument orderDoc, ARCDocument arcDocument)
        {
            bool isFirstTime = arcDocument == null;

            if (arcDocument == null)
            {
                arcDocument = new ARCDocument
                {
                    Id = $"{orderDoc.Id}-ARC",
                    PartitionKey = $"{orderDoc.Id}-ARC",
                    BookingReference = orderDoc.BookingReference,
                    ARCTransactionHistory = new List<ARCTransactionHistory>()
                };
            }

            var customerHistory = (arcDocument.ARCTransactionHistory ?? new List<ARCTransactionHistory>()).ToList();

            foreach (var pax in orderDoc.DataList.CustomerDetails)
            {
                bool isNameCorrectionFlow = orderDoc.ChangeType == ChangeTypeEnum.NameCorrection;

                if (!isFirstTime && isNameCorrectionFlow && !pax.IsNameChanged)
                    continue;


                var orderItem = orderDoc.OrderItems .FirstOrDefault(oi => oi.Services.Any(s => s.ServiceBeneficiaries.Contains(pax.CustId)));
                var docRef = orderItem.AccountableDocumentRefs .First(d => d.CustomerRefId == pax.CustId);
                var passengerHistory = customerHistory.FirstOrDefault(x => x.CustomerId == pax.CustId);

                if (passengerHistory == null)
                {
                    passengerHistory = new ARCTransactionHistory
                    {
                        CustomerId = pax.CustId,
                        PassengerTicketDetails = new List<PassengerTicketDetails>()
                    };
                    customerHistory.Add(passengerHistory);
                }

                var passengerDetails = (passengerHistory.PassengerTicketDetails ?? Enumerable.Empty<PassengerTicketDetails>()).ToList();
                int nextVersion = passengerDetails.Any() ? passengerDetails.Max(x => x.Version) + 1 : 1;
            
                if (isFirstTime)
                {
                    passengerDetails.Add(new PassengerTicketDetails
                    {
                        Version = nextVersion++,
                        Type = "OrderCreate",
                        Amount = new CurrencyAmount
                        {
                            MonetaryAmount = orderItem.OriginalSalePrice .First(p => p.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount
                        },
                        AccountableDocumentRefs = new List<AccountableDocumentRef>
                        {
                            new AccountableDocumentRef
                            {
                                DocumentNumber = docRef.OriginalDocumentNumber,
                                CustomerRefId = pax.CustId
                            }
                        }
                    });
                }

                if (isNameCorrectionFlow)
                {
                    passengerDetails.Add(new PassengerTicketDetails
                    {
                        Version = nextVersion++,
                        Type = "NameCorrection",
                        Amount = new CurrencyAmount { MonetaryAmount = "0.00" },
                        AccountableDocumentRefs = new List<AccountableDocumentRef>
                        {
                            new AccountableDocumentRef
                            {
                                DocumentNumber = docRef.DocumentNumber,
                                CustomerRefId = pax.CustId
                            }
                        }
                    });
                }
                else
                {
                    var addCollectAmount = orderItem?.AddCollectPrice?.Amount?.MonetaryAmount;
                    var transactionType = !string.IsNullOrEmpty(addCollectAmount) && addCollectAmount != "0.00" ? "AddCollect": "EvenExchange";

                    passengerDetails.Add(new PassengerTicketDetails
                    {
                        Version = nextVersion++,
                        Type = transactionType,
                        Amount = new CurrencyAmount
                        {
                            MonetaryAmount = addCollectAmount ?? "0.00"
                        },
                        AccountableDocumentRefs = new List<AccountableDocumentRef>
                        {
                            new AccountableDocumentRef
                            {
                                DocumentNumber = docRef.DocumentNumber,
                                CustomerRefId = pax.CustId
                            }
                        }
                    });
                }

                passengerHistory.PassengerTicketDetails = passengerDetails;
            }

            arcDocument.ARCTransactionHistory = customerHistory;
            return arcDocument;
        }
    }
}