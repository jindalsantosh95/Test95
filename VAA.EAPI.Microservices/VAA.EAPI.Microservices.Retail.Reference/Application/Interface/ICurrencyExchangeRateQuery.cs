using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Interface
{
    public interface ICurrencyExchangeRateQuery
    {
        Task<ExchangeRateResponse> Handle();
    }
}
