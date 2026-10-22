using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Basket.Application.Command;
using VAA.EAPI.Microservices.Retail.Basket.Domain;
using VAA.EAPI.Microservices.Retail.Basket.Domain.FCEarnPoints;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Basket.Domain.PointsCurrencyConverter;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Seat;
using VAA.EAPI.Microservices.Retail.Basket.Domain.Insurance;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Retail.Basket.Mapper
{
    public class BasketMapper
    {
        public static Domain.Basket UpdateContactDetails(ContactDetails contactDetails, Domain.Basket originalBasket, bool? FcAutoMatched)
        {
            originalBasket.ContactDetails = contactDetails;
            originalBasket.FcAutoMatched = FcAutoMatched;
            return originalBasket;
        }

        public static Domain.Basket UpdateTnCDetails(TermsAndConditions tnCDetails, Domain.Basket originalBasket)
        {
            originalBasket.TermsAndConditions = tnCDetails;
            return originalBasket;
        }

        public static Domain.Basket UpdateOfferID(IEnumerable<string> offerIds, Domain.Basket originalBasket)
        {
            originalBasket.OfferIds = offerIds;
            return originalBasket;
        }

        public static Domain.Basket UpdatePassengerDetails(Person person, string peopleId, Domain.Basket originalBasket)
        {
            var ppl = originalBasket.People.Where(x => x.Id == peopleId).Select(a =>
            {
                a.DateOfBirth = person.DateOfBirth;
                a.Validation = person.Validation;
                a.FirstName = person.FirstName;
                a.LastName = person.LastName;
                a.Age = person.Age;
                a.Gender = person.Gender;
                a.ProfilePassenger = person.ProfilePassenger;
                a.Title = person.Title;
                a.MembershipDetails = person.MembershipDetails;

                return a;
            }).ToList();

            return originalBasket;
        }

        public static Domain.Basket UpdateLoyaltyDetails(LoyaltyMemberDetails loyaltyDetails, Domain.Basket originalBasket)
        {
            originalBasket.CustomerProfile = loyaltyDetails;
            return originalBasket;
        }

        public static Domain.Basket UpdatePaymentSessionDetails(IEnumerable<PaymentSession> paymentSession, Domain.Basket originalBasket)
        {
            originalBasket.PaymentSessions = paymentSession;
            return originalBasket;
        }

        public static Domain.Basket UpdateBasketOrderStatus(Domain.Basket originalBasket, BasketStatusEnum status)
        {
            originalBasket.Status = new BasketStatus()
            {
                CurrentBasketStatus = status,
                TimeStamp = DateTime.UtcNow
            };
            return originalBasket;
        }

        public static Domain.Basket UpdateBasketFlightSummary(Domain.Basket originalBasket, UpdateFlightSummaryCommand updateFlightSummaryCommand)
        {
            originalBasket.FlightSummary = updateFlightSummaryCommand.FlightSummary;
            originalBasket.SummaryLink = updateFlightSummaryCommand.SummaryLink;
            return originalBasket;
        }

        public static Domain.Basket UpdateSeatDetails(IEnumerable<FlightPassengerSeatMap> seatDetails, Domain.Basket originalBasket)
        {
            if (originalBasket.FlightPassengerSeatMap is null)
            {
                originalBasket.FlightPassengerSeatMap = seatDetails;
                return originalBasket;
            }

            if (originalBasket.FlightPassengerSeatMap.FirstOrDefault(seatMap => seatMap.SectorId == seatDetails.First().SectorId) is not null)
            {
                var updateSeats = originalBasket.FlightPassengerSeatMap.Select(seat =>
                {
                    if (seat.SectorId == seatDetails.First().SectorId)
                    {
                        var seatDetail = seatDetails.First(s => s.PassengerId == seat.PassengerId);
                        return new FlightPassengerSeatMap()
                        {
                            PassengerId = seat.PassengerId,
                            SectorId = seatDetail.SectorId,
                            SeatNumber = seatDetail.SeatNumber,
                            Pos = seatDetail.Pos,
                            SeatOfferId = seatDetail.SeatOfferId,
                            SelectedSeatOfferId = seatDetail.SelectedSeatOfferId,
                            Price = seatDetail.SeatNumber.IsNullOrEmpty() == false ? seatDetail.Price : new SeatPrice { Amount = 0, Currency = "" },
                            LinkedInfantId = seatDetail.LinkedInfantId
                        };
                    }
                    else
                        return seat;
                });

                originalBasket.FlightPassengerSeatMap = updateSeats;
            }
            else
            {
                originalBasket.FlightPassengerSeatMap = originalBasket.FlightPassengerSeatMap.Concat(seatDetails);
            }
            return originalBasket;
        }

        public static Domain.Basket UpdatePointsEquivalentCurrencyDetails(IEnumerable<PtcWisePointsEquivalentCurrencyDetails> ptcWisePointsEquivalentCurrencyDetails, Domain.Basket originalBasket)
        {
            originalBasket.PtcWisePointsEquivalentCurrencyDetails = ptcWisePointsEquivalentCurrencyDetails;
            return originalBasket;
        }

        public static Domain.Basket UpdateCalculatedEarnPoints(EarnPoints earnPoints, Domain.Basket originalBasket)
        {
            originalBasket.EarnPoints = earnPoints;
            return originalBasket;
        }

        public static Domain.Basket UpdateInsuranceQuotes(IEnumerable<InsuranceQuote> insuranceQuotes, Domain.Basket originalBasket)

        {
            originalBasket.InsuranceQuotes = insuranceQuotes;
            return originalBasket;
        }

        public static Domain.Basket UpdateInsuranceDecision(bool isInsured, Domain.Basket originalBasket)

        {
            if (!originalBasket.InsuranceQuotes.IsNullOrEmpty())
                originalBasket.IsInsuranceAdded = isInsured;

            return originalBasket;
        }

        public static Domain.Basket SetCobrandProgressStatus(CoBrandStatusEnum cobrandStatus, Domain.Basket originalBasket)

        {
            originalBasket.CoBrandStatus = cobrandStatus;

            return originalBasket;
        }

    }
}
