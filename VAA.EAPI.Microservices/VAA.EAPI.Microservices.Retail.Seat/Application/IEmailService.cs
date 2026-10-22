using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Seat.Domain;
using VAA.EAPI.Microservices.Retail.Seat.Domain.Seats;

namespace VAA.EAPI.Microservices.Retail.Seat.Application
{
    public interface IEmailService
    {
        Task<bool> AddEventForEmailReceiptAsync(OrderEmailRequest request);

    }
}
