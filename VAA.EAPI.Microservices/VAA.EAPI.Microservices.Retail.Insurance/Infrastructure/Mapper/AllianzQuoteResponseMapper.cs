using VAA.EAPI.Microservices.Retail.Insurance.Domain;
using VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Proxy.AllianzQuoteResponse;

namespace VAA.EAPI.Microservices.Retail.Insurance.Infrastructure.Mapper
{
    public class AllianzQuoteResponseMapper
    {
        public static InsuranceQuoteResponse? MapInsuranceQuoteResponse(QuoteResponse quoteResponse, InsuranceQuoteRequest insuranceQuoteRequest)
        {
            if (quoteResponse is not null)
                return new InsuranceQuoteResponse()
                {
                    Status = new()
                    {
                        Code = quoteResponse.Status.Code,
                        ProcessId = quoteResponse.Status.ProcessId,
                        CreationDate = quoteResponse.Status.CreationDate,
                        Message = quoteResponse.Status?.Message,
                        Version = quoteResponse.Status.Version,
                        ErrorText = quoteResponse.Status?.ErrorText,
                        Reason = quoteResponse.Status?.Reason,

                    },
                    Products = quoteResponse.Products?.Where(product => product.ProductCode != "NoProduct").Select(p => new Domain.Product()
                    {
                        ExternalCode = p.ExternalCode,
                        ProductCode = p.ProductCode,
                        OfferId = p.OfferId,
                        IndividualPrices = p.Individualprices?.Select(i => new Domain.IndividualPrice()
                        {
                            AdminFee = i.AdminFee,
                            Price = i.Price,
                            Sequence  = i.Sequence
                        }),
                        GroupPrices = p.Groupprices?.Select(p => new Domain.GroupPrice()
                        { 
                            AdminFee = p.AdminFee,
                            Price = p.Price,
                            Sequence = p.Sequence,
                            SequenceName = p.SequenceName
                        }),
                        IsAvailable = p.IsAvailable,
                        ProductName = p.ProductName,
                        QuoteId = p.QuoteId,
                        SupplierCode = p.SupplierCode,
                        ProductType = p.ProductType,
                        ProductSpecificMessages = p.Productspecificmessage?.Select(p =>new Domain.ProductSpecificMessage()
                        {
                            Type = p.Type,
                            Text = p.Text,
                            Uri = p.Uri
                        }),
                        PricingModelCode = p.PricingModelCode,
                        Prices = p.Prices.Select(price => new PriceDetails()
                        {
                            IsAvailable = price.IsAvailable,
                            IsIncluded = price.IsIncluded,
                            Price = price.Price,
                            Sequences = price.Sequences?.Select(s => new Domain.Sequence()
                            {
                                Name = s.Name,
                                Value  = s.Value
                            })
                        }),
                        TotalPrice = p.TotalPrice,
                        PricingModelMinAmount = p.PricingModelMinAmount,
                        PricingModelDescription = p.PricingModelDescription,
                        PricingModelMaxAmount = p.PricingModelMaxAmount,
                    }),
                    TransactionDetails = new TransactionDetails()
                    {
                        ControlType = quoteResponse.TransactionDetails.ControlType,
                        Cxid = quoteResponse.TransactionDetails.Cxid,
                        PopulationCode = quoteResponse.TransactionDetails.PopulationCode,   
                        ResponseId = quoteResponse.TransactionDetails.ResponseId,
                        SegmentCode = quoteResponse.TransactionDetails.SegmentCode, 
                        SegmentName = quoteResponse.TransactionDetails.SegmentName,
                        TreatmentId = quoteResponse.TransactionDetails.TreatmentId,
                        TreeCode = quoteResponse.TransactionDetails.TreeCode
                    },
                    Locations = quoteResponse.Locations?.Select(l => new Domain.Location()
                    {
                        AutoRender = l.AutoRender,
                        LocationCode = l.LocationCode,
                        LocationType = l.LocationType,
                        Output  = l.Output,
                        Selector = l.Selector,
                        OutputType = l.OutputType,
                        Status = new Domain.Status() { 
                            Code = l.Status.Code,
                            Message = l.Status.Message,
                            ErrorText = l.Status.ErrorText,
                            Reason = l.Status.Reason
                        },
                        TemplateCode = l.TemplateCode,
                        TemplateType = l.TemplateType,
                        TreatmentCode = l.TreatmentCode,
                        Offers = l.Offers.Select(o => new Domain.Offer()
                        {
                            ContentCode = o.ContentCode,
                            ContentType = o.ContentType,
                            Id = o.Id,
                            Sequence = o.Sequence,
                            Messages = o.Messages?.Select(m =>  new Domain.Message()
                            {
                                CompiledMessage = m.CompiledMessage,
                                MessageSequence = m.MessageSequence,
                                MessageTag = m.MessageTag
                            })
                        })
                    })
                };

            else
                return null;
        }

    }
}