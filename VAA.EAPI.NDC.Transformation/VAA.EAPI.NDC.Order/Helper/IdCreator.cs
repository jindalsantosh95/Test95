namespace VAA.EAPI.NDC.Order.Helper
{
    public class IdCreator
    {
        public static string CreateContactInfoId(int count)
        {
            return string.Format("{0}{1}", Constants.Application.CI, count);
        }

        public static string CreatePaxSegmentID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SEG, ++count);
        }

        public static string CreateServiceID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SV, count);
        }

        public static string CreateOriginDestID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.OD, count);
        }
        public static string CreatePaxJourneyID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.JOUR, ++count);
        }
        public static string CreateBaggageAllowanceID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.BAGA, count);
        }
        public static string CreateCabinBaggageAllowanceID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.CAB, count);
        }

        public static string CreateDatedMarketingSegmentRefId(int count)
        {
            return string.Format("{0}{1}", Constants.Application.DATAMARKETINGSEGMENT, ++count);
        }

        public static string CreateDatedMarketingSegmentID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.DATAMARKETINGSEGMENT, ++count);
        }
        public static string CreateDatedMarketingRefID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.DATAMARKETINGREF, ++count);
        }

        public static string CreateDatedOperatingSegmentID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.DATAMARKETINGREF, ++count);
        }

        public static string CreateServiceDefID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SERVICEREF, ++count);
        }

        public static string CreateServiceMealID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.Meal, ++count);
        }

        public static string CreateServiceDisabilityID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.Disability, ++count);
        }

        public static string CreateDatedOperatingLegID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.DATEDOPERATINGLEGREF, ++count);
        }
    }
}
