using VAA.EAPI.GraphQL.Domain.Book.Models.Request.Loyalty;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Loyalty;

namespace VAA.EAPI.GraphQL.Domain.Book.Interfaces
{
    public interface ILoyaltySyncServiceProvider
    {
        Task<IEnumerable<PointsCurrencyConverterResponse>> PointsCurrencyConverter(IEnumerable<PointsCurrencyConverterRequest> pointsCurrencyConverterMSRequest);
    }
}