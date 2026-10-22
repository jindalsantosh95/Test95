namespace VAA.EAPI.NDC.Offer.Infrastructure.Mapper
{
    public static class IdCreator
    {
        public static string CreateServiceID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SV, count);
        }

        public static string CreateNewServiceID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SVNEW, count);
        }

        public static string CreatePriceClassID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.PC, ++count);
        }

        public static string CreatePaxSegmentID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SEG, ++count);
        }

        public static string CreateDatedMarketingSegmentID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.DATEDMARKETINGSEGMENT, ++count);
        }

        public static string CreateDatedOperatingSegmentID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.DATEDMARKETINGREF, ++count);
        }

        public static string CreateSeatPaxSegmentID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SEG, count);
        }

        public static string CreateReshopPenaltyID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.PENCHANGE, ++count);
        }

        public static string CreatePaxJourneyID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.JOUR, ++count);
        }

        public static string CreatePenaltyID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.PEN, count);
        }

        public static string CreateBaggageAllowanceID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.BAGA, count);
        }

        public static string CreateCabinBaggageAllowanceID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.CAB, count);
        }

        public static string CreateOriginDestID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.OD, count);
        }

        public static string CreateDatedOperatingLegID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.DATEDOPERATINGLEG, ++count);
        }

        public static string CreateSeatDiscloureTypeID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SeatDisclosure, ++count);
        }

        public static string CreateSeatProfileTypeID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SeatProfile, ++count);
        }

        public static string CreateServiceDefinitionID(int count)
        {
            return string.Format("{0}{1}{2}", Constants.Application.ServiceDefinitionID, "0", ++count);
        }

        public static string CreateServiceRefID(int count)
        {
            return string.Format("{0}{1}{2}", Constants.Application.Service, "0", ++count);
        }

        public static string CreateServiceDefinitionRefID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SRV, ++count);
        }
        public static string CreateNewServiceDefinitionRefID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SRVNEW, ++count);
        }
    }
}
