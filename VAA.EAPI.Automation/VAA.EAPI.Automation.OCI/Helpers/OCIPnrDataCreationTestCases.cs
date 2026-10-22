using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.OCI.Helpers
{
    public class OCIPnrDataCreationTestCases
    {
        private const int noOfDays = 3;
        public const string surnameTesting = "TESTING";
        public const string givenNamesEstaa = "ESTAA";
        public const string givenNamesEstab = "ESTAB";
        public const string givenNamesEstac = "ESTAC";

        public static IEnumerable<object[]> GetRevenuePNRCreationData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, string firstName, string lastName)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, string firstName, string lastName)>
            {
               (LocationType.ATL, LocationType.LHR, 5, 0, 0, 4, "JO", "LEE"),
               (LocationType.LHR, LocationType.ATL, 3, 0, 0, 2, "Bartholomew", "Palmer-Tompkinson"),
               (LocationType.LHR, LocationType.DEL, 8, 0, 0, 1, "ANTHONY", "D'SILVA"),
               (LocationType.LHR, LocationType.SEA, 9, 0, 0, 0, "Venkateshwaran's", "Panchapakesan"),
               (LocationType.BOS, LocationType.LHR, 6, 0, 0, 3, "BOB", "DOE"),
               (LocationType.LHR, LocationType.JAI, 1, 0, 0, 0, "DONOTBOARD", "TESTING"),
               (LocationType.LHR, LocationType.MEX, 2, 0, 0, 1, "ANGELA JOE", "ANGELA JOE"),
               (LocationType.JFK, LocationType.DUB, 1, 0, 0, 1, "O'NEILL", "Jean-Luc Picard"),
               (LocationType.LHR, LocationType.GND, 1, 0, 0, 1, "VIRGIN", "TESTONE"),
               (LocationType.DEL, LocationType.LHR, 2, 0, 0, 0, "INDIA", "TESTING"),
               (LocationType.CUN, LocationType.LHR, 1, 0, 0, 0, "Online-CheckIn", "TESTING"),
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants, string firstName, string lastName) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children,data.infants, data.firstName, data.lastName, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetRedemptionPNRCreationData()
        {
            List<(LocationType origin, LocationType destination, string tierCode)> baseData = new List<(LocationType origin, LocationType destination, string tierCode)>
            {
               (LocationType.LHR, LocationType.MCO, "Red"),
               (LocationType.MCO, LocationType.LHR, "Gold"),
               (LocationType.LHR, LocationType.LAS, "Silver"),
               (LocationType.LHR, LocationType.JFK, "Gold"),
               (LocationType.LHR, LocationType.ATL, "Gold"),
            };

            foreach ((LocationType origin, LocationType destination, string tierCode) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.tierCode, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetRevenuePNRCreationDataWithSeat()
        {
            List<(LocationType origin, LocationType destination, FareFamilyType fareFamilyType, int adults, int teens, int children, int infants, string productName, CabinName cabinName)> baseData = new List<(LocationType origin, LocationType destination, FareFamilyType fareFamilyType, int adults, int teens, int children, int infants, string productName, CabinName cabinName)>
            {
               (LocationType.LHR, LocationType.JFK, FareFamilyType.ECON, 1, 0, 0, 0, "Standard Seat", CabinName.VSCL),
               (LocationType.LHR, LocationType.ATL, FareFamilyType.ECON, 2, 1, 1, 0, "Preferred Seat - Economy Cabin", CabinName.VSCL),
               (LocationType.LHR, LocationType.BOS, FareFamilyType.ECONOMYDELIGHT, 2, 1, 1, 0, "Economy Delight seat", CabinName.VSDT),
               (LocationType.LHR, LocationType.ATL, FareFamilyType.ECONCLASSICFLEXIBLE, 1, 1, 0, 0, "Preferred Seat - Economy Cabin", CabinName.VSCL),
               (LocationType.LHR, LocationType.ATL, FareFamilyType.ECONDELIGHTFLEXIBLE, 1, 1, 0, 0, "Economy Delight seat", CabinName.VSDT),
               (LocationType.LHR, LocationType.ATL, FareFamilyType.COMFORTPLUSPREMIUMECON, 1, 1, 0, 0, "Premium Economy Cabin Seat", CabinName.VSPE),
               (LocationType.ATL, LocationType.LHR, FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE, 1, 1, 0, 0, "Premium Economy Cabin Seat", CabinName.VSPE),
               (LocationType.LHR, LocationType.TLV, FareFamilyType.BUSFIRST, 1, 1, 0, 0, "Upper Class seat", CabinName.VSUP),
               (LocationType.LHR, LocationType.MCO, FareFamilyType.BUSFIRSTFLEXIBLE, 1, 1, 0, 0, "Upper Class seat", CabinName.VSUP)
            };

            foreach ((LocationType origin, LocationType destination, FareFamilyType fareFamilyType, int adults, int teens, int children, int infants, string productName, CabinName cabinName) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.fareFamilyType, data.adults, data.teens, data.children, data.infants, data.productName, data.cabinName, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetPNRCreationData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)>
            {
               (LocationType.DEL, LocationType.LHR, 1, 1, 0, 0),
               (LocationType.LHR, LocationType.ATL, 1, 2, 0, 0),
               (LocationType.LHR, LocationType.JFK, 1, 1, 1, 1),
               (LocationType.LHR, LocationType.SEA, 2, 0, 0, 0),
               (LocationType.LHR, LocationType.BOS, 3, 0, 0, 0)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetPNRCreationJVPartnersData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType)>
            {
               (LocationType.LHR, LocationType.JFK, 1, 1, 1, 1, FareFamilyType.ECON, FlightType.AF, FareFamilyType.ECONOMYDELIGHT),
               (LocationType.LHR, LocationType.ATL, 1, 1, 0, 0, FareFamilyType.ECONOMYDELIGHT, FlightType.DL, FareFamilyType.COMFORTPLUSPREMIUMECON),
               (LocationType.LHR, LocationType.LAX, 1, 0, 0, 0, FareFamilyType.BUSFIRST, FlightType.KL, FareFamilyType.ECON)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, data.outboundFareFamilyType, data.outboundFlightType, data.inboundFareFamilyType, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetPNRCreationCodeshareData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType, FlightType inboundFlightType)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType, FlightType inboundFlightType)>
            {
               (LocationType.LHR, LocationType.ICN, 1, 1, 1, 1, FareFamilyType.ECON, FlightType.KE, FareFamilyType.ECON, FlightType.KE),
               (LocationType.LHR, LocationType.YYC, 1, 0, 0, 0, FareFamilyType.ECON, FlightType.WS, FareFamilyType.ECON, FlightType.VS),
               (LocationType.LHR, LocationType.IDR, 1, 0, 0, 0, FareFamilyType.ECON, FlightType.VS, FareFamilyType.ECON, FlightType.VS),
               (LocationType.LHR, LocationType.JAI, 1, 0, 0, 0, FareFamilyType.ECON, FlightType.VS, FareFamilyType.ECON, FlightType.VS)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType, FlightType inboundFlightType) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, data.outboundFareFamilyType, data.outboundFlightType, data.inboundFareFamilyType, data.inboundFlightType, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetPNRCreationInterlineData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType)>
            {
              (LocationType.LHR, LocationType.POS, 1, 1, 1, 1, FareFamilyType.ECON, FlightType.BW, FareFamilyType.ECONOMYDELIGHT),
              (LocationType.LHR, LocationType.COK, 1, 1, 0, 0, FareFamilyType.BUSFIRST, FlightType.AI, FareFamilyType.COMFORTPLUSPREMIUMECON)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants, FareFamilyType outboundFareFamilyType, FlightType outboundFlightType, FareFamilyType inboundFareFamilyType) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, data.outboundFareFamilyType, data.outboundFlightType, data.inboundFareFamilyType, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetPNRCreationMultiCity3SlicesData()
        {
            List<LocationType[]> baseData = new List<LocationType[]>
            {
                new[] { LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LAX, LocationType.LAX, LocationType.LHR },
                new[] { LocationType.IAD, LocationType.LHR, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.LHR },
                new[] { LocationType.TLV, LocationType.LHR, LocationType.LHR, LocationType.JFK, LocationType.ATL, LocationType.LHR },
                new[] { LocationType.DEL, LocationType.LHR, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR }
            };

            foreach (LocationType[] data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetPNRCreationMultiCity4SlicesData()
        {
            List<LocationType[]> baseData = new List<LocationType[]>
            {
                new[] { LocationType.DEL, LocationType.LHR, LocationType.LHR, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.LHR, LocationType.DEL },
                new[] { LocationType.SEA, LocationType.AMS, LocationType.AMS, LocationType.JFK, LocationType.JFK, LocationType.LHR, LocationType.LHR, LocationType.BLR },
                new[] { LocationType.IAD, LocationType.LHR, LocationType.LHR, LocationType.DEL, LocationType.DEL, LocationType.ATL, LocationType.ATL, LocationType.LHR },
                new[] { LocationType.LHR, LocationType.ATL, LocationType.ATL, LocationType.YYZ, LocationType.YYZ, LocationType.JFK, LocationType.JFK, LocationType.LHR },
            };

            foreach (LocationType[] data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetOCIReturnDateScenarioData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)>
            {
               (LocationType.LHR, LocationType.BGI, 1, 1, 1, 1),
               (LocationType.LHR, LocationType.MBJ, 1, 0, 0, 0),
               (LocationType.LHR, LocationType.JNB, 1, 1, 0, 0),
               (LocationType.LHR, LocationType.DXB, 1, 0, 0, 0),
               (LocationType.LHR, LocationType.GND, 1, 1, 1, 1),
               (LocationType.LHR, LocationType.MLE, 1, 1, 1, 1),
               (LocationType.LHR, LocationType.CUN, 1, 0, 0, 0),
               (LocationType.LHR, LocationType.HKT, 1, 0, 0, 0),
               (LocationType.LHR, LocationType.HRE, 1, 0, 0, 0)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> GetOCIDirectWithStopScenarioData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)>
            {
               (LocationType.LHR, LocationType.ANU, 1, 1, 1, 1),
               (LocationType.LHR, LocationType.SVD, 1, 0, 0, 0),
               (LocationType.LHR, LocationType.GND, 1, 0, 0, 0)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> FlightsWithTwoStopScenarioData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)>
            {
               (LocationType.JFK, LocationType.JAI, 1, 1, 1, 1),
               (LocationType.DEL, LocationType.ATL, 9, 0, 0, 0),
               (LocationType.MAN, LocationType.LOS, 2, 0, 0, 0)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> FlightsWithOneStopScenarioData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)>
            {
               (LocationType.LHR, LocationType.JFK, 9, 0, 0, 0),
               (LocationType.LHR, LocationType.DEL, 2, 0, 0, 0),
               (LocationType.LHR, LocationType.LOS, 1, 1, 1, 1),
               (LocationType.JFK, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.ATL, LocationType.LHR, 1, 0, 0, 0)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> DirectFlightsScenarioData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants)>
            {
               (LocationType.LHR, LocationType.JFK, 5, 0, 0, 0),
               (LocationType.LHR, LocationType.DEL, 2, 0, 0, 0),
               (LocationType.JFK, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.ATL, LocationType.LHR, 2, 0, 0, 0),
               (LocationType.LHR, LocationType.BOM, 1, 1, 1, 1),
               (LocationType.LHR, LocationType.LAS, 3, 1, 1, 1),
               (LocationType.LHR, LocationType.MLE, 4, 0, 0, 0),
               (LocationType.JFK, LocationType.AMS, 1, 0, 0, 0),
               (LocationType.JFK, LocationType.CDG, 1, 0, 0, 0),
               (LocationType.LHR, LocationType.RUH, 1, 0, 0, 0),
               (LocationType.CUN, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.MLE, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.CPT, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.DXB, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.ICN, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.BOM, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.CUN, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.ANU, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.RUH, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.BGI, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.JNB, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.LOS, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.MBJ, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.YYZ, LocationType.LHR, 1, 0, 0, 0),
               (LocationType.LHR, LocationType.YYZ, 1, 0, 0, 0),
               (LocationType.LHR, LocationType.LOS, 1, 0, 0, 0)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> LongLayoverFlightsScenarioData()
        {
            List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, int connectionTimeDuration)> baseData = new List<(LocationType origin, LocationType destination, int adults, int teens, int children, int infants, int connectionTimeDuration)>
            {
               (LocationType.LHR, LocationType.YYZ, 2, 0, 0, 0, 8),
               (LocationType.LHR, LocationType.GND, 1, 0, 0, 0, 20),
               (LocationType.JFK, LocationType.DEL, 1, 0, 0, 0, 10)
            };

            foreach ((LocationType origin, LocationType destination, int adults, int teens, int children, int infants, int connectionTimeDuration) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.adults, data.teens, data.children, data.infants, data.connectionTimeDuration, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> TSAPNRCreationData()
        {
            List<(string firstName, string lastName)> baseData = new List<(string firstName, string lastName)>
            {
               ("AKnown", "Traveler"),
               ("BKnown", "Traveler"),
               ("CKnown", "Traveler"),
               ("AFlex", "Traveler"),
               ("BFlex", "Traveler"),
               ("Yellow", "Bandit"),
               ("Alan", "Aqua")
            };

            foreach ((string firstName, string lastName) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.firstName, data.lastName, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> SeamlessFlightsScenarioData()
        {
            List<(LocationType origin, LocationType destination, FlightType flightType)> baseData = new List<(LocationType origin, LocationType destination, FlightType flightType)>
            {
               (LocationType.LHR, LocationType.JFK, FlightType.DL),
               (LocationType.LHR, LocationType.ATL, FlightType.AF),
               (LocationType.JFK, LocationType.LHR, FlightType.AF),
               (LocationType.LHR, LocationType.ATL, FlightType.KL),
               (LocationType.JFK, LocationType.LHR, FlightType.KL),
               (LocationType.LHR, LocationType.ICN, FlightType.KE),
               (LocationType.LHR, LocationType.RUH, FlightType.SV),
               (LocationType.RUH, LocationType.JFK, FlightType.SV),
               (LocationType.ICN, LocationType.LHR, FlightType.KE)
            };

            foreach ((LocationType origin, LocationType destination, FlightType flightType) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.flightType, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> NonSeamlessFlightsScenarioData()
        {
            List<(LocationType origin, LocationType destination)> baseData = new List<(LocationType origin, LocationType destination)>
            {
               (LocationType.IDR, LocationType.LHR),
               (LocationType.COK, LocationType.LHR),
               (LocationType.JAI, LocationType.LHR)
            };

            foreach ((LocationType origin, LocationType destination) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> USABookingWithSpecialNamesESTAData()
        {
            List<(LocationType origin, LocationType destination, string firstName, string lastName)> baseData = new List<(LocationType origin, LocationType destination, string firstName, string lastName)>
            {
               (LocationType.LHR, LocationType.JFK, givenNamesEstaa, surnameTesting),
               (LocationType.LHR, LocationType.ATL, givenNamesEstaa, surnameTesting),
               (LocationType.LHR, LocationType.JFK, givenNamesEstab, surnameTesting),
               (LocationType.LHR, LocationType.ATL, givenNamesEstab, surnameTesting),
               (LocationType.LHR, LocationType.JFK, givenNamesEstac, surnameTesting),
               (LocationType.LHR, LocationType.ATL, givenNamesEstac, surnameTesting)
            };

            foreach ((LocationType origin, LocationType destination, string firstName, string lastName) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, data.firstName, data.lastName, dayOffset
                    };
                }
            }
        }

        public static IEnumerable<object[]> SeasonalFlightsRoutes()
        {
            List<(LocationType origin, LocationType destination)> baseData = new List<(LocationType origin, LocationType destination)>
            {
               (LocationType.LHR, LocationType.HKT),
               (LocationType.LHR, LocationType.CUN),
               (LocationType.LHR, LocationType.MLE),
               (LocationType.MAN, LocationType.LAS),
               (LocationType.MAN, LocationType.BGI),
               (LocationType.MAN, LocationType.MBJ)
            };

            foreach ((LocationType origin, LocationType destination) data in baseData)
            {
                for (int dayOffset = 0; dayOffset < noOfDays; dayOffset++)
                {
                    yield return new object[]
                    {
                       data.origin, data.destination, dayOffset
                    };
                }
            }
        }
    }
}
