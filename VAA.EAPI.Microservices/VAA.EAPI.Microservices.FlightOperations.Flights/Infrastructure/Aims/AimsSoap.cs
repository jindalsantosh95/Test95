namespace VAA.EAPI.Microservices.FlightOperations.Flights.Infrastructure.Aims
{
    namespace AimsSoap
    {
        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "AIMSCountriesList", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class AIMSCountriesList : object
        {

            private int cCountField;

            private TAIMSCountry[] cCountryListField;

            private string cErrorExplanationField;

            [System.Runtime.Serialization.DataMember()]
            public int cCount
            {
                get
                {
                    return cCountField;
                }
                set
                {
                    cCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSCountry[] cCountryList
            {
                get
                {
                    return cCountryListField;
                }
                set
                {
                    cCountryListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cErrorExplanation
            {
                get
                {
                    return cErrorExplanationField;
                }
                set
                {
                    cErrorExplanationField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCountry", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCountry : object
        {

            private string cCountryCodeField;

            private string cCountryNameField;

            [System.Runtime.Serialization.DataMember()]
            public string cCountryCode
            {
                get
                {
                    return cCountryCodeField;
                }
                set
                {
                    cCountryCodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cCountryName
            {
                get
                {
                    return cCountryNameField;
                }
                set
                {
                    cCountryNameField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "AimsAirportList", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class AimsAirportList : object
        {

            private TAIMSAirport[] cAIMSAirportListField;

            private int cCountField;

            private string cErrorExplanationField;

            [System.Runtime.Serialization.DataMember()]
            public TAIMSAirport[] cAIMSAirportList
            {
                get
                {
                    return cAIMSAirportListField;
                }
                set
                {
                    cAIMSAirportListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int cCount
            {
                get
                {
                    return cCountField;
                }
                set
                {
                    cCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cErrorExplanation
            {
                get
                {
                    return cErrorExplanationField;
                }
                set
                {
                    cErrorExplanationField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSAirport", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSAirport : object
        {

            private string cAirportCodeField;

            private string cAirportNameField;

            private int cAltitudeField;

            private bool cCKNotEFBField;

            private string cCountryCodeField;

            private string cHeadInfoField;

            private double cHeilideckHeightField;

            private bool cIsPossibleDiversionBitField;

            private bool cIsPossibleFuelLocationField;

            private string cLatitudeField;

            private int cLocationTypeField;

            private string cLongtitudeField;

            private string cMagneticVariationField;

            private int cRunwayLengthField;

            private short cTAXI_ARRField;

            private short cTAXI_DEPField;

            [System.Runtime.Serialization.DataMember()]
            public string cAirportCode
            {
                get
                {
                    return cAirportCodeField;
                }
                set
                {
                    cAirportCodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cAirportName
            {
                get
                {
                    return cAirportNameField;
                }
                set
                {
                    cAirportNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int cAltitude
            {
                get
                {
                    return cAltitudeField;
                }
                set
                {
                    cAltitudeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool cCKNotEFB
            {
                get
                {
                    return cCKNotEFBField;
                }
                set
                {
                    cCKNotEFBField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cCountryCode
            {
                get
                {
                    return cCountryCodeField;
                }
                set
                {
                    cCountryCodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cHeadInfo
            {
                get
                {
                    return cHeadInfoField;
                }
                set
                {
                    cHeadInfoField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public double cHeilideckHeight
            {
                get
                {
                    return cHeilideckHeightField;
                }
                set
                {
                    cHeilideckHeightField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool cIsPossibleDiversionBit
            {
                get
                {
                    return cIsPossibleDiversionBitField;
                }
                set
                {
                    cIsPossibleDiversionBitField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool cIsPossibleFuelLocation
            {
                get
                {
                    return cIsPossibleFuelLocationField;
                }
                set
                {
                    cIsPossibleFuelLocationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cLatitude
            {
                get
                {
                    return cLatitudeField;
                }
                set
                {
                    cLatitudeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int cLocationType
            {
                get
                {
                    return cLocationTypeField;
                }
                set
                {
                    cLocationTypeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cLongtitude
            {
                get
                {
                    return cLongtitudeField;
                }
                set
                {
                    cLongtitudeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cMagneticVariation
            {
                get
                {
                    return cMagneticVariationField;
                }
                set
                {
                    cMagneticVariationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int cRunwayLength
            {
                get
                {
                    return cRunwayLengthField;
                }
                set
                {
                    cRunwayLengthField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short cTAXI_ARR
            {
                get
                {
                    return cTAXI_ARRField;
                }
                set
                {
                    cTAXI_ARRField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short cTAXI_DEP
            {
                get
                {
                    return cTAXI_DEPField;
                }
                set
                {
                    cTAXI_DEPField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "AIMSActypeList", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class AIMSActypeList : object
        {

            private TAIMSACType[] cAIMSACTypeListField;

            private int cCountField;

            private string cErrorExplanationField;

            [System.Runtime.Serialization.DataMember()]
            public TAIMSACType[] cAIMSACTypeList
            {
                get
                {
                    return cAIMSACTypeListField;
                }
                set
                {
                    cAIMSACTypeListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int cCount
            {
                get
                {
                    return cCountField;
                }
                set
                {
                    cCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cErrorExplanation
            {
                get
                {
                    return cErrorExplanationField;
                }
                set
                {
                    cErrorExplanationField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSACType", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSACType : object
        {

            private string cACTypeCodeField;

            private string cCHCDescriptionField;

            private string cDescriptionField;

            [System.Runtime.Serialization.DataMember()]
            public string cACTypeCode
            {
                get
                {
                    return cACTypeCodeField;
                }
                set
                {
                    cACTypeCodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cCHCDescription
            {
                get
                {
                    return cCHCDescriptionField;
                }
                set
                {
                    cCHCDescriptionField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cDescription
            {
                get
                {
                    return cDescriptionField;
                }
                set
                {
                    cDescriptionField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "AIMSAircraftList", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class AIMSAircraftList : object
        {

            private TAIMSAircraft[] cAIMSAircraftListField;

            private int cCountField;

            private string cErrorExplanationField;

            [System.Runtime.Serialization.DataMember()]
            public TAIMSAircraft[] cAIMSAircraftList
            {
                get
                {
                    return cAIMSAircraftListField;
                }
                set
                {
                    cAIMSAircraftListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int cCount
            {
                get
                {
                    return cCountField;
                }
                set
                {
                    cCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cErrorExplanation
            {
                get
                {
                    return cErrorExplanationField;
                }
                set
                {
                    cErrorExplanationField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSAircraft", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSAircraft : object
        {

            private string cACCountryField;

            private string cACRegField;

            private string cACSerialNumberField;

            private string cACTypeField;

            private string cACUsePoundField;

            [System.Runtime.Serialization.DataMember()]
            public string cACCountry
            {
                get
                {
                    return cACCountryField;
                }
                set
                {
                    cACCountryField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cACReg
            {
                get
                {
                    return cACRegField;
                }
                set
                {
                    cACRegField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cACSerialNumber
            {
                get
                {
                    return cACSerialNumberField;
                }
                set
                {
                    cACSerialNumberField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cACType
            {
                get
                {
                    return cACTypeField;
                }
                set
                {
                    cACTypeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string cACUsePound
            {
                get
                {
                    return cACUsePoundField;
                }
                set
                {
                    cACUsePoundField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "AIMSChartList", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class AIMSChartList : object
        {

            private string ErrorexplanationField;

            private TAIMSchartRec[] TAIMSchartReclistField;

            private int rCountField;

            [System.Runtime.Serialization.DataMember()]
            public string Errorexplanation
            {
                get
                {
                    return ErrorexplanationField;
                }
                set
                {
                    ErrorexplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSchartRec[] TAIMSchartReclist
            {
                get
                {
                    return TAIMSchartReclistField;
                }
                set
                {
                    TAIMSchartReclistField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int rCount
            {
                get
                {
                    return rCountField;
                }
                set
                {
                    rCountField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSchartRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSchartRec : object
        {

            private string CHARTERERCODEField;

            private string CHARTERERDESCRIPTIONField;

            [System.Runtime.Serialization.DataMember()]
            public string CHARTERERCODE
            {
                get
                {
                    return CHARTERERCODEField;
                }
                set
                {
                    CHARTERERCODEField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CHARTERERDESCRIPTION
            {
                get
                {
                    return CHARTERERDESCRIPTIONField;
                }
                set
                {
                    CHARTERERDESCRIPTIONField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSGetCrewMaxFDP", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSGetCrewMaxFDP : object
        {

            private int CountField;

            private string ErrorExplanationField;

            private string FlightCarrierField;

            private string FlightDDField;

            private string FlightDepField;

            private string FlightDescField;

            private string FlightLegCDField;

            private string FlightMMField;

            private string FlightNoField;

            private string FlightYYField;

            private CrewMaxFDP[] TAIMSGetCrewMaxFDPMemberField;

            [System.Runtime.Serialization.DataMember()]
            public int Count
            {
                get
                {
                    return CountField;
                }
                set
                {
                    CountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightCarrier
            {
                get
                {
                    return FlightCarrierField;
                }
                set
                {
                    FlightCarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDD
            {
                get
                {
                    return FlightDDField;
                }
                set
                {
                    FlightDDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDep
            {
                get
                {
                    return FlightDepField;
                }
                set
                {
                    FlightDepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDesc
            {
                get
                {
                    return FlightDescField;
                }
                set
                {
                    FlightDescField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightLegCD
            {
                get
                {
                    return FlightLegCDField;
                }
                set
                {
                    FlightLegCDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightMM
            {
                get
                {
                    return FlightMMField;
                }
                set
                {
                    FlightMMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightNo
            {
                get
                {
                    return FlightNoField;
                }
                set
                {
                    FlightNoField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightYY
            {
                get
                {
                    return FlightYYField;
                }
                set
                {
                    FlightYYField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Name = "TAIMSGetCrewMaxFDP")]
            public CrewMaxFDP[] TAIMSGetCrewMaxFDPMember
            {
                get
                {
                    return TAIMSGetCrewMaxFDPMemberField;
                }
                set
                {
                    TAIMSGetCrewMaxFDPMemberField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "CrewMaxFDP", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class CrewMaxFDP : object
        {

            private string CRouteDateField;

            private string CrewNameField;

            private string CrewRouteField;

            private string FDPEndField;

            private string FDPStartField;

            private string MaxFDPField;

            private string RankField;

            private string StaffNumField;

            [System.Runtime.Serialization.DataMember()]
            public string CRouteDate
            {
                get
                {
                    return CRouteDateField;
                }
                set
                {
                    CRouteDateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrewName
            {
                get
                {
                    return CrewNameField;
                }
                set
                {
                    CrewNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrewRoute
            {
                get
                {
                    return CrewRouteField;
                }
                set
                {
                    CrewRouteField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FDPEnd
            {
                get
                {
                    return FDPEndField;
                }
                set
                {
                    FDPEndField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FDPStart
            {
                get
                {
                    return FDPStartField;
                }
                set
                {
                    FDPStartField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MaxFDP
            {
                get
                {
                    return MaxFDPField;
                }
                set
                {
                    MaxFDPField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Rank
            {
                get
                {
                    return RankField;
                }
                set
                {
                    RankField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string StaffNum
            {
                get
                {
                    return StaffNumField;
                }
                set
                {
                    StaffNumField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "RouteAndSectorList", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class RouteAndSectorList : object
        {

            private TAIMSARoute[] AIMSARouteListField;

            private string CountField;

            private string ErrorExplanationField;

            [System.Runtime.Serialization.DataMember()]
            public TAIMSARoute[] AIMSARouteList
            {
                get
                {
                    return AIMSARouteListField;
                }
                set
                {
                    AIMSARouteListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Count
            {
                get
                {
                    return CountField;
                }
                set
                {
                    CountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSARoute", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSARoute : object
        {

            private string ADateField;

            private TAIMSSector[] AIMSSectorlistField;

            private string ARouteField;

            private string ARouteADateField;

            private string AcSerialNumberField;

            [System.Runtime.Serialization.DataMember()]
            public string ADate
            {
                get
                {
                    return ADateField;
                }
                set
                {
                    ADateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSSector[] AIMSSectorlist
            {
                get
                {
                    return AIMSSectorlistField;
                }
                set
                {
                    AIMSSectorlistField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ARoute
            {
                get
                {
                    return ARouteField;
                }
                set
                {
                    ARouteField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ARouteADate
            {
                get
                {
                    return ARouteADateField;
                }
                set
                {
                    ARouteADateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string AcSerialNumber
            {
                get
                {
                    return AcSerialNumberField;
                }
                set
                {
                    AcSerialNumberField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSSector", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSSector : object
        {

            private CrewOnSector[] CrewOnSectorListField;

            private string DepField;

            private string OperdateField;

            private string arouteadateField;

            private string arrField;

            private string arrdateField;

            private string carriercodeField;

            private string charterercodeField;

            private string fltnoField;

            private string legcdField;

            private string staField;

            private string stdField;

            [System.Runtime.Serialization.DataMember()]
            public CrewOnSector[] CrewOnSectorList
            {
                get
                {
                    return CrewOnSectorListField;
                }
                set
                {
                    CrewOnSectorListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Dep
            {
                get
                {
                    return DepField;
                }
                set
                {
                    DepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Operdate
            {
                get
                {
                    return OperdateField;
                }
                set
                {
                    OperdateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string arouteadate
            {
                get
                {
                    return arouteadateField;
                }
                set
                {
                    arouteadateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string arr
            {
                get
                {
                    return arrField;
                }
                set
                {
                    arrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string arrdate
            {
                get
                {
                    return arrdateField;
                }
                set
                {
                    arrdateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string carriercode
            {
                get
                {
                    return carriercodeField;
                }
                set
                {
                    carriercodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string charterercode
            {
                get
                {
                    return charterercodeField;
                }
                set
                {
                    charterercodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string fltno
            {
                get
                {
                    return fltnoField;
                }
                set
                {
                    fltnoField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string legcd
            {
                get
                {
                    return legcdField;
                }
                set
                {
                    legcdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string sta
            {
                get
                {
                    return staField;
                }
                set
                {
                    staField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string std
            {
                get
                {
                    return stdField;
                }
                set
                {
                    stdField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "CrewOnSector", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class CrewOnSector : object
        {

            private string IdField;

            private string PosField;

            [System.Runtime.Serialization.DataMember()]
            public string Id
            {
                get
                {
                    return IdField;
                }
                set
                {
                    IdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Pos
            {
                get
                {
                    return PosField;
                }
                set
                {
                    PosField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "GetCrewMember", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class GetCrewMember : object
        {

            private int CountField;

            private string CrouteDDField;

            private string CrouteMMField;

            private string CrouteYYField;

            private string ErrorExplanationField;

            private TAIMSMember[] FMemberField;

            [System.Runtime.Serialization.DataMember()]
            public int Count
            {
                get
                {
                    return CountField;
                }
                set
                {
                    CountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrouteDD
            {
                get
                {
                    return CrouteDDField;
                }
                set
                {
                    CrouteDDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrouteMM
            {
                get
                {
                    return CrouteMMField;
                }
                set
                {
                    CrouteMMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrouteYY
            {
                get
                {
                    return CrouteYYField;
                }
                set
                {
                    CrouteYYField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSMember[] FMember
            {
                get
                {
                    return FMemberField;
                }
                set
                {
                    FMemberField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSMember", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSMember : object
        {

            private string acField;

            private string altposField;

            private string baseField;

            private short crsdayField;

            private string crsdayddField;

            private string crsdaymmField;

            private string crsdayyyField;

            private string crteField;

            private string firstNameField;

            private string idField;

            private string lastNameField;

            private string nameField;

            private string notifField;

            private string paxField;

            private string posField;

            private string qualcatField;

            private string requestField;

            private string shortNameField;

            private string trndutyField;

            [System.Runtime.Serialization.DataMember()]
            public string ac
            {
                get
                {
                    return acField;
                }
                set
                {
                    acField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(EmitDefaultValue = false)]
            public string altpos
            {
                get
                {
                    return altposField;
                }
                set
                {
                    altposField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string @base
            {
                get
                {
                    return baseField;
                }
                set
                {
                    baseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short crsday
            {
                get
                {
                    return crsdayField;
                }
                set
                {
                    crsdayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string crsdaydd
            {
                get
                {
                    return crsdayddField;
                }
                set
                {
                    crsdayddField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string crsdaymm
            {
                get
                {
                    return crsdaymmField;
                }
                set
                {
                    crsdaymmField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string crsdayyy
            {
                get
                {
                    return crsdayyyField;
                }
                set
                {
                    crsdayyyField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string crte
            {
                get
                {
                    return crteField;
                }
                set
                {
                    crteField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string firstName
            {
                get
                {
                    return firstNameField;
                }
                set
                {
                    firstNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string id
            {
                get
                {
                    return idField;
                }
                set
                {
                    idField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string lastName
            {
                get
                {
                    return lastNameField;
                }
                set
                {
                    lastNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string name
            {
                get
                {
                    return nameField;
                }
                set
                {
                    nameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string notif
            {
                get
                {
                    return notifField;
                }
                set
                {
                    notifField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string pax
            {
                get
                {
                    return paxField;
                }
                set
                {
                    paxField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string pos
            {
                get
                {
                    return posField;
                }
                set
                {
                    posField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string qualcat
            {
                get
                {
                    return qualcatField;
                }
                set
                {
                    qualcatField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string request
            {
                get
                {
                    return requestField;
                }
                set
                {
                    requestField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string shortName
            {
                get
                {
                    return shortNameField;
                }
                set
                {
                    shortNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string trnduty
            {
                get
                {
                    return trndutyField;
                }
                set
                {
                    trndutyField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "GetLegMembers", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class GetLegMembers : object
        {

            private string ErrorExplanationField;

            private TAIMSGetLegMembers[] LegMembsField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSGetLegMembers[] LegMembs
            {
                get
                {
                    return LegMembsField;
                }
                set
                {
                    LegMembsField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSGetLegMembers", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSGetLegMembers : object
        {

            private int FCountField;

            private TAIMSMember[] FMemberField;

            private string FlightCarrierField;

            private string FlightDDField;

            private string FlightDepField;

            private string FlightDescField;

            private string FlightLegCDField;

            private string FlightMMField;

            private int FlightNoField;

            private string FlightYYField;

            [System.Runtime.Serialization.DataMember()]
            public int FCount
            {
                get
                {
                    return FCountField;
                }
                set
                {
                    FCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSMember[] FMember
            {
                get
                {
                    return FMemberField;
                }
                set
                {
                    FMemberField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightCarrier
            {
                get
                {
                    return FlightCarrierField;
                }
                set
                {
                    FlightCarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDD
            {
                get
                {
                    return FlightDDField;
                }
                set
                {
                    FlightDDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDep
            {
                get
                {
                    return FlightDepField;
                }
                set
                {
                    FlightDepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDesc
            {
                get
                {
                    return FlightDescField;
                }
                set
                {
                    FlightDescField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightLegCD
            {
                get
                {
                    return FlightLegCDField;
                }
                set
                {
                    FlightLegCDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightMM
            {
                get
                {
                    return FlightMMField;
                }
                set
                {
                    FlightMMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int FlightNo
            {
                get
                {
                    return FlightNoField;
                }
                set
                {
                    FlightNoField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightYY
            {
                get
                {
                    return FlightYYField;
                }
                set
                {
                    FlightYYField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "GetLegMembersPerDay", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class GetLegMembersPerDay : object
        {

            private int CountField;

            private string DayField;

            private TAIMSGetLegMembers[] DayMemberField;

            private string ErrorExplanationField;

            private string MonthField;

            private string YearField;

            [System.Runtime.Serialization.DataMember()]
            public int Count
            {
                get
                {
                    return CountField;
                }
                set
                {
                    CountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Day
            {
                get
                {
                    return DayField;
                }
                set
                {
                    DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSGetLegMembers[] DayMember
            {
                get
                {
                    return DayMemberField;
                }
                set
                {
                    DayMemberField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Month
            {
                get
                {
                    return MonthField;
                }
                set
                {
                    MonthField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Year
            {
                get
                {
                    return YearField;
                }
                set
                {
                    YearField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "GetLegMembersPerPeriod", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class GetLegMembersPerPeriod : object
        {

            private int CountField;

            private string DDField;

            private short DayField;

            private TAIMSGetLegMembers[] DayMemberField;

            private string ErrorExplanationField;

            private string FromHrsField;

            private string FromMinField;

            private string MMField;

            private string ToHrsField;

            private string ToMinField;

            private string YYField;

            [System.Runtime.Serialization.DataMember()]
            public int Count
            {
                get
                {
                    return CountField;
                }
                set
                {
                    CountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string DD
            {
                get
                {
                    return DDField;
                }
                set
                {
                    DDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short Day
            {
                get
                {
                    return DayField;
                }
                set
                {
                    DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSGetLegMembers[] DayMember
            {
                get
                {
                    return DayMemberField;
                }
                set
                {
                    DayMemberField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FromHrs
            {
                get
                {
                    return FromHrsField;
                }
                set
                {
                    FromHrsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FromMin
            {
                get
                {
                    return FromMinField;
                }
                set
                {
                    FromMinField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MM
            {
                get
                {
                    return MMField;
                }
                set
                {
                    MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ToHrs
            {
                get
                {
                    return ToHrsField;
                }
                set
                {
                    ToHrsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ToMin
            {
                get
                {
                    return ToMinField;
                }
                set
                {
                    ToMinField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string YY
            {
                get
                {
                    return YYField;
                }
                set
                {
                    YYField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "FlightParams", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class FlightParams : object
        {

            private string DepArrModeField;

            private string FlightACTypeField;

            private string FlightArrField;

            private string FlightCarrierField;

            private string FlightDepField;

            private string FlightLegCdField;

            private string FlightNumField;

            private string FromDDField;

            private string FromHHField;

            private string FromMinField;

            private string FromMonthField;

            private string FromYYYYField;

            private string ToDDField;

            private string ToHHField;

            private string ToMinField;

            private string ToMonthField;

            private string ToYYYYField;

            [System.Runtime.Serialization.DataMember()]
            public string DepArrMode
            {
                get
                {
                    return DepArrModeField;
                }
                set
                {
                    DepArrModeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightACType
            {
                get
                {
                    return FlightACTypeField;
                }
                set
                {
                    FlightACTypeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightArr
            {
                get
                {
                    return FlightArrField;
                }
                set
                {
                    FlightArrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightCarrier
            {
                get
                {
                    return FlightCarrierField;
                }
                set
                {
                    FlightCarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDep
            {
                get
                {
                    return FlightDepField;
                }
                set
                {
                    FlightDepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightLegCd
            {
                get
                {
                    return FlightLegCdField;
                }
                set
                {
                    FlightLegCdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightNum
            {
                get
                {
                    return FlightNumField;
                }
                set
                {
                    FlightNumField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FromDD
            {
                get
                {
                    return FromDDField;
                }
                set
                {
                    FromDDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FromHH
            {
                get
                {
                    return FromHHField;
                }
                set
                {
                    FromHHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FromMin
            {
                get
                {
                    return FromMinField;
                }
                set
                {
                    FromMinField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FromMonth
            {
                get
                {
                    return FromMonthField;
                }
                set
                {
                    FromMonthField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FromYYYY
            {
                get
                {
                    return FromYYYYField;
                }
                set
                {
                    FromYYYYField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ToDD
            {
                get
                {
                    return ToDDField;
                }
                set
                {
                    ToDDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ToHH
            {
                get
                {
                    return ToHHField;
                }
                set
                {
                    ToHHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ToMin
            {
                get
                {
                    return ToMinField;
                }
                set
                {
                    ToMinField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ToMonth
            {
                get
                {
                    return ToMonthField;
                }
                set
                {
                    ToMonthField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ToYYYY
            {
                get
                {
                    return ToYYYYField;
                }
                set
                {
                    ToYYYYField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSFLightsList", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSFLightsList : object
        {

            private string ErrorExplanationField;

            private int FlightCountField;

            private TAIMSFlight[] FlightListField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int FlightCount
            {
                get
                {
                    return FlightCountField;
                }
                set
                {
                    FlightCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSFlight[] FlightList
            {
                get
                {
                    return FlightListField;
                }
                set
                {
                    FlightListField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSFlight", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        [System.Runtime.Serialization.KnownType(typeof(TAIMSFlightX1))]
        public partial class TAIMSFlight : object
        {

            private string DelayRemarksField;

            private string FlagsField;

            private string FlightAcTypeField;

            private string FlightActualDayArrField;

            private string FlightActualDayDepField;

            private string FlightArrField;

            private FlightDelay[] FlightArrDelaysField;

            private AssosiatedCrewRouteInfo[] FlightAssocCrwRtesField;

            private string FlightAtaField;

            private string FlightAtdField;

            private string FlightBlkTimeField;

            private string FlightCarrierField;

            private string FlightDDField;

            private string FlightDateField;

            private string FlightDepField;

            private FlightDelay[] FlightDepDelaysField;

            private string FlightDescField;

            private string FlightEtaField;

            private string FlightEtdField;

            private string FlightLegCDField;

            private string FlightMMField;

            private int FlightNoField;

            private int FlightNoOfPaxField;

            private string FlightRegField;

            private string FlightStaField;

            private string FlightStatusField;

            private string FlightStdField;

            private string FlightTDOWNField;

            private string FlightTKOFFField;

            private string FlightTypeField;

            private string FlightYYField;

            [System.Runtime.Serialization.DataMember(EmitDefaultValue = false)]
            public string DelayRemarks
            {
                get
                {
                    return DelayRemarksField;
                }
                set
                {
                    DelayRemarksField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(EmitDefaultValue = false)]
            public string Flags
            {
                get
                {
                    return FlagsField;
                }
                set
                {
                    FlagsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightAcType
            {
                get
                {
                    return FlightAcTypeField;
                }
                set
                {
                    FlightAcTypeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightActualDayArr
            {
                get
                {
                    return FlightActualDayArrField;
                }
                set
                {
                    FlightActualDayArrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightActualDayDep
            {
                get
                {
                    return FlightActualDayDepField;
                }
                set
                {
                    FlightActualDayDepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightArr
            {
                get
                {
                    return FlightArrField;
                }
                set
                {
                    FlightArrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public FlightDelay[] FlightArrDelays
            {
                get
                {
                    return FlightArrDelaysField;
                }
                set
                {
                    FlightArrDelaysField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public AssosiatedCrewRouteInfo[] FlightAssocCrwRtes
            {
                get
                {
                    return FlightAssocCrwRtesField;
                }
                set
                {
                    FlightAssocCrwRtesField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightAta
            {
                get
                {
                    return FlightAtaField;
                }
                set
                {
                    FlightAtaField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightAtd
            {
                get
                {
                    return FlightAtdField;
                }
                set
                {
                    FlightAtdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightBlkTime
            {
                get
                {
                    return FlightBlkTimeField;
                }
                set
                {
                    FlightBlkTimeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightCarrier
            {
                get
                {
                    return FlightCarrierField;
                }
                set
                {
                    FlightCarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDD
            {
                get
                {
                    return FlightDDField;
                }
                set
                {
                    FlightDDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDate
            {
                get
                {
                    return FlightDateField;
                }
                set
                {
                    FlightDateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDep
            {
                get
                {
                    return FlightDepField;
                }
                set
                {
                    FlightDepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public FlightDelay[] FlightDepDelays
            {
                get
                {
                    return FlightDepDelaysField;
                }
                set
                {
                    FlightDepDelaysField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDesc
            {
                get
                {
                    return FlightDescField;
                }
                set
                {
                    FlightDescField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightEta
            {
                get
                {
                    return FlightEtaField;
                }
                set
                {
                    FlightEtaField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightEtd
            {
                get
                {
                    return FlightEtdField;
                }
                set
                {
                    FlightEtdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightLegCD
            {
                get
                {
                    return FlightLegCDField;
                }
                set
                {
                    FlightLegCDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightMM
            {
                get
                {
                    return FlightMMField;
                }
                set
                {
                    FlightMMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int FlightNo
            {
                get
                {
                    return FlightNoField;
                }
                set
                {
                    FlightNoField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int FlightNoOfPax
            {
                get
                {
                    return FlightNoOfPaxField;
                }
                set
                {
                    FlightNoOfPaxField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightReg
            {
                get
                {
                    return FlightRegField;
                }
                set
                {
                    FlightRegField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightSta
            {
                get
                {
                    return FlightStaField;
                }
                set
                {
                    FlightStaField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightStatus
            {
                get
                {
                    return FlightStatusField;
                }
                set
                {
                    FlightStatusField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightStd
            {
                get
                {
                    return FlightStdField;
                }
                set
                {
                    FlightStdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightTDOWN
            {
                get
                {
                    return FlightTDOWNField;
                }
                set
                {
                    FlightTDOWNField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightTKOFF
            {
                get
                {
                    return FlightTKOFFField;
                }
                set
                {
                    FlightTKOFFField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightType
            {
                get
                {
                    return FlightTypeField;
                }
                set
                {
                    FlightTypeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightYY
            {
                get
                {
                    return FlightYYField;
                }
                set
                {
                    FlightYYField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSFlightX1", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSFlightX1 : TAIMSFlight
        {

            private string CancelledField;

            private string ETADayField;

            private string ETDdayField;

            private short NextCarrierField;

            private string NextDEPField;

            private string NextDayField;

            private short NextFltField;

            private string NextLegCdField;

            private short NextStdField;

            private short TaxiArrField;

            private short TaxiDepField;

            [System.Runtime.Serialization.DataMember()]
            public string Cancelled
            {
                get
                {
                    return CancelledField;
                }
                set
                {
                    CancelledField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ETADay
            {
                get
                {
                    return ETADayField;
                }
                set
                {
                    ETADayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ETDday
            {
                get
                {
                    return ETDdayField;
                }
                set
                {
                    ETDdayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short NextCarrier
            {
                get
                {
                    return NextCarrierField;
                }
                set
                {
                    NextCarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string NextDEP
            {
                get
                {
                    return NextDEPField;
                }
                set
                {
                    NextDEPField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string NextDay
            {
                get
                {
                    return NextDayField;
                }
                set
                {
                    NextDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short NextFlt
            {
                get
                {
                    return NextFltField;
                }
                set
                {
                    NextFltField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string NextLegCd
            {
                get
                {
                    return NextLegCdField;
                }
                set
                {
                    NextLegCdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short NextStd
            {
                get
                {
                    return NextStdField;
                }
                set
                {
                    NextStdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short TaxiArr
            {
                get
                {
                    return TaxiArrField;
                }
                set
                {
                    TaxiArrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short TaxiDep
            {
                get
                {
                    return TaxiDepField;
                }
                set
                {
                    TaxiDepField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "FlightDelay", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class FlightDelay : object
        {

            private string FDelayCodeField;

            private string FDelayTimeField;

            [System.Runtime.Serialization.DataMember()]
            public string FDelayCode
            {
                get
                {
                    return FDelayCodeField;
                }
                set
                {
                    FDelayCodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FDelayTime
            {
                get
                {
                    return FDelayTimeField;
                }
                set
                {
                    FDelayTimeField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "AssosiatedCrewRouteInfo", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class AssosiatedCrewRouteInfo : object
        {

            private string RouteDateField;

            private string RouteNameField;

            [System.Runtime.Serialization.DataMember()]
            public string RouteDate
            {
                get
                {
                    return RouteDateField;
                }
                set
                {
                    RouteDateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string RouteName
            {
                get
                {
                    return RouteNameField;
                }
                set
                {
                    RouteNameField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSFLightsListX1", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSFLightsListX1 : object
        {

            private string ErrorExplanationField;

            private int FlightCountField;

            private TAIMSFlightX1[] FlightListX1Field;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int FlightCount
            {
                get
                {
                    return FlightCountField;
                }
                set
                {
                    FlightCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSFlightX1[] FlightListX1
            {
                get
                {
                    return FlightListX1Field;
                }
                set
                {
                    FlightListX1Field = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "CrewMemberRosterDetailsForPeriodd", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class CrewMemberRosterDetailsForPeriodd : object
        {

            private int CrewRostCountField;

            private string ErrorExplanationField;

            private TAIMSCrewRostItm[] TAIMSCrewRostDetailListField;

            [System.Runtime.Serialization.DataMember()]
            public int CrewRostCount
            {
                get
                {
                    return CrewRostCountField;
                }
                set
                {
                    CrewRostCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSCrewRostItm[] TAIMSCrewRostDetailList
            {
                get
                {
                    return TAIMSCrewRostDetailListField;
                }
                set
                {
                    TAIMSCrewRostDetailListField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrewRostItm", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrewRostItm : object
        {

            private string ATAField;

            private string ATABaseField;

            private string ATABaseDayField;

            private string ATADayField;

            private string ATALocalField;

            private string ATALocaldayField;

            private string ATDField;

            private string ATDBaseField;

            private string ATDLocalField;

            private string ATDLocalDayField;

            private string ArrField;

            private string CDATEField;

            private string CROUTEField;

            private string CarrierField;

            private string CrewBaseField;

            private string DayField;

            private string DepField;

            private string FltField;

            private string GDBEGField;

            private string GDBEGBaseField;

            private string GDBEGBaseDayField;

            private string GDBEGLocalField;

            private string GDBEGLocalDayField;

            private string GDBEG_DayField;

            private string GDENDField;

            private string GDENDBaseField;

            private string GDENDBaseDayField;

            private string GDENDDayField;

            private string GDENDLocalField;

            private string GDENDLocalDayField;

            private string LegcdField;

            private string PAXField;

            private string STAField;

            private string STABaseField;

            private string STABaseDayField;

            private string STADayField;

            private string STALocalField;

            private string STALocalDayField;

            private string STDField;

            private string STDBaseField;

            private string STDLocalField;

            [System.Runtime.Serialization.DataMember()]
            public string ATA
            {
                get
                {
                    return ATAField;
                }
                set
                {
                    ATAField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATABase
            {
                get
                {
                    return ATABaseField;
                }
                set
                {
                    ATABaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATABaseDay
            {
                get
                {
                    return ATABaseDayField;
                }
                set
                {
                    ATABaseDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATADay
            {
                get
                {
                    return ATADayField;
                }
                set
                {
                    ATADayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATALocal
            {
                get
                {
                    return ATALocalField;
                }
                set
                {
                    ATALocalField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATALocalday
            {
                get
                {
                    return ATALocaldayField;
                }
                set
                {
                    ATALocaldayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATD
            {
                get
                {
                    return ATDField;
                }
                set
                {
                    ATDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATDBase
            {
                get
                {
                    return ATDBaseField;
                }
                set
                {
                    ATDBaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATDLocal
            {
                get
                {
                    return ATDLocalField;
                }
                set
                {
                    ATDLocalField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATDLocalDay
            {
                get
                {
                    return ATDLocalDayField;
                }
                set
                {
                    ATDLocalDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Arr
            {
                get
                {
                    return ArrField;
                }
                set
                {
                    ArrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CDATE
            {
                get
                {
                    return CDATEField;
                }
                set
                {
                    CDATEField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CROUTE
            {
                get
                {
                    return CROUTEField;
                }
                set
                {
                    CROUTEField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Carrier
            {
                get
                {
                    return CarrierField;
                }
                set
                {
                    CarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrewBase
            {
                get
                {
                    return CrewBaseField;
                }
                set
                {
                    CrewBaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Day
            {
                get
                {
                    return DayField;
                }
                set
                {
                    DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Dep
            {
                get
                {
                    return DepField;
                }
                set
                {
                    DepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Flt
            {
                get
                {
                    return FltField;
                }
                set
                {
                    FltField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDBEG
            {
                get
                {
                    return GDBEGField;
                }
                set
                {
                    GDBEGField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDBEGBase
            {
                get
                {
                    return GDBEGBaseField;
                }
                set
                {
                    GDBEGBaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDBEGBaseDay
            {
                get
                {
                    return GDBEGBaseDayField;
                }
                set
                {
                    GDBEGBaseDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDBEGLocal
            {
                get
                {
                    return GDBEGLocalField;
                }
                set
                {
                    GDBEGLocalField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDBEGLocalDay
            {
                get
                {
                    return GDBEGLocalDayField;
                }
                set
                {
                    GDBEGLocalDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDBEG_Day
            {
                get
                {
                    return GDBEG_DayField;
                }
                set
                {
                    GDBEG_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDEND
            {
                get
                {
                    return GDENDField;
                }
                set
                {
                    GDENDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDENDBase
            {
                get
                {
                    return GDENDBaseField;
                }
                set
                {
                    GDENDBaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDENDBaseDay
            {
                get
                {
                    return GDENDBaseDayField;
                }
                set
                {
                    GDENDBaseDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDENDDay
            {
                get
                {
                    return GDENDDayField;
                }
                set
                {
                    GDENDDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDENDLocal
            {
                get
                {
                    return GDENDLocalField;
                }
                set
                {
                    GDENDLocalField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string GDENDLocalDay
            {
                get
                {
                    return GDENDLocalDayField;
                }
                set
                {
                    GDENDLocalDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Legcd
            {
                get
                {
                    return LegcdField;
                }
                set
                {
                    LegcdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string PAX
            {
                get
                {
                    return PAXField;
                }
                set
                {
                    PAXField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string STA
            {
                get
                {
                    return STAField;
                }
                set
                {
                    STAField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string STABase
            {
                get
                {
                    return STABaseField;
                }
                set
                {
                    STABaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string STABaseDay
            {
                get
                {
                    return STABaseDayField;
                }
                set
                {
                    STABaseDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string STADay
            {
                get
                {
                    return STADayField;
                }
                set
                {
                    STADayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string STALocal
            {
                get
                {
                    return STALocalField;
                }
                set
                {
                    STALocalField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string STALocalDay
            {
                get
                {
                    return STALocalDayField;
                }
                set
                {
                    STALocalDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string STD
            {
                get
                {
                    return STDField;
                }
                set
                {
                    STDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string STDBase
            {
                get
                {
                    return STDBaseField;
                }
                set
                {
                    STDBaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string STDLocal
            {
                get
                {
                    return STDLocalField;
                }
                set
                {
                    STDLocalField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "CrewMisssingTrnIndicatorsForPeriodd", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class CrewMisssingTrnIndicatorsForPeriodd : object
        {

            private int CrwMissTrnIndCountField;

            private TAIMSCrwMissTrnIndItem[] CrwMissTrnIndListField;

            private string ErrorExplanationField;

            [System.Runtime.Serialization.DataMember()]
            public int CrwMissTrnIndCount
            {
                get
                {
                    return CrwMissTrnIndCountField;
                }
                set
                {
                    CrwMissTrnIndCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSCrwMissTrnIndItem[] CrwMissTrnIndList
            {
                get
                {
                    return CrwMissTrnIndListField;
                }
                set
                {
                    CrwMissTrnIndListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrwMissTrnIndItem", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrwMissTrnIndItem : object
        {

            private string ErrorExplanationField;

            private string MissTrnInd_ACField;

            private string MissTrnInd_BASEField;

            private string MissTrnInd_DDField;

            private string MissTrnInd_DateField;

            private int MissTrnInd_IDField;

            private string MissTrnInd_MMField;

            private string MissTrnInd_NAMEField;

            private string MissTrnInd_POSField;

            private string MissTrnInd_YYField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MissTrnInd_AC
            {
                get
                {
                    return MissTrnInd_ACField;
                }
                set
                {
                    MissTrnInd_ACField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MissTrnInd_BASE
            {
                get
                {
                    return MissTrnInd_BASEField;
                }
                set
                {
                    MissTrnInd_BASEField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MissTrnInd_DD
            {
                get
                {
                    return MissTrnInd_DDField;
                }
                set
                {
                    MissTrnInd_DDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MissTrnInd_Date
            {
                get
                {
                    return MissTrnInd_DateField;
                }
                set
                {
                    MissTrnInd_DateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int MissTrnInd_ID
            {
                get
                {
                    return MissTrnInd_IDField;
                }
                set
                {
                    MissTrnInd_IDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MissTrnInd_MM
            {
                get
                {
                    return MissTrnInd_MMField;
                }
                set
                {
                    MissTrnInd_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MissTrnInd_NAME
            {
                get
                {
                    return MissTrnInd_NAMEField;
                }
                set
                {
                    MissTrnInd_NAMEField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MissTrnInd_POS
            {
                get
                {
                    return MissTrnInd_POSField;
                }
                set
                {
                    MissTrnInd_POSField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MissTrnInd_YY
            {
                get
                {
                    return MissTrnInd_YYField;
                }
                set
                {
                    MissTrnInd_YYField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "CrewPendingNotifForPeriodf", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class CrewPendingNotifForPeriodf : object
        {

            private string ErrorExplanationField;

            private int PendNotifCountField;

            private TAIMSCrewLogItm[] PendNotifListField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int PendNotifCount
            {
                get
                {
                    return PendNotifCountField;
                }
                set
                {
                    PendNotifCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSCrewLogItm[] PendNotifList
            {
                get
                {
                    return PendNotifListField;
                }
                set
                {
                    PendNotifListField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrewLogItm", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrewLogItm : object
        {

            private string LogByUserField;

            private string LogCrewIDField;

            private string LogForDayField;

            private string LogMessageField;

            private string LogOnDateField;

            private string LogOnDayTimeField;

            private string LogOnTimeField;

            [System.Runtime.Serialization.DataMember()]
            public string LogByUser
            {
                get
                {
                    return LogByUserField;
                }
                set
                {
                    LogByUserField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogCrewID
            {
                get
                {
                    return LogCrewIDField;
                }
                set
                {
                    LogCrewIDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogForDay
            {
                get
                {
                    return LogForDayField;
                }
                set
                {
                    LogForDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogMessage
            {
                get
                {
                    return LogMessageField;
                }
                set
                {
                    LogMessageField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogOnDate
            {
                get
                {
                    return LogOnDateField;
                }
                set
                {
                    LogOnDateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogOnDayTime
            {
                get
                {
                    return LogOnDayTimeField;
                }
                set
                {
                    LogOnDayTimeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogOnTime
            {
                get
                {
                    return LogOnTimeField;
                }
                set
                {
                    LogOnTimeField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "FetchCrewMessagesf", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class FetchCrewMessagesf : object
        {

            private string ErrorExplanationField;

            private string MessageListField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MessageList
            {
                get
                {
                    return MessageListField;
                }
                set
                {
                    MessageListField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "FetchCrewQualsf", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class FetchCrewQualsf : object
        {

            private TAIMSCrewQualsRec[] CrewQualListField;

            private string ErrorExplanationField;

            private int TotalCrewField;

            [System.Runtime.Serialization.DataMember()]
            public TAIMSCrewQualsRec[] CrewQualList
            {
                get
                {
                    return CrewQualListField;
                }
                set
                {
                    CrewQualListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int TotalCrew
            {
                get
                {
                    return TotalCrewField;
                }
                set
                {
                    TotalCrewField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrewQualsRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrewQualsRec : object
        {

            private string Address1Field;

            private string Address2Field;

            private string Address3Field;

            private string CellPhoneField;

            private string CityField;

            private string ContactField;

            private string Crew3LCField;

            private string CrewIDField;

            private string CrewNameField;

            private TAIMSCrewQualsItm[] CrewQualsField;

            private string EmailField;

            private string Email2Field;

            private string EmployBegField;

            private string EmployEndField;

            private string EndReasonField;

            private string FaxField;

            private string FirstNameField;

            private string GenderField;

            private string KinNameField;

            private string LastNameField;

            private string MailboxField;

            private string MaritalField;

            private string PayrollNmField;

            private string RelationField;

            private string ResidenceField;

            private string SecNumberField;

            private string StateField;

            private string TelephoneField;

            private string ZipCodeField;

            [System.Runtime.Serialization.DataMember()]
            public string Address1
            {
                get
                {
                    return Address1Field;
                }
                set
                {
                    Address1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Address2
            {
                get
                {
                    return Address2Field;
                }
                set
                {
                    Address2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Address3
            {
                get
                {
                    return Address3Field;
                }
                set
                {
                    Address3Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CellPhone
            {
                get
                {
                    return CellPhoneField;
                }
                set
                {
                    CellPhoneField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string City
            {
                get
                {
                    return CityField;
                }
                set
                {
                    CityField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Contact
            {
                get
                {
                    return ContactField;
                }
                set
                {
                    ContactField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Crew3LC
            {
                get
                {
                    return Crew3LCField;
                }
                set
                {
                    Crew3LCField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrewID
            {
                get
                {
                    return CrewIDField;
                }
                set
                {
                    CrewIDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrewName
            {
                get
                {
                    return CrewNameField;
                }
                set
                {
                    CrewNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSCrewQualsItm[] CrewQuals
            {
                get
                {
                    return CrewQualsField;
                }
                set
                {
                    CrewQualsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Email
            {
                get
                {
                    return EmailField;
                }
                set
                {
                    EmailField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Email2
            {
                get
                {
                    return Email2Field;
                }
                set
                {
                    Email2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string EmployBeg
            {
                get
                {
                    return EmployBegField;
                }
                set
                {
                    EmployBegField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string EmployEnd
            {
                get
                {
                    return EmployEndField;
                }
                set
                {
                    EmployEndField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string EndReason
            {
                get
                {
                    return EndReasonField;
                }
                set
                {
                    EndReasonField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Fax
            {
                get
                {
                    return FaxField;
                }
                set
                {
                    FaxField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FirstName
            {
                get
                {
                    return FirstNameField;
                }
                set
                {
                    FirstNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Gender
            {
                get
                {
                    return GenderField;
                }
                set
                {
                    GenderField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string KinName
            {
                get
                {
                    return KinNameField;
                }
                set
                {
                    KinNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LastName
            {
                get
                {
                    return LastNameField;
                }
                set
                {
                    LastNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Mailbox
            {
                get
                {
                    return MailboxField;
                }
                set
                {
                    MailboxField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Marital
            {
                get
                {
                    return MaritalField;
                }
                set
                {
                    MaritalField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string PayrollNm
            {
                get
                {
                    return PayrollNmField;
                }
                set
                {
                    PayrollNmField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Relation
            {
                get
                {
                    return RelationField;
                }
                set
                {
                    RelationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Residence
            {
                get
                {
                    return ResidenceField;
                }
                set
                {
                    ResidenceField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string SecNumber
            {
                get
                {
                    return SecNumberField;
                }
                set
                {
                    SecNumberField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string State
            {
                get
                {
                    return StateField;
                }
                set
                {
                    StateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Telephone
            {
                get
                {
                    return TelephoneField;
                }
                set
                {
                    TelephoneField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ZipCode
            {
                get
                {
                    return ZipCodeField;
                }
                set
                {
                    ZipCodeField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrewQualsItm", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrewQualsItm : object
        {

            private bool NotToBeRosterField;

            private bool PrimaryQualField;

            private string QualAcField;

            private string QualBDayField;

            private string QualBaseField;

            private string QualEDayField;

            private string QualPosField;

            private int RosterGroupField;

            [System.Runtime.Serialization.DataMember()]
            public bool NotToBeRoster
            {
                get
                {
                    return NotToBeRosterField;
                }
                set
                {
                    NotToBeRosterField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool PrimaryQual
            {
                get
                {
                    return PrimaryQualField;
                }
                set
                {
                    PrimaryQualField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualAc
            {
                get
                {
                    return QualAcField;
                }
                set
                {
                    QualAcField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualBDay
            {
                get
                {
                    return QualBDayField;
                }
                set
                {
                    QualBDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualBase
            {
                get
                {
                    return QualBaseField;
                }
                set
                {
                    QualBaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualEDay
            {
                get
                {
                    return QualEDayField;
                }
                set
                {
                    QualEDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualPos
            {
                get
                {
                    return QualPosField;
                }
                set
                {
                    QualPosField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int RosterGroup
            {
                get
                {
                    return RosterGroupField;
                }
                set
                {
                    RosterGroupField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrewCheckIn", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrewCheckIn : object
        {

            private string AcField;

            private string BaseField;

            private bool ConfirmedGivenField;

            private string ErrorExplanationField;

            private string FlightArrField;

            private string FlightDateField;

            private string FlightDepField;

            private string FlightNoField;

            private string FlightStaField;

            private string FlightStdField;

            private int IdField;

            private string NameField;

            private string PosField;

            private string ShortNameField;

            private int StatusField;

            [System.Runtime.Serialization.DataMember()]
            public string Ac
            {
                get
                {
                    return AcField;
                }
                set
                {
                    AcField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Base
            {
                get
                {
                    return BaseField;
                }
                set
                {
                    BaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool ConfirmedGiven
            {
                get
                {
                    return ConfirmedGivenField;
                }
                set
                {
                    ConfirmedGivenField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightArr
            {
                get
                {
                    return FlightArrField;
                }
                set
                {
                    FlightArrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDate
            {
                get
                {
                    return FlightDateField;
                }
                set
                {
                    FlightDateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDep
            {
                get
                {
                    return FlightDepField;
                }
                set
                {
                    FlightDepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightNo
            {
                get
                {
                    return FlightNoField;
                }
                set
                {
                    FlightNoField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightSta
            {
                get
                {
                    return FlightStaField;
                }
                set
                {
                    FlightStaField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightStd
            {
                get
                {
                    return FlightStdField;
                }
                set
                {
                    FlightStdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int Id
            {
                get
                {
                    return IdField;
                }
                set
                {
                    IdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Name
            {
                get
                {
                    return NameField;
                }
                set
                {
                    NameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Pos
            {
                get
                {
                    return PosField;
                }
                set
                {
                    PosField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ShortName
            {
                get
                {
                    return ShortNameField;
                }
                set
                {
                    ShortNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int Status
            {
                get
                {
                    return StatusField;
                }
                set
                {
                    StatusField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrewCheckOut", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrewCheckOut : object
        {

            private string AcField;

            private string BaseField;

            private bool ConfirmedField;

            private string ErrorExplanationField;

            private string FlightArrField;

            private string FlightDateField;

            private string FlightDepField;

            private string FlightNoField;

            private string FlightStaField;

            private string FlightStdField;

            private int IdField;

            private string NameField;

            private string PosField;

            private string ShortNameField;

            private int StatusField;

            [System.Runtime.Serialization.DataMember()]
            public string Ac
            {
                get
                {
                    return AcField;
                }
                set
                {
                    AcField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Base
            {
                get
                {
                    return BaseField;
                }
                set
                {
                    BaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool Confirmed
            {
                get
                {
                    return ConfirmedField;
                }
                set
                {
                    ConfirmedField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightArr
            {
                get
                {
                    return FlightArrField;
                }
                set
                {
                    FlightArrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDate
            {
                get
                {
                    return FlightDateField;
                }
                set
                {
                    FlightDateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightDep
            {
                get
                {
                    return FlightDepField;
                }
                set
                {
                    FlightDepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightNo
            {
                get
                {
                    return FlightNoField;
                }
                set
                {
                    FlightNoField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightSta
            {
                get
                {
                    return FlightStaField;
                }
                set
                {
                    FlightStaField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightStd
            {
                get
                {
                    return FlightStdField;
                }
                set
                {
                    FlightStdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int Id
            {
                get
                {
                    return IdField;
                }
                set
                {
                    IdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Name
            {
                get
                {
                    return NameField;
                }
                set
                {
                    NameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Pos
            {
                get
                {
                    return PosField;
                }
                set
                {
                    PosField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ShortName
            {
                get
                {
                    return ShortNameField;
                }
                set
                {
                    ShortNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int Status
            {
                get
                {
                    return StatusField;
                }
                set
                {
                    StatusField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "GetCrewListf", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class GetCrewListf : object
        {

            private TAIMSGetCrewItm[] CrewListField;

            private string ErrorExplanationField;

            private int GetCrewListCountField;

            [System.Runtime.Serialization.DataMember()]
            public TAIMSGetCrewItm[] CrewList
            {
                get
                {
                    return CrewListField;
                }
                set
                {
                    CrewListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int GetCrewListCount
            {
                get
                {
                    return GetCrewListCountField;
                }
                set
                {
                    GetCrewListCountField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSGetCrewItm", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSGetCrewItm : object
        {

            private string AddressField;

            private string CityField;

            private string ContactCellField;

            private string ContactCell2Field;

            private string ContactCell3Field;

            private string ContactCoMobField;

            private string ContactCoMob2Field;

            private string ContactCoMob3Field;

            private string ContactTelField;

            private string ContactTel2Field;

            private string ContactTel3Field;

            private string CrewNameField;

            private string EmailField;

            private string EmploymentDateField;

            private string IdField;

            private string Language1Field;

            private string Language2Field;

            private string LicenseNumField;

            private string LocationField;

            private string MaritalField;

            private string NationalityField;

            private string NationalityCodeField;

            private string NextKinContactField;

            private string NextKinNameField;

            private string NextKinRelsnField;

            private string QualsField;

            private string SexField;

            private string ShortNameField;

            private string StateField;

            private string ZipField;

            [System.Runtime.Serialization.DataMember()]
            public string Address
            {
                get
                {
                    return AddressField;
                }
                set
                {
                    AddressField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string City
            {
                get
                {
                    return CityField;
                }
                set
                {
                    CityField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ContactCell
            {
                get
                {
                    return ContactCellField;
                }
                set
                {
                    ContactCellField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ContactCell2
            {
                get
                {
                    return ContactCell2Field;
                }
                set
                {
                    ContactCell2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ContactCell3
            {
                get
                {
                    return ContactCell3Field;
                }
                set
                {
                    ContactCell3Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ContactCoMob
            {
                get
                {
                    return ContactCoMobField;
                }
                set
                {
                    ContactCoMobField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ContactCoMob2
            {
                get
                {
                    return ContactCoMob2Field;
                }
                set
                {
                    ContactCoMob2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ContactCoMob3
            {
                get
                {
                    return ContactCoMob3Field;
                }
                set
                {
                    ContactCoMob3Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ContactTel
            {
                get
                {
                    return ContactTelField;
                }
                set
                {
                    ContactTelField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ContactTel2
            {
                get
                {
                    return ContactTel2Field;
                }
                set
                {
                    ContactTel2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ContactTel3
            {
                get
                {
                    return ContactTel3Field;
                }
                set
                {
                    ContactTel3Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrewName
            {
                get
                {
                    return CrewNameField;
                }
                set
                {
                    CrewNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Email
            {
                get
                {
                    return EmailField;
                }
                set
                {
                    EmailField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string EmploymentDate
            {
                get
                {
                    return EmploymentDateField;
                }
                set
                {
                    EmploymentDateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Id
            {
                get
                {
                    return IdField;
                }
                set
                {
                    IdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Language1
            {
                get
                {
                    return Language1Field;
                }
                set
                {
                    Language1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Language2
            {
                get
                {
                    return Language2Field;
                }
                set
                {
                    Language2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LicenseNum
            {
                get
                {
                    return LicenseNumField;
                }
                set
                {
                    LicenseNumField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Location
            {
                get
                {
                    return LocationField;
                }
                set
                {
                    LocationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Marital
            {
                get
                {
                    return MaritalField;
                }
                set
                {
                    MaritalField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Nationality
            {
                get
                {
                    return NationalityField;
                }
                set
                {
                    NationalityField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string NationalityCode
            {
                get
                {
                    return NationalityCodeField;
                }
                set
                {
                    NationalityCodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string NextKinContact
            {
                get
                {
                    return NextKinContactField;
                }
                set
                {
                    NextKinContactField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string NextKinName
            {
                get
                {
                    return NextKinNameField;
                }
                set
                {
                    NextKinNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string NextKinRelsn
            {
                get
                {
                    return NextKinRelsnField;
                }
                set
                {
                    NextKinRelsnField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Quals
            {
                get
                {
                    return QualsField;
                }
                set
                {
                    QualsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Sex
            {
                get
                {
                    return SexField;
                }
                set
                {
                    SexField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ShortName
            {
                get
                {
                    return ShortNameField;
                }
                set
                {
                    ShortNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string State
            {
                get
                {
                    return StateField;
                }
                set
                {
                    StateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Zip
            {
                get
                {
                    return ZipField;
                }
                set
                {
                    ZipField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "CrewScheduleChangesForPeriodf", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class CrewScheduleChangesForPeriodf : object
        {

            private TAIMSCrewLogItm[] CrewLogListField;

            private string ErrorExplanationField;

            private int LogCounterField;

            [System.Runtime.Serialization.DataMember()]
            public TAIMSCrewLogItm[] CrewLogList
            {
                get
                {
                    return CrewLogListField;
                }
                set
                {
                    CrewLogListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int LogCounter
            {
                get
                {
                    return LogCounterField;
                }
                set
                {
                    LogCounterField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSUnfinCrewRoutesList", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSUnfinCrewRoutesList : object
        {

            private string ErrorExplanationField;

            private int UnfinRteCountField;

            private TAIMSUnfinRteItem[] UnfinRteListField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int UnfinRteCount
            {
                get
                {
                    return UnfinRteCountField;
                }
                set
                {
                    UnfinRteCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSUnfinRteItem[] UnfinRteList
            {
                get
                {
                    return UnfinRteListField;
                }
                set
                {
                    UnfinRteListField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSUnfinRteItem", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSUnfinRteItem : object
        {

            private string ErrorExplanationField;

            private string UnFinDDField;

            private string UnFinDateField;

            private string UnFinMMField;

            private string UnFinRouteField;

            private string UnFinYYField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string UnFinDD
            {
                get
                {
                    return UnFinDDField;
                }
                set
                {
                    UnFinDDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string UnFinDate
            {
                get
                {
                    return UnFinDateField;
                }
                set
                {
                    UnFinDateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string UnFinMM
            {
                get
                {
                    return UnFinMMField;
                }
                set
                {
                    UnFinMMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string UnFinRoute
            {
                get
                {
                    return UnFinRouteField;
                }
                set
                {
                    UnFinRouteField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string UnFinYY
            {
                get
                {
                    return UnFinYYField;
                }
                set
                {
                    UnFinYYField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "FltSchedModificationLog", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class FltSchedModificationLog : object
        {

            private string ErrorExplanationField;

            private TAimsFltsSchedModLogItem[] FltsSchedModificationListField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAimsFltsSchedModLogItem[] FltsSchedModificationList
            {
                get
                {
                    return FltsSchedModificationListField;
                }
                set
                {
                    FltsSchedModificationListField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAimsFltsSchedModLogItem", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAimsFltsSchedModLogItem : object
        {

            private string FltsSchedModLog_AcOwnerField;

            private string FltsSchedModLog_AcRegField;

            private string FltsSchedModLog_AcTypeField;

            private string FltsSchedModLog_ArrField;

            private string FltsSchedModLog_ArrDlyCode1Field;

            private string FltsSchedModLog_ArrDlyCode2Field;

            private string FltsSchedModLog_ArrDlyTime1Field;

            private string FltsSchedModLog_ArrDlyTime2Field;

            private string FltsSchedModLog_ArrGateField;

            private string FltsSchedModLog_ArrStandField;

            private string FltsSchedModLog_CarrierField;

            private string FltsSchedModLog_DayField;

            private string FltsSchedModLog_DepField;

            private string FltsSchedModLog_DepGateField;

            private string FltsSchedModLog_DepStandField;

            private string FltsSchedModLog_DlyCode1Field;

            private string FltsSchedModLog_DlyCode2Field;

            private string FltsSchedModLog_DlyCode3Field;

            private string FltsSchedModLog_DlyCode4Field;

            private string FltsSchedModLog_DlyTime1Field;

            private string FltsSchedModLog_DlyTime2Field;

            private string FltsSchedModLog_DlyTime3Field;

            private string FltsSchedModLog_DlyTime4Field;

            private int FltsSchedModLog_ExpPassngrField;

            private string FltsSchedModLog_FlightTypeField;

            private int FltsSchedModLog_FltField;

            private string FltsSchedModLog_LOCSTN_ATAField;

            private string FltsSchedModLog_LOCSTN_ATA_DayField;

            private string FltsSchedModLog_LOCSTN_ATDField;

            private string FltsSchedModLog_LOCSTN_ATD_DayField;

            private string FltsSchedModLog_LOCSTN_ETAField;

            private string FltsSchedModLog_LOCSTN_ETA_DayField;

            private string FltsSchedModLog_LOCSTN_ETDField;

            private string FltsSchedModLog_LOCSTN_ETD_DayField;

            private string FltsSchedModLog_LOCSTN_STAField;

            private string FltsSchedModLog_LOCSTN_STA_DayField;

            private string FltsSchedModLog_LOCSTN_STDField;

            private string FltsSchedModLog_LOCSTN_STD_DayField;

            private string FltsSchedModLog_LOCSTN_TDWNField;

            private string FltsSchedModLog_LOCSTN_TDWN_DayField;

            private string FltsSchedModLog_LOCSTN_TOFFField;

            private string FltsSchedModLog_LOCSTN_TOFF_DayField;

            private string FltsSchedModLog_LegCdField;

            private int FltsSchedModLog_NoOfPaxField;

            private string FltsSchedModLog_OnDayField;

            private string FltsSchedModLog_OnTimeField;

            private string FltsSchedModLog_StatusField;

            private string FltsSchedModLog_UTC_ATAField;

            private string FltsSchedModLog_UTC_ATA_DayField;

            private string FltsSchedModLog_UTC_ATDField;

            private string FltsSchedModLog_UTC_ATD_DayField;

            private string FltsSchedModLog_UTC_ETAField;

            private string FltsSchedModLog_UTC_ETA_DayField;

            private string FltsSchedModLog_UTC_ETDField;

            private string FltsSchedModLog_UTC_ETD_DayField;

            private string FltsSchedModLog_UTC_STAField;

            private string FltsSchedModLog_UTC_STA_DayField;

            private string FltsSchedModLog_UTC_STDField;

            private string FltsSchedModLog_UTC_STD_DayField;

            private string FltsSchedModLog_UTC_TDWNField;

            private string FltsSchedModLog_UTC_TDWN_DayField;

            private string FltsSchedModLog_UTC_TOFFField;

            private string FltsSchedModLog_UTC_TOFF_DayField;

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_AcOwner
            {
                get
                {
                    return FltsSchedModLog_AcOwnerField;
                }
                set
                {
                    FltsSchedModLog_AcOwnerField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_AcReg
            {
                get
                {
                    return FltsSchedModLog_AcRegField;
                }
                set
                {
                    FltsSchedModLog_AcRegField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_AcType
            {
                get
                {
                    return FltsSchedModLog_AcTypeField;
                }
                set
                {
                    FltsSchedModLog_AcTypeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_Arr
            {
                get
                {
                    return FltsSchedModLog_ArrField;
                }
                set
                {
                    FltsSchedModLog_ArrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_ArrDlyCode1
            {
                get
                {
                    return FltsSchedModLog_ArrDlyCode1Field;
                }
                set
                {
                    FltsSchedModLog_ArrDlyCode1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_ArrDlyCode2
            {
                get
                {
                    return FltsSchedModLog_ArrDlyCode2Field;
                }
                set
                {
                    FltsSchedModLog_ArrDlyCode2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_ArrDlyTime1
            {
                get
                {
                    return FltsSchedModLog_ArrDlyTime1Field;
                }
                set
                {
                    FltsSchedModLog_ArrDlyTime1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_ArrDlyTime2
            {
                get
                {
                    return FltsSchedModLog_ArrDlyTime2Field;
                }
                set
                {
                    FltsSchedModLog_ArrDlyTime2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_ArrGate
            {
                get
                {
                    return FltsSchedModLog_ArrGateField;
                }
                set
                {
                    FltsSchedModLog_ArrGateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_ArrStand
            {
                get
                {
                    return FltsSchedModLog_ArrStandField;
                }
                set
                {
                    FltsSchedModLog_ArrStandField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_Carrier
            {
                get
                {
                    return FltsSchedModLog_CarrierField;
                }
                set
                {
                    FltsSchedModLog_CarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_Day
            {
                get
                {
                    return FltsSchedModLog_DayField;
                }
                set
                {
                    FltsSchedModLog_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_Dep
            {
                get
                {
                    return FltsSchedModLog_DepField;
                }
                set
                {
                    FltsSchedModLog_DepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DepGate
            {
                get
                {
                    return FltsSchedModLog_DepGateField;
                }
                set
                {
                    FltsSchedModLog_DepGateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DepStand
            {
                get
                {
                    return FltsSchedModLog_DepStandField;
                }
                set
                {
                    FltsSchedModLog_DepStandField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DlyCode1
            {
                get
                {
                    return FltsSchedModLog_DlyCode1Field;
                }
                set
                {
                    FltsSchedModLog_DlyCode1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DlyCode2
            {
                get
                {
                    return FltsSchedModLog_DlyCode2Field;
                }
                set
                {
                    FltsSchedModLog_DlyCode2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DlyCode3
            {
                get
                {
                    return FltsSchedModLog_DlyCode3Field;
                }
                set
                {
                    FltsSchedModLog_DlyCode3Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DlyCode4
            {
                get
                {
                    return FltsSchedModLog_DlyCode4Field;
                }
                set
                {
                    FltsSchedModLog_DlyCode4Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DlyTime1
            {
                get
                {
                    return FltsSchedModLog_DlyTime1Field;
                }
                set
                {
                    FltsSchedModLog_DlyTime1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DlyTime2
            {
                get
                {
                    return FltsSchedModLog_DlyTime2Field;
                }
                set
                {
                    FltsSchedModLog_DlyTime2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DlyTime3
            {
                get
                {
                    return FltsSchedModLog_DlyTime3Field;
                }
                set
                {
                    FltsSchedModLog_DlyTime3Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_DlyTime4
            {
                get
                {
                    return FltsSchedModLog_DlyTime4Field;
                }
                set
                {
                    FltsSchedModLog_DlyTime4Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int FltsSchedModLog_ExpPassngr
            {
                get
                {
                    return FltsSchedModLog_ExpPassngrField;
                }
                set
                {
                    FltsSchedModLog_ExpPassngrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_FlightType
            {
                get
                {
                    return FltsSchedModLog_FlightTypeField;
                }
                set
                {
                    FltsSchedModLog_FlightTypeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int FltsSchedModLog_Flt
            {
                get
                {
                    return FltsSchedModLog_FltField;
                }
                set
                {
                    FltsSchedModLog_FltField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_ATA
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_ATAField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_ATAField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_ATA_Day
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_ATA_DayField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_ATA_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_ATD
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_ATDField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_ATDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_ATD_Day
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_ATD_DayField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_ATD_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_ETA
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_ETAField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_ETAField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_ETA_Day
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_ETA_DayField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_ETA_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_ETD
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_ETDField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_ETDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_ETD_Day
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_ETD_DayField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_ETD_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_STA
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_STAField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_STAField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_STA_Day
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_STA_DayField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_STA_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_STD
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_STDField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_STDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_STD_Day
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_STD_DayField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_STD_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_TDWN
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_TDWNField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_TDWNField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_TDWN_Day
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_TDWN_DayField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_TDWN_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_TOFF
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_TOFFField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_TOFFField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LOCSTN_TOFF_Day
            {
                get
                {
                    return FltsSchedModLog_LOCSTN_TOFF_DayField;
                }
                set
                {
                    FltsSchedModLog_LOCSTN_TOFF_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_LegCd
            {
                get
                {
                    return FltsSchedModLog_LegCdField;
                }
                set
                {
                    FltsSchedModLog_LegCdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int FltsSchedModLog_NoOfPax
            {
                get
                {
                    return FltsSchedModLog_NoOfPaxField;
                }
                set
                {
                    FltsSchedModLog_NoOfPaxField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_OnDay
            {
                get
                {
                    return FltsSchedModLog_OnDayField;
                }
                set
                {
                    FltsSchedModLog_OnDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_OnTime
            {
                get
                {
                    return FltsSchedModLog_OnTimeField;
                }
                set
                {
                    FltsSchedModLog_OnTimeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_Status
            {
                get
                {
                    return FltsSchedModLog_StatusField;
                }
                set
                {
                    FltsSchedModLog_StatusField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_ATA
            {
                get
                {
                    return FltsSchedModLog_UTC_ATAField;
                }
                set
                {
                    FltsSchedModLog_UTC_ATAField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_ATA_Day
            {
                get
                {
                    return FltsSchedModLog_UTC_ATA_DayField;
                }
                set
                {
                    FltsSchedModLog_UTC_ATA_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_ATD
            {
                get
                {
                    return FltsSchedModLog_UTC_ATDField;
                }
                set
                {
                    FltsSchedModLog_UTC_ATDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_ATD_Day
            {
                get
                {
                    return FltsSchedModLog_UTC_ATD_DayField;
                }
                set
                {
                    FltsSchedModLog_UTC_ATD_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_ETA
            {
                get
                {
                    return FltsSchedModLog_UTC_ETAField;
                }
                set
                {
                    FltsSchedModLog_UTC_ETAField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_ETA_Day
            {
                get
                {
                    return FltsSchedModLog_UTC_ETA_DayField;
                }
                set
                {
                    FltsSchedModLog_UTC_ETA_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_ETD
            {
                get
                {
                    return FltsSchedModLog_UTC_ETDField;
                }
                set
                {
                    FltsSchedModLog_UTC_ETDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_ETD_Day
            {
                get
                {
                    return FltsSchedModLog_UTC_ETD_DayField;
                }
                set
                {
                    FltsSchedModLog_UTC_ETD_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_STA
            {
                get
                {
                    return FltsSchedModLog_UTC_STAField;
                }
                set
                {
                    FltsSchedModLog_UTC_STAField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_STA_Day
            {
                get
                {
                    return FltsSchedModLog_UTC_STA_DayField;
                }
                set
                {
                    FltsSchedModLog_UTC_STA_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_STD
            {
                get
                {
                    return FltsSchedModLog_UTC_STDField;
                }
                set
                {
                    FltsSchedModLog_UTC_STDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_STD_Day
            {
                get
                {
                    return FltsSchedModLog_UTC_STD_DayField;
                }
                set
                {
                    FltsSchedModLog_UTC_STD_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_TDWN
            {
                get
                {
                    return FltsSchedModLog_UTC_TDWNField;
                }
                set
                {
                    FltsSchedModLog_UTC_TDWNField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_TDWN_Day
            {
                get
                {
                    return FltsSchedModLog_UTC_TDWN_DayField;
                }
                set
                {
                    FltsSchedModLog_UTC_TDWN_DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_TOFF
            {
                get
                {
                    return FltsSchedModLog_UTC_TOFFField;
                }
                set
                {
                    FltsSchedModLog_UTC_TOFFField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FltsSchedModLog_UTC_TOFF_Day
            {
                get
                {
                    return FltsSchedModLog_UTC_TOFF_DayField;
                }
                set
                {
                    FltsSchedModLog_UTC_TOFF_DayField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "GreenwichMeanTimeDifferenceInMinsf", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class GreenwichMeanTimeDifferenceInMinsf : object
        {

            private string ErrorExplanationField;

            private short resultField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short result
            {
                get
                {
                    return resultField;
                }
                set
                {
                    resultField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrewInfo", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrewInfo : object
        {

            private string Address1Field;

            private string Address2Field;

            private string Address3Field;

            private string CellPhoneField;

            private string CityField;

            private string ContactField;

            private string Crew3LCField;

            private string CrewIDField;

            private string CrewNameField;

            private string EmailField;

            private string Email2Field;

            private string EmployBegField;

            private string EmployEndField;

            private string EndReasonField;

            private string FaxField;

            private string GenderField;

            private string KinNameField;

            private string MailBoxField;

            private string MaritalField;

            private string PayrollNmField;

            private TAIMSQualRec[] QualItemsField;

            private string RelationField;

            private string ResidenceField;

            private string SecNumberField;

            private string StateField;

            private string TelephoneField;

            private string ZipCodeField;

            [System.Runtime.Serialization.DataMember()]
            public string Address1
            {
                get
                {
                    return Address1Field;
                }
                set
                {
                    Address1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Address2
            {
                get
                {
                    return Address2Field;
                }
                set
                {
                    Address2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Address3
            {
                get
                {
                    return Address3Field;
                }
                set
                {
                    Address3Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CellPhone
            {
                get
                {
                    return CellPhoneField;
                }
                set
                {
                    CellPhoneField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string City
            {
                get
                {
                    return CityField;
                }
                set
                {
                    CityField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Contact
            {
                get
                {
                    return ContactField;
                }
                set
                {
                    ContactField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Crew3LC
            {
                get
                {
                    return Crew3LCField;
                }
                set
                {
                    Crew3LCField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrewID
            {
                get
                {
                    return CrewIDField;
                }
                set
                {
                    CrewIDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrewName
            {
                get
                {
                    return CrewNameField;
                }
                set
                {
                    CrewNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Email
            {
                get
                {
                    return EmailField;
                }
                set
                {
                    EmailField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Email2
            {
                get
                {
                    return Email2Field;
                }
                set
                {
                    Email2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string EmployBeg
            {
                get
                {
                    return EmployBegField;
                }
                set
                {
                    EmployBegField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string EmployEnd
            {
                get
                {
                    return EmployEndField;
                }
                set
                {
                    EmployEndField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string EndReason
            {
                get
                {
                    return EndReasonField;
                }
                set
                {
                    EndReasonField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Fax
            {
                get
                {
                    return FaxField;
                }
                set
                {
                    FaxField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Gender
            {
                get
                {
                    return GenderField;
                }
                set
                {
                    GenderField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string KinName
            {
                get
                {
                    return KinNameField;
                }
                set
                {
                    KinNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MailBox
            {
                get
                {
                    return MailBoxField;
                }
                set
                {
                    MailBoxField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Marital
            {
                get
                {
                    return MaritalField;
                }
                set
                {
                    MaritalField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string PayrollNm
            {
                get
                {
                    return PayrollNmField;
                }
                set
                {
                    PayrollNmField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public TAIMSQualRec[] QualItems
            {
                get
                {
                    return QualItemsField;
                }
                set
                {
                    QualItemsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Relation
            {
                get
                {
                    return RelationField;
                }
                set
                {
                    RelationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Residence
            {
                get
                {
                    return ResidenceField;
                }
                set
                {
                    ResidenceField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string SecNumber
            {
                get
                {
                    return SecNumberField;
                }
                set
                {
                    SecNumberField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string State
            {
                get
                {
                    return StateField;
                }
                set
                {
                    StateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Telephone
            {
                get
                {
                    return TelephoneField;
                }
                set
                {
                    TelephoneField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ZipCode
            {
                get
                {
                    return ZipCodeField;
                }
                set
                {
                    ZipCodeField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSQualRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSQualRec : object
        {

            private string QualAcField;

            private string QualBDayField;

            private string QualBaseField;

            private string QualEDayField;

            private string QualPosField;

            [System.Runtime.Serialization.DataMember()]
            public string QualAc
            {
                get
                {
                    return QualAcField;
                }
                set
                {
                    QualAcField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualBDay
            {
                get
                {
                    return QualBDayField;
                }
                set
                {
                    QualBDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualBase
            {
                get
                {
                    return QualBaseField;
                }
                set
                {
                    QualBaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualEDay
            {
                get
                {
                    return QualEDayField;
                }
                set
                {
                    QualEDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualPos
            {
                get
                {
                    return QualPosField;
                }
                set
                {
                    QualPosField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "InsertMemb", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class InsertMemb : object
        {

            private string ErrorExplanationField;

            private bool IsMemberInsertedField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool IsMemberInserted
            {
                get
                {
                    return IsMemberInsertedField;
                }
                set
                {
                    IsMemberInsertedField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrewInfo2", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrewInfo2 : object
        {

            private string CrewIDField;

            private string CrewNameField;

            private string GenderField;

            private string EmployBegField;

            private bool ExemptSBYField;

            private entitlementitem[] EntitlementCategoryListField;

            private bool UnderTrainField;

            private TAIMSCrwCtgsItems[] CrewCategsField;

            private TAIMSQualsItems2[] CrewQualsField;

            [System.Runtime.Serialization.DataMember()]
            public string CrewID
            {
                get
                {
                    return CrewIDField;
                }
                set
                {
                    CrewIDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string CrewName
            {
                get
                {
                    return CrewNameField;
                }
                set
                {
                    CrewNameField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Gender
            {
                get
                {
                    return GenderField;
                }
                set
                {
                    GenderField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 3)]
            public string EmployBeg
            {
                get
                {
                    return EmployBegField;
                }
                set
                {
                    EmployBegField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 4)]
            public bool ExemptSBY
            {
                get
                {
                    return ExemptSBYField;
                }
                set
                {
                    ExemptSBYField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 5)]
            public entitlementitem[] EntitlementCategoryList
            {
                get
                {
                    return EntitlementCategoryListField;
                }
                set
                {
                    EntitlementCategoryListField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 6)]
            public bool UnderTrain
            {
                get
                {
                    return UnderTrainField;
                }
                set
                {
                    UnderTrainField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 7)]
            public TAIMSCrwCtgsItems[] CrewCategs
            {
                get
                {
                    return CrewCategsField;
                }
                set
                {
                    CrewCategsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 8)]
            public TAIMSQualsItems2[] CrewQuals
            {
                get
                {
                    return CrewQualsField;
                }
                set
                {
                    CrewQualsField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "entitlementitem", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class entitlementitem : object
        {
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSCrwCtgsItems", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSCrwCtgsItems : object
        {

            private short CrewCatIdxField;

            private string CrewCatBDateField;

            private string CrewCatEDateField;

            [System.Runtime.Serialization.DataMember()]
            public short CrewCatIdx
            {
                get
                {
                    return CrewCatIdxField;
                }
                set
                {
                    CrewCatIdxField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 1)]
            public string CrewCatBDate
            {
                get
                {
                    return CrewCatBDateField;
                }
                set
                {
                    CrewCatBDateField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 2)]
            public string CrewCatEDate
            {
                get
                {
                    return CrewCatEDateField;
                }
                set
                {
                    CrewCatEDateField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSQualsItems2", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSQualsItems2 : object
        {

            private bool NotToBeRosterField;

            private bool PrimaryQualField;

            private string QualAcField;

            private string QualBDayField;

            private string QualBaseField;

            private string QualEDayField;

            private string QualPosField;

            private short RosterGroupField;

            [System.Runtime.Serialization.DataMember()]
            public bool NotToBeRoster
            {
                get
                {
                    return NotToBeRosterField;
                }
                set
                {
                    NotToBeRosterField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool PrimaryQual
            {
                get
                {
                    return PrimaryQualField;
                }
                set
                {
                    PrimaryQualField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualAc
            {
                get
                {
                    return QualAcField;
                }
                set
                {
                    QualAcField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualBDay
            {
                get
                {
                    return QualBDayField;
                }
                set
                {
                    QualBDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualBase
            {
                get
                {
                    return QualBaseField;
                }
                set
                {
                    QualBaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualEDay
            {
                get
                {
                    return QualEDayField;
                }
                set
                {
                    QualEDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string QualPos
            {
                get
                {
                    return QualPosField;
                }
                set
                {
                    QualPosField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short RosterGroup
            {
                get
                {
                    return RosterGroupField;
                }
                set
                {
                    RosterGroupField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "InsertMemb2", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class InsertMemb2 : object
        {

            private string ErrorExplanationField;

            private bool IsMemberInsertedField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool IsMemberInserted
            {
                get
                {
                    return IsMemberInsertedField;
                }
                set
                {
                    IsMemberInsertedField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "UpdateLegInf", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class UpdateLegInf : object
        {

            private string ErrorExplanationField;

            private bool IsUpdatedField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool IsUpdated
            {
                get
                {
                    return IsUpdatedField;
                }
                set
                {
                    IsUpdatedField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSVRData", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSVRData : object
        {

            private string FlightDayField;

            private string FlightMonthField;

            private string FlightYearField;

            private string FlightDepField;

            private string FlightCarrierField;

            private string FlightNumField;

            private string FlightLegCdField;

            private string EstimDepDayField;

            private string EstimDepMonthField;

            private string EstimDepYearField;

            private TAIMSActualTimesRec ActualTimesField;

            private TAIMSDelayTimesRec DelayTimesField;

            private TAIMSLandsTkofRec[] TAIMSLandsTkofItemsField;

            private TAIMSFacilitiesRec[] FacilitiesItemsField;

            private TAIMSPasengersRec[] PasengersItemsField;

            private TAIMSFuelCargoRec[] FuelItemsField;

            private TAIMSFuelCargoRec[] CargoItemsField;

            private TAIMSSupplierRec[] SupplierItemsField;

            private TAIMSAdditionalInfo AdditionalInfoField;

            [System.Runtime.Serialization.DataMember()]
            public string FlightDay
            {
                get
                {
                    return FlightDayField;
                }
                set
                {
                    FlightDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightMonth
            {
                get
                {
                    return FlightMonthField;
                }
                set
                {
                    FlightMonthField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string FlightYear
            {
                get
                {
                    return FlightYearField;
                }
                set
                {
                    FlightYearField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 3)]
            public string FlightDep
            {
                get
                {
                    return FlightDepField;
                }
                set
                {
                    FlightDepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 4)]
            public string FlightCarrier
            {
                get
                {
                    return FlightCarrierField;
                }
                set
                {
                    FlightCarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 5)]
            public string FlightNum
            {
                get
                {
                    return FlightNumField;
                }
                set
                {
                    FlightNumField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 6)]
            public string FlightLegCd
            {
                get
                {
                    return FlightLegCdField;
                }
                set
                {
                    FlightLegCdField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 7)]
            public string EstimDepDay
            {
                get
                {
                    return EstimDepDayField;
                }
                set
                {
                    EstimDepDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 8)]
            public string EstimDepMonth
            {
                get
                {
                    return EstimDepMonthField;
                }
                set
                {
                    EstimDepMonthField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 9)]
            public string EstimDepYear
            {
                get
                {
                    return EstimDepYearField;
                }
                set
                {
                    EstimDepYearField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 10)]
            public TAIMSActualTimesRec ActualTimes
            {
                get
                {
                    return ActualTimesField;
                }
                set
                {
                    ActualTimesField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 11)]
            public TAIMSDelayTimesRec DelayTimes
            {
                get
                {
                    return DelayTimesField;
                }
                set
                {
                    DelayTimesField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 12)]
            public TAIMSLandsTkofRec[] TAIMSLandsTkofItems
            {
                get
                {
                    return TAIMSLandsTkofItemsField;
                }
                set
                {
                    TAIMSLandsTkofItemsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 13)]
            public TAIMSFacilitiesRec[] FacilitiesItems
            {
                get
                {
                    return FacilitiesItemsField;
                }
                set
                {
                    FacilitiesItemsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 14)]
            public TAIMSPasengersRec[] PasengersItems
            {
                get
                {
                    return PasengersItemsField;
                }
                set
                {
                    PasengersItemsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 15)]
            public TAIMSFuelCargoRec[] FuelItems
            {
                get
                {
                    return FuelItemsField;
                }
                set
                {
                    FuelItemsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 16)]
            public TAIMSFuelCargoRec[] CargoItems
            {
                get
                {
                    return CargoItemsField;
                }
                set
                {
                    CargoItemsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 17)]
            public TAIMSSupplierRec[] SupplierItems
            {
                get
                {
                    return SupplierItemsField;
                }
                set
                {
                    SupplierItemsField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 18)]
            public TAIMSAdditionalInfo AdditionalInfo
            {
                get
                {
                    return AdditionalInfoField;
                }
                set
                {
                    AdditionalInfoField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSActualTimesRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSActualTimesRec : object
        {

            private string ATA_HHField;

            private string ATA_MMField;

            private string ATD_HHField;

            private string ATD_MMField;

            private string TDOWN_HHField;

            private string TDOWN_MMField;

            private string TKOF_HHField;

            private string TKOF_MMField;

            [System.Runtime.Serialization.DataMember()]
            public string ATA_HH
            {
                get
                {
                    return ATA_HHField;
                }
                set
                {
                    ATA_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATA_MM
            {
                get
                {
                    return ATA_MMField;
                }
                set
                {
                    ATA_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATD_HH
            {
                get
                {
                    return ATD_HHField;
                }
                set
                {
                    ATD_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ATD_MM
            {
                get
                {
                    return ATD_MMField;
                }
                set
                {
                    ATD_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string TDOWN_HH
            {
                get
                {
                    return TDOWN_HHField;
                }
                set
                {
                    TDOWN_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string TDOWN_MM
            {
                get
                {
                    return TDOWN_MMField;
                }
                set
                {
                    TDOWN_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string TKOF_HH
            {
                get
                {
                    return TKOF_HHField;
                }
                set
                {
                    TKOF_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string TKOF_MM
            {
                get
                {
                    return TKOF_MMField;
                }
                set
                {
                    TKOF_MMField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSDelayTimesRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSDelayTimesRec : object
        {

            private string DepDelayTime1_HHField;

            private string DepDelayTime1_MMField;

            private string DepDelayCode1Field;

            private string DepDelayTime2_HHField;

            private string DepDelayTime2_MMField;

            private string DepDelayCode2Field;

            private string DepDelayTime3_HHField;

            private string DepDelayTime3_MMField;

            private string DepDelayCode3Field;

            private string DepDelayTime4_HHField;

            private string DepDelayTime4_MMField;

            private string DepDelayCode4Field;

            private string ArrDelayTime1_HHField;

            private string ArrDelayTime1_MMField;

            private string ArrDelayCode1Field;

            private string ArrDelayTime2_HHField;

            private string ArrDelayTime2_MMField;

            private string ArrDelayCode2Field;

            [System.Runtime.Serialization.DataMember()]
            public string DepDelayTime1_HH
            {
                get
                {
                    return DepDelayTime1_HHField;
                }
                set
                {
                    DepDelayTime1_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string DepDelayTime1_MM
            {
                get
                {
                    return DepDelayTime1_MMField;
                }
                set
                {
                    DepDelayTime1_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 2)]
            public string DepDelayCode1
            {
                get
                {
                    return DepDelayCode1Field;
                }
                set
                {
                    DepDelayCode1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 3)]
            public string DepDelayTime2_HH
            {
                get
                {
                    return DepDelayTime2_HHField;
                }
                set
                {
                    DepDelayTime2_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 4)]
            public string DepDelayTime2_MM
            {
                get
                {
                    return DepDelayTime2_MMField;
                }
                set
                {
                    DepDelayTime2_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 5)]
            public string DepDelayCode2
            {
                get
                {
                    return DepDelayCode2Field;
                }
                set
                {
                    DepDelayCode2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 6)]
            public string DepDelayTime3_HH
            {
                get
                {
                    return DepDelayTime3_HHField;
                }
                set
                {
                    DepDelayTime3_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 7)]
            public string DepDelayTime3_MM
            {
                get
                {
                    return DepDelayTime3_MMField;
                }
                set
                {
                    DepDelayTime3_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 8)]
            public string DepDelayCode3
            {
                get
                {
                    return DepDelayCode3Field;
                }
                set
                {
                    DepDelayCode3Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 9)]
            public string DepDelayTime4_HH
            {
                get
                {
                    return DepDelayTime4_HHField;
                }
                set
                {
                    DepDelayTime4_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 10)]
            public string DepDelayTime4_MM
            {
                get
                {
                    return DepDelayTime4_MMField;
                }
                set
                {
                    DepDelayTime4_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 11)]
            public string DepDelayCode4
            {
                get
                {
                    return DepDelayCode4Field;
                }
                set
                {
                    DepDelayCode4Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 12)]
            public string ArrDelayTime1_HH
            {
                get
                {
                    return ArrDelayTime1_HHField;
                }
                set
                {
                    ArrDelayTime1_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 13)]
            public string ArrDelayTime1_MM
            {
                get
                {
                    return ArrDelayTime1_MMField;
                }
                set
                {
                    ArrDelayTime1_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 14)]
            public string ArrDelayCode1
            {
                get
                {
                    return ArrDelayCode1Field;
                }
                set
                {
                    ArrDelayCode1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 15)]
            public string ArrDelayTime2_HH
            {
                get
                {
                    return ArrDelayTime2_HHField;
                }
                set
                {
                    ArrDelayTime2_HHField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 16)]
            public string ArrDelayTime2_MM
            {
                get
                {
                    return ArrDelayTime2_MMField;
                }
                set
                {
                    ArrDelayTime2_MMField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 17)]
            public string ArrDelayCode2
            {
                get
                {
                    return ArrDelayCode2Field;
                }
                set
                {
                    ArrDelayCode2Field = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSAdditionalInfo", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSAdditionalInfo : object
        {

            private string ExtraFld1Field;

            private string ExtraFld2Field;

            private string RemarksField;

            [System.Runtime.Serialization.DataMember()]
            public string ExtraFld1
            {
                get
                {
                    return ExtraFld1Field;
                }
                set
                {
                    ExtraFld1Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ExtraFld2
            {
                get
                {
                    return ExtraFld2Field;
                }
                set
                {
                    ExtraFld2Field = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Remarks
            {
                get
                {
                    return RemarksField;
                }
                set
                {
                    RemarksField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSLandsTkofRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSLandsTkofRec : object
        {

            private int TKFLND_IDField;

            private bool TKOF_DAYField;

            private bool TKOF_NIGHTField;

            private bool TKOF_MANField;

            private bool TKOF_AUTOField;

            private bool LAND_DAYField;

            private bool LAND_NIGHTField;

            private bool LAND_MANField;

            private bool LAND_AUTOField;

            private bool CAT3_ACTField;

            private bool CAT3_PRACField;

            private bool RHSField;

            [System.Runtime.Serialization.DataMember()]
            public int TKFLND_ID
            {
                get
                {
                    return TKFLND_IDField;
                }
                set
                {
                    TKFLND_IDField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool TKOF_DAY
            {
                get
                {
                    return TKOF_DAYField;
                }
                set
                {
                    TKOF_DAYField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool TKOF_NIGHT
            {
                get
                {
                    return TKOF_NIGHTField;
                }
                set
                {
                    TKOF_NIGHTField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 3)]
            public bool TKOF_MAN
            {
                get
                {
                    return TKOF_MANField;
                }
                set
                {
                    TKOF_MANField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 4)]
            public bool TKOF_AUTO
            {
                get
                {
                    return TKOF_AUTOField;
                }
                set
                {
                    TKOF_AUTOField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 5)]
            public bool LAND_DAY
            {
                get
                {
                    return LAND_DAYField;
                }
                set
                {
                    LAND_DAYField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 6)]
            public bool LAND_NIGHT
            {
                get
                {
                    return LAND_NIGHTField;
                }
                set
                {
                    LAND_NIGHTField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 7)]
            public bool LAND_MAN
            {
                get
                {
                    return LAND_MANField;
                }
                set
                {
                    LAND_MANField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 8)]
            public bool LAND_AUTO
            {
                get
                {
                    return LAND_AUTOField;
                }
                set
                {
                    LAND_AUTOField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 9)]
            public bool CAT3_ACT
            {
                get
                {
                    return CAT3_ACTField;
                }
                set
                {
                    CAT3_ACTField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 10)]
            public bool CAT3_PRAC
            {
                get
                {
                    return CAT3_PRACField;
                }
                set
                {
                    CAT3_PRACField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 11)]
            public bool RHS
            {
                get
                {
                    return RHSField;
                }
                set
                {
                    RHSField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSFacilitiesRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSFacilitiesRec : object
        {

            private string FacilDescField;

            private double? FacilAmountField;

            [System.Runtime.Serialization.DataMember()]
            public string FacilDesc
            {
                get
                {
                    return FacilDescField;
                }
                set
                {
                    FacilDescField = value;
                }
            }

            [System.Runtime.Serialization.DataMember(Order = 1)]
            public double? FacilAmount
            {
                get
                {
                    return FacilAmountField;
                }
                set
                {
                    FacilAmountField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSPasengersRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSPasengersRec : object
        {

            private string PasngrCodeField;

            private short? PasngrNumField;

            [System.Runtime.Serialization.DataMember()]
            public string PasngrCode
            {
                get
                {
                    return PasngrCodeField;
                }
                set
                {
                    PasngrCodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public short? PasngrNum
            {
                get
                {
                    return PasngrNumField;
                }
                set
                {
                    PasngrNumField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSFuelCargoRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSFuelCargoRec : object
        {

            private string FieldDescrField;

            private byte MeasureUnitField;

            private string ValueField;

            [System.Runtime.Serialization.DataMember()]
            public string FieldDescr
            {
                get
                {
                    return FieldDescrField;
                }
                set
                {
                    FieldDescrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public byte MeasureUnit
            {
                get
                {
                    return MeasureUnitField;
                }
                set
                {
                    MeasureUnitField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Value
            {
                get
                {
                    return ValueField;
                }
                set
                {
                    ValueField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSSupplierRec", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSSupplierRec : object
        {

            private string Supl_AgentCompanyField;

            private string Supl_SlipNumberField;

            private int Supl_ValueField;

            [System.Runtime.Serialization.DataMember()]
            public string Supl_AgentCompany
            {
                get
                {
                    return Supl_AgentCompanyField;
                }
                set
                {
                    Supl_AgentCompanyField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Supl_SlipNumber
            {
                get
                {
                    return Supl_SlipNumberField;
                }
                set
                {
                    Supl_SlipNumberField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int Supl_Value
            {
                get
                {
                    return Supl_ValueField;
                }
                set
                {
                    Supl_ValueField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "UpdateVRDataresp", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class UpdateVRDataresp : object
        {

            private string ErrorExplanationField;

            private bool IsUpdatedField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool IsUpdated
            {
                get
                {
                    return IsUpdatedField;
                }
                set
                {
                    IsUpdatedField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "RemoveNotifIndicator", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class RemoveNotifIndicator : object
        {

            private string ErrorExplanationField;

            private bool IndicatorRemovedField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public bool IndicatorRemoved
            {
                get
                {
                    return IndicatorRemovedField;
                }
                set
                {
                    IndicatorRemovedField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "FlightScheduleLogDetails", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class FlightScheduleLogDetails : object
        {

            private string ErrorExplanationField;

            private LegLogChanges[] FlightLegsChangedlistField;

            private int FlightLegsCountField;

            [System.Runtime.Serialization.DataMember()]
            public string ErrorExplanation
            {
                get
                {
                    return ErrorExplanationField;
                }
                set
                {
                    ErrorExplanationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public LegLogChanges[] FlightLegsChangedlist
            {
                get
                {
                    return FlightLegsChangedlistField;
                }
                set
                {
                    FlightLegsChangedlistField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public int FlightLegsCount
            {
                get
                {
                    return FlightLegsCountField;
                }
                set
                {
                    FlightLegsCountField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "LegLogChanges", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class LegLogChanges : object
        {

            private int LegLogChangesCountField;

            private LegLogDetails[] LegLogDetailslistField;

            private string LogCarrierField;

            private string LogDepField;

            private string LogFltField;

            private string LogForDayField;

            private string LogLegCdField;

            [System.Runtime.Serialization.DataMember()]
            public int LegLogChangesCount
            {
                get
                {
                    return LegLogChangesCountField;
                }
                set
                {
                    LegLogChangesCountField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public LegLogDetails[] LegLogDetailslist
            {
                get
                {
                    return LegLogDetailslistField;
                }
                set
                {
                    LegLogDetailslistField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogCarrier
            {
                get
                {
                    return LogCarrierField;
                }
                set
                {
                    LogCarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogDep
            {
                get
                {
                    return LogDepField;
                }
                set
                {
                    LogDepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogFlt
            {
                get
                {
                    return LogFltField;
                }
                set
                {
                    LogFltField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogForDay
            {
                get
                {
                    return LogForDayField;
                }
                set
                {
                    LogForDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogLegCd
            {
                get
                {
                    return LogLegCdField;
                }
                set
                {
                    LogLegCdField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "LegLogDetails", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class LegLogDetails : object
        {

            private string LogChangedByField;

            private string LogDayTimeField;

            private string LogDescriptionField;

            private string LogOnDayField;

            private string LogOnTimeField;

            [System.Runtime.Serialization.DataMember()]
            public string LogChangedBy
            {
                get
                {
                    return LogChangedByField;
                }
                set
                {
                    LogChangedByField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogDayTime
            {
                get
                {
                    return LogDayTimeField;
                }
                set
                {
                    LogDayTimeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogDescription
            {
                get
                {
                    return LogDescriptionField;
                }
                set
                {
                    LogDescriptionField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogOnDay
            {
                get
                {
                    return LogOnDayField;
                }
                set
                {
                    LogOnDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LogOnTime
            {
                get
                {
                    return LogOnTimeField;
                }
                set
                {
                    LogOnTimeField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TFlightKey", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TFlightKey : object
        {

            private string CarrierField;

            private string DayField;

            private string DepField;

            private string FltField;

            private string LegCdField;

            [System.Runtime.Serialization.DataMember()]
            public string Carrier
            {
                get
                {
                    return CarrierField;
                }
                set
                {
                    CarrierField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Day
            {
                get
                {
                    return DayField;
                }
                set
                {
                    DayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Dep
            {
                get
                {
                    return DepField;
                }
                set
                {
                    DepField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Flt
            {
                get
                {
                    return FltField;
                }
                set
                {
                    FltField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string LegCd
            {
                get
                {
                    return LegCdField;
                }
                set
                {
                    LegCdField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.Runtime.Serialization.DataContract(Name = "TAIMSMtceEvent", Namespace = "http://schemas.datacontract.org/2004/07/AIMS.Data.AimsWebService")]
        public partial class TAIMSMtceEvent : object
        {

            private string AcRegField;

            private string ActEndDayField;

            private string ActEndTimeField;

            private string ActStartDayField;

            private string ActStartTimeField;

            private string DescrField;

            private string MemoField;

            private string MtceBaseField;

            private string MtceCodeField;

            private string OperationField;

            private string ScheEndDayField;

            private string ScheEndTimeField;

            private string ScheStartDayField;

            private string ScheStartTimeField;

            private string WorkPageField;

            [System.Runtime.Serialization.DataMember()]
            public string AcReg
            {
                get
                {
                    return AcRegField;
                }
                set
                {
                    AcRegField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ActEndDay
            {
                get
                {
                    return ActEndDayField;
                }
                set
                {
                    ActEndDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ActEndTime
            {
                get
                {
                    return ActEndTimeField;
                }
                set
                {
                    ActEndTimeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ActStartDay
            {
                get
                {
                    return ActStartDayField;
                }
                set
                {
                    ActStartDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ActStartTime
            {
                get
                {
                    return ActStartTimeField;
                }
                set
                {
                    ActStartTimeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Descr
            {
                get
                {
                    return DescrField;
                }
                set
                {
                    DescrField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Memo
            {
                get
                {
                    return MemoField;
                }
                set
                {
                    MemoField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MtceBase
            {
                get
                {
                    return MtceBaseField;
                }
                set
                {
                    MtceBaseField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string MtceCode
            {
                get
                {
                    return MtceCodeField;
                }
                set
                {
                    MtceCodeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string Operation
            {
                get
                {
                    return OperationField;
                }
                set
                {
                    OperationField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ScheEndDay
            {
                get
                {
                    return ScheEndDayField;
                }
                set
                {
                    ScheEndDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ScheEndTime
            {
                get
                {
                    return ScheEndTimeField;
                }
                set
                {
                    ScheEndTimeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ScheStartDay
            {
                get
                {
                    return ScheStartDayField;
                }
                set
                {
                    ScheStartDayField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string ScheStartTime
            {
                get
                {
                    return ScheStartTimeField;
                }
                set
                {
                    ScheStartTimeField = value;
                }
            }

            [System.Runtime.Serialization.DataMember()]
            public string WorkPage
            {
                get
                {
                    return WorkPageField;
                }
                set
                {
                    WorkPageField = value;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.ServiceModel.MessageContract(WrapperName = "ChangeAcRegToRoute", WrapperNamespace = "http://tempuri.org/", IsWrapped = true)]
        public partial class ChangeAcRegToRouteRequest
        {

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 0)]
            public string UN;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 1)]
            public string PSW;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 2)]
            public string AcReg;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 3)]
            public TFlightKey flight;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 4)]
            public string FirstLegOnly;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 5)]
            public string Error;

            public ChangeAcRegToRouteRequest()
            {
            }

            public ChangeAcRegToRouteRequest(string UN, string PSW, string AcReg, TFlightKey flight, string FirstLegOnly, string Error)
            {
                this.UN = UN;
                this.PSW = PSW;
                this.AcReg = AcReg;
                this.flight = flight;
                this.FirstLegOnly = FirstLegOnly;
                this.Error = Error;
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.ServiceModel.MessageContract(WrapperName = "ChangeAcRegToRouteResponse", WrapperNamespace = "http://tempuri.org/", IsWrapped = true)]
        public partial class ChangeAcRegToRouteResponse
        {

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 0)]
            public bool ChangeAcRegToRouteResult;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 1)]
            public string Error;

            public ChangeAcRegToRouteResponse()
            {
            }

            public ChangeAcRegToRouteResponse(bool ChangeAcRegToRouteResult, string Error)
            {
                this.ChangeAcRegToRouteResult = ChangeAcRegToRouteResult;
                this.Error = Error;
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.ServiceModel.MessageContract(WrapperName = "UpdateMtceEvents", WrapperNamespace = "http://tempuri.org/", IsWrapped = true)]
        public partial class UpdateMtceEventsRequest
        {

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 0)]
            public string UN;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 1)]
            public string PSW;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 2)]
            public TAIMSMtceEvent[] Events;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 3)]
            public string Error;

            public UpdateMtceEventsRequest()
            {
            }

            public UpdateMtceEventsRequest(string UN, string PSW, TAIMSMtceEvent[] Events, string Error)
            {
                this.UN = UN;
                this.PSW = PSW;
                this.Events = Events;
                this.Error = Error;
            }
        }

        [System.Diagnostics.DebuggerStepThrough()]
        [System.ServiceModel.MessageContract(WrapperName = "UpdateMtceEventsResponse", WrapperNamespace = "http://tempuri.org/", IsWrapped = true)]
        public partial class UpdateMtceEventsResponse
        {

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 0)]
            public bool UpdateMtceEventsResult;

            [System.ServiceModel.MessageBodyMember(Namespace = "http://tempuri.org/", Order = 1)]
            public string Error;

            public UpdateMtceEventsResponse()
            {
            }

            public UpdateMtceEventsResponse(bool UpdateMtceEventsResult, string Error)
            {
                this.UpdateMtceEventsResult = UpdateMtceEventsResult;
                this.Error = Error;
            }
        }
    }
}