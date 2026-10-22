using System.ComponentModel;

namespace VAA.EAPI.Automation.Core.Helpers.Enums
{
    public enum LocationType
    {
        None,
        AMD,
        LHR,
        BLR,
        GOI,
        ATL,
        DEL,
        AMS,
        LGW,
        MAN,
        JFK,
        BOM,
        LAS,
        LAX,
        ANU,
        BGI,
        GND,
        UVF,
        LON,
        EWR,
        MCO,
        IAD,
        SFO,
        BOS,
        HKG,
        PVG,
        DXB,
        JNB,
        LOS,
        CUN,
        MBJ,
        SIN,
        SYD,
        AKL,
        BJS,
        PEK,
        HAV,
        TAB,
        KOA,
        BFS,
        MEL,
        MIA,
        GLA,
        BNE,
        SEA,
        MEX,
        TLV,
        AVP,
        LUX,
        GRU,
        ABZ,
        CDG,
        EDI,
        DUS,
        BHD,
        SOU,
        EXT,
        NQY,
        NOC,
        JER,
        STR,
        HAJ,
        LYS,
        BHX,
        LCY,
        NCL,
        ORL,
        NYC,
        YVR,
        TLS,
        PAR,
        TPA,
        PDX,
        CMF,
        EGC,
        ISB,
        LHE,
        SVD,
        SJU,
        CPT,
        NAS,
        AUS,
        YYC,
        ICN,
        DUB,
        HNL,
        HAN,
        BEG,
        POS,
        BEY,
        DTW,
        DJT,
        MLE,
        PLS,
        YYZ,
        MAD,
        LIS,
        BER,
        BRU,
        MIL,
        CPH,
        OSL,
        STO,
        SOF,
        ATH,
        ACC,
        ARN,
        LIN,
        JED,
        RUH,
        GVA,
        JAI,
        COK,
        IDR,
        HKT,
        HRE
    }

    public enum JourneyType
    {
        OneWay,
        Return,
        MultiCity,
        OneWayWithStopover,
        ReturnWithStopover,
        OneWayWithConnectingFlight,
        ReturnWithConnectingFlight
    }

    public enum ResponseCode
    {
        OK = 200,
        InternalServerError = 500,
        Unauthorized = 401,
        UnprocessableEntity = 422
    }

    public enum ReservationType
    {
        REVENUE,
        AWARD
    }

    public enum CabinType
    {
        [Description("Economy Light")]
        EconomyLight,

        [Description("Economy Classic")]
        EconomyClassic,

        [Description("Economy Classic Flex")]
        EconomyClassicFlex,

        EconomyDelight,
        EconomyDelightFlex,
        Premium,
        PremiumFlex,
        Upper,
        UpperFlex,

        [Description("Upper Class")]
        UpperClass,

        UpperClassFlex,
        Economy,
        Business,
        DeltaMain,
        DeltaOne,
    }

    public enum FlightScenarioType
    {
        Virgin,
        NonVirgin,
        VirginAndNonVirgin,
        VirginAndNonVirginRequestedOperator,
        NonVirginRequestedOperator,
        NonVirginRequestedWithOtherOperators,
    }

    public enum FlightType
    {
        VS,
        DL,
        AF,
        KL,
        KE,
        WS,
        BW,
        AI,
        SV
    }

    public enum SpecialAssistanceCategory
    {
        WCH,
        DEA,
        BLN
    }

    public enum CabinName
    {
        NONE,
        VSLT,
        VSCL,
        VSDT,
        VSPE,
        VSUP,
        BE,
        MAIN,
        D1,
        DCP,
        DPPS,
        FIRST,
        KLLT,
        KLEC,
        KLPE,
        KLBU,
        AFLT,
        AFST,
        AFPE,
        AFBU,
        FMAIN,
        VSFL,
        VSFD,
        VSFW,
        VSFC
    }

    [Flags]
    public enum FareFamilyType
    {
        [Description("ECONOMY-DELIGHT")]
        ECONOMYDELIGHT = 1,

        [Description("ECON")]
        ECON = 2,

        [Description("ECONOMY-LIGHT")]
        ECONOMYLIGHT = 4,

        [Description("COMFORT-PLUS-PREMIUM-ECON")]
        COMFORTPLUSPREMIUMECON = 8,

        [Description("BUS-FIRST")]
        BUSFIRST = 16,

        [Description("ECON-CLASSIC-FLEXIBLE")]
        ECONCLASSICFLEXIBLE = 32,

        [Description("ECON-DELIGHT-FLEXIBLE")]
        ECONDELIGHTFLEXIBLE = 64,

        [Description("COMFORT-PLUS-PREMIUM-FLEXIBLE")]
        COMFORTPLUSPREMIUMFLEXIBLE = 128,

        [Description("BUS-FIRST-FLEXIBLE")]
        BUSFIRSTFLEXIBLE = 256,

        [Description("AWARD-ECONOMY")]
        AWARDECONOMY = 512,

        [Description("AWARD-COMFORT-PLUS-PREMIUM-ECONOMY")]
        AWARDCOMFORTPLUSPREMIUMECONOMY = 1024,

        [Description("AWARD-BUSINESS-FIRST")]
        AWARDBUSINESSFIRST = 2048,
    }

    public enum FlyingClubMemberType
    {
        Adult,
        Child
    }

    public enum FlyingClubMemberTiers
    {
        Red,
        Silver,
        Gold,
        VAC
    }

    public enum PromoCode
    {
        VESTA,
        GB02,
    }

    public enum Currency
    {
        GBP,
        USD,
        CAD,
        EUR,
        BBD,
        XCD,
        CHF
    }

    public enum CardType
    {
        Visa,
        Mastercard,
        Discover,
        Diners,
        Visa3DS,
        Visa3DSFL,
        Mastercard3DS,
        Discover3DS,
        Diners3DS
    }

    public enum APMType
    {
        PayPal,
        ApplePay,
        GooglePay
    }

    #region MYTRIPS

    public enum ChannelId
    {
        MyTrips,
        DFMOBILE,
        OCI
    }

    public enum AppId
    {
        DC,
        D2
    }

    public enum ErrorScenario
    {
        NullPnr,
        InvalidPnr,
        InvalidName,
        NullTicket,
        InvalidTicket,
        EmptyTripReservationId,
        EmptyloyaltyProgramCode,
        EmptyloyaltyMemberId,
        EmptypassengerId
    }

    public enum DeepLinks
    {
        FlyReady,
        FlightCancel,
        FlightChange,
        TravelInsurance,
        IROP_FAF,
        IROP_ViewDetails,
        POPSMeal,
        BidForUpgrade,
        BaggageAllowance,
        FlightDisruption
    }

    #endregion MYTRIPS

    #region HOLIDAYS

    public enum HolidayBookingItem
    {
        GEN,
        HTL,
        FLT,
        CAR,
        TRS,
        CRU
    }

    #endregion HOLIDAYS

    public enum PassportCountryCode
    {
        GBR,
        USA,
        IND,
        D
    }

    public enum BaggageSelectionType
    {
        FreeOnly,
        PaidOnly,
        FreePlusPaid
    }

    public enum CheckInStatus
    {
        TO_COMPLETE,
        NEEDED_LATER,
        COMPLETE,
        REQUIRED,
        COMPLETED
    }
}
