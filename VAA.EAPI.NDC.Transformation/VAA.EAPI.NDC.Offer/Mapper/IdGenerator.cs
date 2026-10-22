using System;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public static class IdGenerator
    {
        public static string CreateServiceID(int count)
        {
            return string.Format("{0}{1}", Constants.Application.SV, count);
        }
        public static string CreateNewServiceID(int counter)
        {
            Console.WriteLine("Creating Service ID: SVNEW" + counter); 
            return $"SVNEW{counter}";
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
    }
}
