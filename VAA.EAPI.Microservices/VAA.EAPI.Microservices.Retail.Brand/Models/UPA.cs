using System;
using System.Collections.Generic;
using System.Linq;
//using VAA.EAPI.Microservices.Retail.Brand.Models.RouteHappy;

namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class UPA : SegmentPolicy
    {
        public string AttDescr { get; set; }
        public IEnumerable<string> Categories { get; set; }
        public string CtaText { get; set; }
        public string CtaUrl { get; set; }
        public IEnumerable<Fee> Fees { get; set; }
        public IEnumerable<UPAOsi> OptionalServiceIdentifiers { get; set; }
        public IEnumerable<UPACommon> Photos { get; set; }
        public IEnumerable<UPACommon> Tours { get; set; }
        public IEnumerable<UPACommon> Videos { get; set; }



        public static explicit operator UPA(RouteHappy.Upa source)
        {
            if (source == null)
                return null;

            if (source.Fees.Any())
                throw new NotImplementedException("Fees");            

            return new()
            {
                AttDescr = source.AttDescr,
                Categories = source.Categories,
                CtaText = source.CtaText,
                CtaUrl = source.CtaUrl,
                Description = source.Description,
                Headline = source.Headline,
                LargeImageLink = source.LargeIconUrl,
                OptionalServiceIdentifiers = source.OptionalServiceIdentifiers.Select(osi => (UPAOsi)osi),
                Photos = source.Photos.Select(ph => (UPACommon)ph),
                SmallImageLink = source.SmallIconUrl,
                Fees = source.Fees.Select(f => (Fee)f),
                Tours = source.Tours.Select(f => (UPACommon)f),
                Videos = source.Videos.Select(f => (UPACommon)f),

                // These properties have not yet been fully defined as data sampling does not provide any references to create the models
            };
        }

    }

    public class UPACommon
    {
        public string Caption { get; set; }
        public string LargeUrl { get; set; }
        public string MediumSquareUrl { get; set; }
        public string ThumbUrl { get; set; }
        public string Url { get; set; }
        public string EmbedUrl { get; set; }

        public static explicit operator UPACommon(RouteHappy.UpaCommon source)
        {
            if (source == null)
                return null;

            return new()
            {
                Caption = source.Caption,
                LargeUrl = source.LargeUrl,
                MediumSquareUrl = source.MediumSquareUrl,
                ThumbUrl = source.ThumbUrl,
                Url = source.Url,
                EmbedUrl = source.EmbedUrl
            };
        }
    }

    public class UPAOsi
    {
        public string GroupCode { get; set; }
        public string SubCode { get; set; }
        public string SubGroupCode { get; set; }

        public static explicit operator UPAOsi(RouteHappy.OptionalServiceIdentifier source)
        {
            if (source == null)
                return null;

            return new()
            {
                GroupCode = source.GroupCode,
                SubCode = source.SubCode,
                SubGroupCode = source.SubGroupCode,
            };
        }
    }

    public class UPATour
    {
        // Sample used did not have data to properly reference
    }

    public class UPAVideo
    {
        // Sample used did not have data to properly reference
    }
}
