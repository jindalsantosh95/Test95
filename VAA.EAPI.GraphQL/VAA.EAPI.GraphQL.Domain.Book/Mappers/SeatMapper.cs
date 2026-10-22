using System.Globalization;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Book.Models.Basket;
using VAA.EAPI.GraphQL.Domain.Book.Models.Products.Flight;
using VAA.EAPI.GraphQL.Domain.Book.Models.Reference;
using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Seat;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Offer;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Seat;
using VAA.EAPI.GraphQL.Domain.Book.Models.Seats;

namespace VAA.EAPI.GraphQL.Domain.Book.Mappers
{
    public class SeatMapper
    {
        public static SeatMSRequest SeatRequestMapper(Basket basket, OfferResponse offer, SeatMapRequest request)
        {
            return new SeatMSRequest()
            {
                ChannelId = Constants.Application.SeatMSChannelId,
                Context = new() { CurrencyCode = basket.Summary.PriceCurrency },
                CustomerDetails = basket.People.Select(people => new Models.Customer.CustomerInfo
                {
                    CustId = people.Id,
                    Ptc = people.Type switch
                    {
                        "ADULT" => Constants.Application.DeltaPtcAdult,
                        "CHILD" => Constants.Application.DeltaPtcChild,
                        "TEEN" => Constants.Application.DeltaPtcTeen,
                        "INFANT" => Constants.Application.DeltaPtcInfant,
                        _ => string.Empty
                    },
                    LoyaltyMemberDetails = new() { MemberNumber = people.MembershipDetails?.FQTNumber, ProgramCode = people.MembershipDetails?.FQTProgramme }
                }),
                FlightSegments = GetFlightSegments(basket, offer, request.SectorId)
            };
        }

        public static IEnumerable<Segment> GetFlightSegments(Basket basket, OfferResponse offer, string sectorId)
        {
            var flightSegments = offer.DataList.FlightSegments.Where(segment => segment.Legs.Any(leg => leg.LegId == sectorId));
            var basketFlightSegment = basket.Summary.Slices.Where(segment => segment.Sectors.Any(sector => sector.SectorId == sectorId)).Any() ?
                basket.Summary.Slices.Where(segment => segment.Sectors.Any(sector => sector.SectorId == sectorId)) :
                basket.Summary.Slices.Where(segment => segment.Sectors.Any(sector => sector.SectorId == sectorId));
            var flightSector = basketFlightSegment.Select(sector => sector.Sectors.First(s => s.SectorId == sectorId));

            return flightSegments.Select(segment => new Segment()
            {
                SegmentId = "01",
                MarketingCarrier = segment.MarketingCarrier,
                MarketingFlightNumber = segment.MarketingFlightNumber,
                OperatingCarrier = segment.OperatingCarrier,
                OperatingFlightNumber = segment.OperatingFlightNumber,
                OriginAirportCode = flightSector.First().Departing.Iata,
                OriginAirportName = flightSector.First().Departing.Name,
                DestinationAirportCode = flightSector.First().Arriving.Iata,
                DestinationAirportName = flightSector.First().Arriving.Name,
                FareBasis = offer.Offers.First().OfferItems.First().Services.FirstOrDefault(o => o.FlightSegmentsRefIds.Contains(segment.SegmentId))?.FareDetails?.FareBasisCode,
                ClassOfService = offer.Offers.First().OfferItems.First().Services.FirstOrDefault(o => o.FlightSegmentsRefIds.Contains(segment.SegmentId))?.FareDetails?.FareSegments?.First(fs => fs.FlightSegmentRefId == segment.SegmentId).BookingClass,
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = flightSector.First().ArrivingAt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
                },
                ScheduledDepartureTime = new()
                {
                    LocalDateTime = flightSector.First().DepartingAt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)
                }
            });
        }

        public static SeatMapResponse SeatResponseMapper(Basket basket, OfferResponse offer, SeatMSResponse seatMSResponse, IEnumerable<CabinRefData> cabinRefData, string sectorId, IEnumerable<Models.Response.Seat.SeatMap> aircraftTemplate)
        {
            var offerSegment = offer.DataList.FlightSegments.Where(segment => segment.Legs.Any(leg => leg.LegId == sectorId));
            var fareProduct = GetFareProduct(offer, cabinRefData, offerSegment.First());
            var disableExitFlag = basket.People.Any(x => x.Type == Constants.Application.ChildPtc || x.Type == Constants.Application.InfantPtc);

            return new SeatMapResponse()
            {
                FlightSegment = GetFlightSegments(basket, offer, sectorId).FirstOrDefault(),
                FlightPassengerSeatMap = GetPassengerSeatDetails(basket, seatMSResponse, sectorId),
                CabinClass = GetCabinClass(fareProduct.Id),
                FareFamily = offer.Offers.First().OfferItems.First().Services.First(x => x.FlightSegmentsRefIds.Contains(offerSegment.First().SegmentId)).FareDetails.FareFamilyType,
                FareProduct = fareProduct,
                SeatMap = SeatMapMapperVS(seatMSResponse, aircraftTemplate, fareProduct.Id, disableExitFlag)
            };
        }

        private static IEnumerable<SeatDetails> GetPassengerSeatDetails(Basket basket, SeatMSResponse seatMSResponse, string sectorId)
        {
            int infantCount = 0;
            var infantPassengers = basket.People.Where(customer => customer.Type != Constants.Application.InfantPtc).ToList();

            return basket.FlightPassengerSeatMap.Where(seat => seat.SectorId == sectorId).Select(p => new SeatDetails()
            {
                FirstName = basket.People.First(person => person.Id == p.PassengerId).FirstName,
                LastName = basket.People.First(person => person.Id == p.PassengerId).LastName,
                SeatNumber = p.SeatNumber,
                Price = p.Price,
                Pos = p.SeatNumber.IsNullOrEmpty() == false ? GetSeatPosition(seatMSResponse, p.SeatNumber) : string.Empty,
                LinkedInfant = p.LinkedInfantId != null ? new InfantDetails()
                {
                    InfantPassengerId = p.LinkedInfantId,
                    InfantFirstName = basket.People.First(infant => infant.Id == p.LinkedInfantId).FirstName,
                    InfantLastName = basket.People.First(infant => infant.Id == p.LinkedInfantId).LastName
                } : null

            }) ?? basket.People.Where(p => p.Type != Constants.Application.InfantPtc).Select(passenger =>
            {
                var passengerSeatMap = new SeatDetails()
                {
                    FirstName = passenger.FirstName,
                    LastName = passenger.LastName,
                    SeatNumber = string.Empty,
                    Price = new() { Amount = 0, Currency = string.Empty },
                    Pos = string.Empty,
                    LinkedInfant = passenger.Type != Constants.Application.ChildPtc && infantPassengers.Count > infantCount ? new InfantDetails()
                    {
                        InfantPassengerId = infantPassengers[infantCount].Id,
                        InfantFirstName = infantPassengers[infantCount].FirstName,
                        InfantLastName = infantPassengers[infantCount].LastName
                    } : null
                };
                if (passenger.Type != Constants.Application.ChildPtc)
                    infantCount++;
                return passengerSeatMap;
            }
           );
        }

        private static FareProduct GetFareProduct(OfferResponse offerResponse, IEnumerable<CabinRefData> cabinData, Segment segment)
        {
            var flightSegmentIndex = offerResponse.Offers.First().OfferItems.First().Services.First(x => x.FlightSegmentsRefIds.Contains(segment.SegmentId)).FlightSegmentsRefIds.ToList().FindIndex(x => x == segment.SegmentId);

            var singlefaresegment = offerResponse.Offers.First().OfferItems.First().Services.First(x => x.FlightSegmentsRefIds.Contains(segment.SegmentId)).FareDetails.FareSegments.ToArray()[flightSegmentIndex];

            return new() { Id = singlefaresegment.CabinName, Name = cabinData?.FirstOrDefault(cabin => cabin.BrandID == singlefaresegment.CabinName)?.BrandName };

        }

        public static string GetCabinClass(string brandId)
        {
            return brandId switch
            {
                "VSLT" => "ECONOMY",
                "VSCL" => "ECONOMY",
                "VSDT" => "ECONOMY",
                "VSDL" => "ECONOMY",
                "VSFL" => "ECONOMY",
                "VSFD" => "ECONOMY",
                "VSPE" => "PREMIUM ECONOMY",
                "VSPL" => "PREMIUM ECONOMY",
                "VSFW" => "PREMIUM ECONOMY",
                "VSUP" => "UPPER CLASS",
                "VSUL" => "UPPER CLASS",
                "VSFC" => "UPPER CLASS",
                _ => throw new NotImplementedException()
            };
        }

        public static Models.Response.Seat.SeatMap SeatMapMapperVS(SeatMSResponse seatMSResponse, IEnumerable<Models.Response.Seat.SeatMap> aircraftTemplate, string brandId, bool disableExitFlag)
        {
            var templateSeatMap = aircraftTemplate.Where(seatmap => seatmap.ClassType == GetCabinClass(brandId)).First();
            templateSeatMap.SeatOfferId = seatMSResponse.SeatOfferId;

            if (!seatMSResponse.FlightSegmentSeatMap.Any())
            {
                return templateSeatMap;
            }

            var deltaSeatMap = seatMSResponse.FlightSegmentSeatMap.First().Seatmap;
            var seatMap = deltaSeatMap.Where(seatmap => seatmap.SeatRows.Any(row => !row.RowNumber.IsNullOrEmpty() && row.SeatColumns.Any(col => GetBrandIdForFlex(brandId, col.BrandId))));

            if (seatMap is not null)
            {
                templateSeatMap.FareSummary = GetFareSummary(seatMap, brandId);

                var mapRows = seatMap.First().SeatRows.Where(row => !row.RowNumber.IsNullOrEmpty() && row.SeatColumns.Any());

                foreach (var row in templateSeatMap.Rows.Where(r => !r.RowNumber.IsNullOrDefault()))
                {
                    var deltaRow = mapRows.Where(drow => drow.RowNumber == row.RowNumber.ToString());

                    if (deltaRow.Any())
                    {
                        foreach (var col in row.Columns.Where(r => r != null && !r.SeatNumber.IsNullOrDefault()))
                        {
                            var deltaSeat = deltaRow?.First().SeatColumns.Where(seat => seat.Seat && seat.SeatIdentifer == col.SeatNumber);
                            if (deltaSeat.Any())
                            {
                                col.Pos = deltaSeat?.First().Pos;
                                if (GetBrandIdForFlex(brandId, deltaSeat.First().BrandId) && col.SeatNumber == deltaSeat.First().SeatIdentifer)
                                {
                                    col.Availability = deltaSeat.First().Exit && disableExitFlag ? Constants.Application.SeatMapStatusBlocked : MapSeatStatus(deltaSeat?.First());
                                    col.Offers = deltaSeat?.First()?.Eligibility?.Select(offer => new Offer()
                                    {
                                        OfferId = offer.CatalogueRefId,
                                        OfferItemId = offer.OfferItemId.First().OfferItemId,
                                        CustomerId = offer.CustomerId.First(),
                                        ProductId = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == offer.CatalogueRefId).First().ProductId,
                                        ProductName = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == offer.CatalogueRefId).First().ProductName,
                                        FlightIdRef = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == offer.CatalogueRefId).First().FlightIdRef,
                                        FareProduct = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == offer.CatalogueRefId).First().BrandId,
                                        Price = GetPriceForOffer(seatMSResponse.FlightSegmentSeatMap.First(), offer.CatalogueRefId)
                                    });
                                    if (col.OverrideSeatType)
                                    {
                                        var seatProductID = col.Offers.First().ProductId;
                                        foreach (var summary in templateSeatMap.FareSummary)
                                        {
                                            if (seatProductID == summary.ProductId)
                                            {
                                                summary.SeatType = col.OverrideSeatTypeTo;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    col.Availability = Constants.Application.SeatMapStatusBlocked;
                                    col.Offers = null;
                                }

                                col.Type = deltaSeat.First().Seat ? "SEAT" : null;
                                col.OverWing = row.OverWing;
                            }
                        }
                    }
                }
            }

            return templateSeatMap;
        }

        public static bool GetBrandIdForFlex(string offerBrandId, string seatBrandId)
        {
            return offerBrandId switch
            {
                "VSCL" => seatBrandId == "VSCL" || seatBrandId == "VSFL",
                "VSFL" => seatBrandId == "VSCL" || seatBrandId == "VSFL",
                "VSDT" => seatBrandId == "VSDT" || seatBrandId == "VSFD",
                "VSDL" => seatBrandId == "VSDT" || seatBrandId == "VSFD",
                "VSFD" => seatBrandId == "VSDT" || seatBrandId == "VSFD",
                "VSPE" => seatBrandId == "VSPE" || seatBrandId == "VSFW",
                "VSPL" => seatBrandId == "VSPE" || seatBrandId == "VSFW",
                "VSFW" => seatBrandId == "VSPE" || seatBrandId == "VSFW",
                "VSUP" => seatBrandId == "VSUP" || seatBrandId == "VSFC",
                "VSUL" => seatBrandId == "VSUP" || seatBrandId == "VSFC",
                "VSFC" => seatBrandId == "VSUP" || seatBrandId == "VSFC",
                _ => throw new NotImplementedException()
            };
        }

        public static SeatPrice GetPriceForOffer(FlightSegmentSeatMap seatMap, string catalogueId)
        {
            var price = seatMap.Catalogue.Where(catalogue => catalogue.CatalogueId == catalogueId).First().SeatPrice;
            return new SeatPrice()
            {
                Amount = Convert.ToDecimal(price.MonetaryAmount),
                Currency = price.BaseCurrency
            };
        }


        /* DELTA SEAT MAPPER*/
        public static SeatMapResponse SeatResponseMapper(Basket basket, OfferResponse offer, SeatMSResponse seatMSResponse, IEnumerable<CabinRefData> cabinRefData, string sectorId)
        {
            var flightSegments = basket.Summary.Slices.Where(segment => segment.Sectors.Any(sector => sector.SectorId == sectorId)).Any() ?
                basket.Summary.Slices.Where(segment => segment.Sectors.Any(sector => sector.SectorId == sectorId)) :
                basket.Summary.Slices.Where(segment => segment.Sectors.Any(sector => sector.SectorId == sectorId));

            var flightSector = flightSegments.Select(sector => sector.Sectors.First(s => s.SectorId == sectorId));
            var offerSegment = offer.DataList.FlightSegments.Where(segment => segment.Legs.Any(leg => leg.LegId == sectorId));
            var fareProduct = GetFareProduct(offer, cabinRefData, offerSegment.First());
            var disableExitFlag = basket.People.Any(x => x.Type == "CHILD" || x.Type == "INFANT");

            return new SeatMapResponse()
            {
                FlightSegment = GetFlightSegments(basket, offer, sectorId).FirstOrDefault(),
                FlightPassengerSeatMap = GetPassengerSeatDetails(basket, seatMSResponse, sectorId),
                CabinClass = flightSector.First().CabinClass,
                FareFamily = cabinRefData.FirstOrDefault(x => x.BrandID == fareProduct.Id)?.FareFamily,
                FareProduct = fareProduct,
                SeatMap = SeatMapMapperDL(seatMSResponse, fareProduct.Id, disableExitFlag)
            };
        }

        private static string GetSeatPosition(SeatMSResponse seatMSResponse, string seatNumber)
        {
            var seatMap = seatMSResponse.FlightSegmentSeatMap.First().Seatmap.Where(seatmap => seatmap.SeatRows.Any(row => !row.RowNumber.IsNullOrEmpty() && row.SeatColumns.Any(col => col.SeatIdentifer == seatNumber))).First();
            var seatRow = seatMap.SeatRows.First(row => row.SeatColumns.Any(seat => seat.SeatIdentifer == seatNumber));
            var seatPosition = seatRow.SeatColumns.First(seat => seat.SeatIdentifer == seatNumber).Pos;
            return seatPosition;
        }

        /* DELTA SEAT MAP MAPPER*/
        public static Models.Response.Seat.SeatMap SeatMapMapperDL(SeatMSResponse seatMSResponse, string brandId, bool disableExitFlag)
        {
            var dlSeatMap = new Models.Response.Seat.SeatMap
            {
                SeatOfferId = seatMSResponse.SeatOfferId,
                ClassType = "",
                Configuration = Enumerable.Empty<string>(),
                Rows = Enumerable.Empty<Row>(),
                FareSummary = Enumerable.Empty<FareSummary>()
            };

            var seatMaps = (brandId == Constants.Application.DeltaMainCabin || brandId == Constants.Application.DeltaComfortCabin) ? seatMSResponse.FlightSegmentSeatMap.First().Seatmap.Where(seatmap => seatmap.SeatRows.Any(row => !row.RowNumber.IsNullOrEmpty() && row.SeatColumns.Any(col => col.BrandId == Constants.Application.DeltaComfortCabin || col.BrandId == Constants.Application.DeltaMainCabin))) : seatMSResponse.FlightSegmentSeatMap.First().Seatmap.Where(seatmap => seatmap.SeatRows.Any(row => !row.RowNumber.IsNullOrEmpty() && row.SeatColumns.Any(col => col.BrandId == brandId)));

            if (seatMaps.Any())
            {
                dlSeatMap.Configuration = seatMaps.First().SeatConfiguration.Select(c => c != '|' ? c.ToString() : null);
                dlSeatMap.Equipment = seatMaps.First().Equipment;
                dlSeatMap.FareSummary = GetFareSummary(seatMaps, brandId);

                foreach (var compartment in seatMaps)
                {
                    foreach (var row in compartment.SeatRows)
                    {
                        dlSeatMap.Rows = MapDlSeatRows(seatMSResponse, dlSeatMap, row, disableExitFlag, brandId);
                    }
                }
            }

            return dlSeatMap;
        }

        public static IEnumerable<FareSummary> GetFareSummary(IEnumerable<Models.Seats.SeatMap> seatMaps, string brandId)
        {
            if (seatMaps.Any())
            {
                bool standardExistsForDelight = (brandId == "VSDT" || brandId == "VSDL" || brandId == "VSFD") && seatMaps.Any(seatMap => seatMap.FareSummary.Any(summary => summary?.BrandId == "VSDT" && summary?.SeatType == "STANDARD"));

                if (seatMaps.Count() == 1)
                {
                    if (standardExistsForDelight)
                    {
                        return seatMaps.First().FareSummary.Where(f => (f.BrandId == "VSDT" || f.BrandId == "VSFD") && f.SeatType == "STANDARD").Select(summary => new FareSummary { Min = summary.Min, Max = summary.Max, SeatType = "DELIGHT", Currency = summary.Currency, BrandId = summary.BrandId, ProductId = summary.ProductId });
                    }
                    else
                        return seatMaps.Where(s => s.FareSummary != null).SelectMany(s => s.FareSummary).Where(f => f.BrandId == brandId || f.BrandId == FlexBrandId(brandId) || (brandId == "VSDL" && f.BrandId == "VSDT") || (brandId == "VSPL" && f.BrandId == "VSPE") || (brandId == "VSUL" && f.BrandId == "VSUP"));
                }
                else
                {
                    var filteredSummaries = seatMaps.Where(s => s.FareSummary != null).SelectMany(s => s.FareSummary).Where(f => f.BrandId == brandId || f.BrandId == FlexBrandId(brandId) || (brandId == "VSDL" && f.BrandId == "VSDT") || (brandId == "VSPL" && f.BrandId == "VSPE") || (brandId == "VSUL" && f.BrandId == "VSUP"));

                    if (standardExistsForDelight)
                    {
                        return filteredSummaries.Where(f => (f.BrandId == "VSDT" || f.BrandId == "VSFD") && f.SeatType == "STANDARD").Select(summary =>
                            new FareSummary { Min = summary.Min, Max = summary.Max, SeatType = "DELIGHT", Currency = summary.Currency, BrandId = summary.BrandId, ProductId = summary.ProductId });
                    }
                    else
                        return filteredSummaries;
                }
            }

            return null;
        }

        public static string FlexBrandId(string brandId)
        {
            return brandId switch
            {
                "VSCL" => "VSFL",
                "VSPE" => "VSFW",
                "VSUP" => "VSFC",
                "VSFL" => "VSCL",
                "VSFW" => "VSPE",
                "VSFC" => "VSUP",
                _ => string.Empty
            };
        }

        public static IEnumerable<Row> MapDlSeatRows(SeatMSResponse seatMSResponse, Models.Response.Seat.SeatMap dlSeatMap, SeatRow row, bool disableExitFlag, string brandId)
        {
            var rows = dlSeatMap.Rows.ToList();

            rows.Add(new Row()
            {
                RowNumber = Convert.ToInt32(row.RowNumber),
                ExitLeft = row.ExitOnLeft,
                ExitRight = row.ExitOnRight,
                Columns = row.SeatColumns.Select(c => new Column()
                {
                    Type = MapSeatType(c),
                    SeatNumber = c.SeatIdentifer,
                    Availability = (c.Exit && disableExitFlag) || c.BrandId != brandId ? Constants.Application.SeatMapStatusBlocked : MapSeatStatus(c),
                    OverWing = false,
                    Offers = c.Available && c.BrandId == brandId ? c.Eligibility.Select(e => new Offer()
                    {
                        OfferId = e.CatalogueRefId,
                        OfferItemId = e.OfferItemId.First().OfferItemId,
                        CustomerId = e.CustomerId.First(),
                        ProductId = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().ProductId,
                        ProductName = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().ProductName,
                        FlightIdRef = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().FlightIdRef,
                        FareProduct = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().BrandId,
                        Price = new()
                        {
                            Amount = Convert.ToDecimal(seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().SeatPrice.MonetaryAmount),
                            Currency = seatMSResponse.FlightSegmentSeatMap.First().Catalogue.Where(catalogue => catalogue.CatalogueId == e.CatalogueRefId).First().SeatPrice.BaseCurrency
                        }
                    }) : null,
                    Pos = c.Pos,
                    Attributes = c.Attributes
                })
            });

            return rows;
        }

        public static string MapSeatType(SeatColumn column)
        {
            if (column.Toilet)
                return "LAVATORY";
            if (column.Aisle)
                return "AISLE";
            if (column.Galley)
                return "GALLEY";
            if (column.Seat)
                return "SEAT";
            return string.Empty;
        }

        public static string MapSeatStatus(SeatColumn column)
        {
            if (column.Reserved)
                return Constants.Application.SeatMapStatusOccupied;
            if (column.Available && !column.Blocked)
                return Constants.Application.SeatMapStatusFree;
            if (column.Blocked)
                return Constants.Application.SeatMapStatusBlocked;

            return Constants.Application.SeatMapStatusBlocked;
        }

        public static SeatUpdateRequest MapBasketSeatDetails(Basket basket, string seatResponseOfferId, string sectorId)
        {
            int infantCount = 0;
            var infantPassengers = basket.People.Where(customer => customer.Type == Constants.Application.InfantPtc).ToList();

            return new()
            {
                BasketId = basket.BasketId.ToString(),
                FlightPassengerSeatMap = basket.People.Where(p => p.Type != Constants.Application.InfantPtc).Select(x =>
                {
                    var passengerSeatMap = new FlightPassengerSeatMap()
                    {
                        PassengerId = x.Id,
                        Price = new() { Amount = 0, Currency = "" },
                        SeatNumber = string.Empty,
                        SeatOfferId = seatResponseOfferId,
                        SectorId = sectorId,
                        LinkedInfantId = x.Type != Constants.Application.ChildPtc && infantPassengers.Count > infantCount ? infantPassengers[infantCount].Id : null,
                    };
                    if (x.Type != Constants.Application.ChildPtc)
                        infantCount++;
                    return passengerSeatMap;
                })
            };
        }

        public static SeatUpdateRequest MapBasketSeatSelectDetails(Basket basket, FlightPassengerSeatMap seatMapDetail)
        {
            return new()
            {
                BasketId = basket.BasketId.ToString(),
                FlightPassengerSeatMap = basket.FlightPassengerSeatMap.Where(seat => seat.SectorId == seatMapDetail.SectorId).Select(x =>
                {
                    if (x.PassengerId == seatMapDetail.PassengerId)
                        return new FlightPassengerSeatMap()
                        {
                            PassengerId = x.PassengerId,
                            SeatOfferId = seatMapDetail.SeatOfferId,
                            SelectedSeatOfferId = seatMapDetail.SelectedSeatOfferId,
                            SectorId = seatMapDetail.SectorId,
                            SeatNumber = seatMapDetail.SeatNumber,
                            LinkedInfantId = x.LinkedInfantId,
                            Price = seatMapDetail.Price
                        };
                    else
                        return x;
                })
            };
        }

        public static SeatMSRequest SeatMSRequestMapper(FlightSeatMapRequest request, string channelId)
        {
            var seg = request.FlightSegment;
            return new SeatMSRequest()
            {
                ChannelId = string.IsNullOrEmpty(channelId) ? Constants.Application.SeatMSChannelId : channelId,
                Context = new() { CurrencyCode = request.CurrencyCode },
                CustomerDetails = request.CustomerDetails.Select(people => new Models.Customer.CustomerInfo
                {
                    CustId = people.CustId,
                    Ptc = people.Ptc,
                    LoyaltyMemberDetails = new()
                    {
                        MemberNumber = people.LoyaltyMemberDetails?.MemberNumber,
                        ProgramCode = people.LoyaltyMemberDetails?.ProgramCode,
                        ProgramName = people.LoyaltyMemberDetails?.ProgramName,
                        MemberType = people.LoyaltyMemberDetails?.MemberType,
                        Status = people.LoyaltyMemberDetails?.Status
                    }
                }),
                FlightSegments = new List<Segment> { new Segment
                { SegmentId = seg.SegmentId  ,
                    MarketingCarrier = seg.MarketingCarrier ,
                    MarketingFlightNumber = seg.MarketingFlightNumber ,
                    OperatingCarrier = seg.OperatingCarrier ,
                    OperatingFlightNumber =seg.OperatingFlightNumber,
                    OriginAirportCode = seg.OriginAirportCode,
                    DestinationAirportCode = seg.DestinationAirportCode,
                    ClassOfService = seg.ClassOfService ,
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = seg.ScheduledArrivalTime.LocalDateTime
                },
                ScheduledDepartureTime = new()
                {
                    LocalDateTime = seg.ScheduledDepartureTime.LocalDateTime
                }
                } }
            };
        }


        public static SeatMapResponse FlightSeatResponseMapper(SeatMSResponse seatMSResponse, IEnumerable<Models.Response.Seat.SeatMap> aircraftTemplate, string brandId, bool exitFlag = false)
        {

            var segment = seatMSResponse.FlightSegmentSeatMap.First().FlightSegment;

            var flightSegment = new Segment
            {
                SegmentId = segment.SegmentId,
                OriginAirportCode = segment.OriginAirportCode,
                DestinationAirportCode = segment.DestinationAirportCode,
                MarketingFlightNumber = segment.MarketingFlightNumber,
                OperatingFlightNumber = segment.OperatingFlightNumber,
                OperatingCarrier = segment.OperatingCarrier,
                MarketingCarrier = segment.MarketingCarrier
            };

            var cabinClass = GetCabinClass(brandId);
            var seatMap = SeatMapMapperVS(seatMSResponse, aircraftTemplate, brandId, exitFlag);

            return new SeatMapResponse
            {
                FlightSegment = flightSegment,
                CabinClass = cabinClass,
                SeatMap = seatMap
            };
        }


    }
}
