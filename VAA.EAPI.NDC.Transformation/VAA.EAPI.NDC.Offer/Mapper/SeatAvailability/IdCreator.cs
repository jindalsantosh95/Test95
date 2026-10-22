namespace VAA.EAPI.NDC.Offer.Mapper.SeatAvailability
{
    public static class IdCreator
    {
        public static string CreatePriceClassID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.PC, ++count);
        }
        public static string CreateSeatPaxSegmentID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.SEG, count);
        }
        public static string CreatePaxSegmentID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.SEG, ++count);
        }
        public static string CreateReshopPenaltyID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.PENCHANGE, ++count);
        }
        public static string CreatePaxJourneyID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.JOUR, ++count);
        }

        public static string CreateServiceID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.SV, count);
        }

        public static string CreatePenaltyID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.PEN, count);
        }

        public static string CreateBaggageAllowanceID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.BAGA, count);
        }

        public static string CreateCabinBaggageAllowanceID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.CAB, count);
        }
        public static string CreateOriginDestID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.OD, count);
        }

        public static string CreateDatedMarketingSegmentID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.DATEDMARKETINGSEGMENT, ++count);
        }
        public static string CreateDatedOperatingLegID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.DATEDOPERATINGLEG, ++count);
        }

        public static string CreateDatedOperatingSegmentID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.DATEDMARKETINGREF, ++count);
        }

        public static string CreateSeatDiscloureTypeID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.SeatDisclosure, ++count);
        }
        public static string CreateSeatProfileTypeID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.SeatProfile, ++count);
        }
        public static string CreateServiceDefinitionID(int count)
        {
            return string.Format("{0}{1}{2}", Domain.Model.VAA.Constants.ServiceDefinitionID, "0", ++count);
        }
        public static string CreateServiceRefID(int count)
        {
            return string.Format("{0}{1}{2}", Domain.Model.VAA.Constants.Service, "0", ++count);
        }
        public static string CreateServiceDefinitionRefID(int count)
        {
            return string.Format("{0}{1}", Domain.Model.VAA.Constants.SRV, ++count);
        }
    }
}
