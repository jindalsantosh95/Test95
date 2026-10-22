using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper.CPD
{
    public class AuthorizePaymentMapper
    {
        public static Authorizepayment AuthorizePaymentRequest(Domain.CPD.Payment request, Result<EAPI.Domain.Entities.Common.Response<InitializePaymentResponse>> initializePaymentResponse, IEnumerable<CountryId> countryList, string posCountryCode)
        {
            var posCountryId = countryList.FirstOrDefault(x => x.CountryCode == posCountryCode)?.Id;
            var mobileCountryId = countryList.FirstOrDefault(x => x.DialingCode == request.Payer.PayerContacts.CountryDailingCode)?.Id;

            bool usAgent = posCountryCode == "US";
            bool isMoto = request.TransactionTypeId == "7";

            return new Authorizepayment
            {
                ClientId = ConfigReader.Get(Constants.Environment.CPDClientId),
                Account = PaymentAccountHelper.GetPaymentAccount(usAgent, isMoto),

                Transaction = new()
                {
                    TypeId = request.TransactionTypeId,
                    Id = initializePaymentResponse.Value?.Value?.Transaction?.Id,

                    Card = new()
                    {
                         TypeId = ((int)request.CardMethod.CardType.ToString().Parse<CardTypeId>()).ToString(),

                        Amount = new()
                        {
                            CountryId = posCountryId,
                            CurrencyId = request.Amount.CurrencyId, 
                            Value = initializePaymentResponse.Value.Value.Transaction.Amount.Value
                        },

                        Cvc = request.CardMethod.Cvv,
                        Cardholdername = request.CardMethod.CardholderName,
                        Cardnumber = request.CardMethod.CardNumber,
                        Expiry = request.CardMethod.ExpiryDate,
                        PreAuthenticated = true,

                        Info3dsecure = (request.TransactionTypeId == "7") ? null : new()
                        {
                            Version = Constants.Application.Info3dSecureversion,

                            Cryptogram = new()
                            {
                                Eci = request.PaymentCardAuthentication.Eci,
                                Xid = request.PaymentCardAuthentication.Xid,
                                AlgorithmId = Constants.Application.Algorithmid,
                                Text = request.PaymentCardAuthentication.Cavv + "=",
                            },
                            Additionaldata = new()
                            {
                                Param = [new() { Name = "directoryServerTransactionId", Text = request.PaymentCardAuthentication.ServerTransactionId }]
                            }
                        },
                        Address = new()
                        {
                            CountryId = countryList.FirstOrDefault(a => a.CountryCode == request.Payer.PayerAddress.Country)?.Id,
                            FirstName = request.Payer.FirstName,
                            LastName = request.Payer.LastName,
                            Street = request.Payer.PayerAddress.AddressLine1 + " " + request.Payer.PayerAddress.AddressLine2,
                            PostalCode = request.Payer.PayerAddress.PostalCode,
                            City = request.Payer.PayerAddress.City,
                            ContactDetails = new()
                            {
                                Email = request.Payer.PayerContacts.Email,
                                Mobile = !string.IsNullOrEmpty(request.Payer.PayerContacts.Phone) ? new()
                                {
                                    CountryId = mobileCountryId,
                                    Text = request.Payer.PayerContacts.Phone
                                } : null
                            }
                        }

                        }
                    },

                Clientinfo = new()
                {
                    Language = Constants.Application.Language,
                    SdkVersion = Constants.Application.Sdkversion,
                    Version = Constants.Application.Version,
                    Platform = Constants.Application.Platform,
                }
            };
        }

    }
}


