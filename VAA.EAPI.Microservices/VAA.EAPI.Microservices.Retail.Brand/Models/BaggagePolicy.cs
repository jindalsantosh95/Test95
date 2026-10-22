using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Serialization;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class BaggagePolicy : SegmentPolicy
    {
        // *************************************************************** //
        //
        // 2023-07-11
        // ==========
        //
        // The OpenAPI swagger document generator appears to have a limit for the number of levels it will recurse
        // to generate any #ref components. BagProperties seems to be one level too deep so the Swagger documentation then
        // reports an error for missing component references.
        // 
        // This BagProperties is a "hack" that will let the OpenAPI documentation generate the reference, but the property itself
        // will be ignored when serialized back to the consumer.
        // 
        // If a more appropriate approach is subsequently identified, then this should be reviewed
        // 
        // *************************************************************** //
        [JsonIgnore]
        [Description("Do not use")]
        public BagProperties depricated { get; set; }
        // *************************************************************** //


        public FreeBag FreeBags { get; set; }
        public IEnumerable<PaidBag> PaidBags { get; set; }

        public static explicit operator BaggagePolicy(RouteHappy.CheckedBaggage source)
        {
            if (source == null)
                return null;

            return new()
            {
                Assessment = source.Assessment.ToString(),
                Description = source.Description,
                Headline = source.Headline,
                LargeImageLink = source.LargeIconUrl,
                SmallImageLink = source.SmallIconUrl,
                FreeBags = (FreeBag)source.FreeBags,
                PaidBags = source.PaidBags.Select(b => (PaidBag)b),
            };
        }

        public static explicit operator BaggagePolicy(RouteHappy.CarryOnBaggage source)
        {
            if (source == null)
                return null;

            return new()
            {
                Assessment = source.Assessment.ToString(),
                Description = source.Description,
                Headline = source.Headline,
                LargeImageLink = source.LargeIconUrl,
                SmallImageLink = source.SmallIconUrl,
                FreeBags = (FreeBag)source.FreeBags,
            };
        }
    }

    public class FreeBag : Bag
    {
        public int? Pieces { get; set; }

        public static explicit operator FreeBag(RouteHappy.FreeBaggageData source)
        {
            if (source is null)
                return null;

            return new()
            {
                Carrier = source.Attributes.Cxr,
                SeqNo = source.Attributes.SeqNo,
                ServiceType = source.Attributes.ServiceType,
                SubCode = source.Attributes.SubCode,
                DisplayText = source.DisplayText,
                OptionalServiceId = source.OptionalServiceId,
                Pieces = source.Pieces.IsNullOrWhiteSpace() ? null : source.Pieces.Parse<int>(0),
                Size = (BagSize)source.Size,
                Weight = (BagWeight)source.Weight
            };
        }

    }

    public class PaidBag : Bag
    {
        public int Excess_First { get; set; }
        public int Excess_Last { get; set; }
        public Fee Fee { get; set; }
        public int Free { get; set; }
        public string Os_Sequence { get; set; }

        public static explicit operator PaidBag(RouteHappy.PaidBaggageData source)
        {
            if (source is null)
                return null;

            return new()
            {
                Carrier = source.Attributes.Cxr,
                SeqNo = source.Attributes.SeqNo,
                ServiceType = source.Attributes.ServiceType,
                SubCode = source.Attributes.SubCode,
                DisplayText = source.DisplayText,
                OptionalServiceId = source.OptionalServiceId,
                Os_Sequence = source.OsSequence,
                Excess_First = source.ExcessFirst.Parse(0),
                Excess_Last = source.ExcessLast.Parse(0),
                Free = source.Free.Parse(0),
                Fee = (Fee)source.Fee,
                Size = (BagSize)source.Size,
                Weight = (BagWeight)source.Weight

            };

        }
    }

    public class Bag
    {
        public string Carrier { get; set; }
        public int? SeqNo { get; set; }
        public string ServiceType { get; set; }
        public string SubCode { get; set; }
        public string DisplayText { get; set; }
        public string OptionalServiceId { get; set; }
        public BagWeight Weight { get; set; }
        public BagSize Size { get; set; }

    }

    public class BagWeight
    {
        public string Application { get; set; }
        public BagProperties Weight { get; set; }

        public static explicit operator BagWeight(RouteHappy.Weight source)
        {
            if (source is null)
                return null;

            return new()
            {
                Application = source.Application.ToString(),
                Weight = (BagProperties)source
            };
        }
    }

    public class BagSize
    {
        public string Application { get; set; }
        public BagProperties Size { get; set; }

        public static explicit operator BagSize(RouteHappy.Size source)
        {
            if (source == null)
                return null;

            return new()
            {
                Application = source.Application.ToString(),
                Size = (BagProperties)source
            };
        }
    }

    public class BagProperties
    {
        public int Metric { get; set; }
        public int Imperial { get; set; }

        public static explicit operator BagProperties(RouteHappy.Size source)
        {
            if (source == null)
                return null;

            return new()
            {
                Imperial = source._In.Parse(0),
                Metric = source.Cm.Parse(0),
            };
        }

        public static explicit operator BagProperties(RouteHappy.Weight source)
        {
            if (source == null)
                return null;

            return new()
            {
                Imperial = source.Lb.Parse(0),
                Metric = source.Kg.Parse(0),
            };
        }
    }

}
