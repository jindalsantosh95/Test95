using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class ManageCustomerRQMapper
    {
        public static List<Customer> CustomerMapper(PNRCommonV6RetrieveResponse pNRCommonV6RetrieveResponse, List<ManageCustomer> customer)
        {
            List<Customer> customers = new();

            var allPassengers = pNRCommonV6RetrieveResponse.PnrType.InfantInArms != null ? pNRCommonV6RetrieveResponse.PnrType.Passengers?.Concat(pNRCommonV6RetrieveResponse.PnrType.InfantInArms)
                : pNRCommonV6RetrieveResponse.PnrType.Passengers;

            customer.ForEach(x =>
              {
                  x.Customer.FirstNameNumber = allPassengers.FirstOrDefault(y => y.FirstName.Is(x.Customer.FirstName) && y.LastName.Replace(Constants.Application.InfIdentificationChar, string.Empty).Is(x.Customer.LastName))?.FirstNameNumber;
                  x.Customer.LastNameNumber = allPassengers.FirstOrDefault(y => y.FirstName.Is(x.Customer.FirstName) && y.LastName.Replace(Constants.Application.InfIdentificationChar, string.Empty).Is(x.Customer.LastName))?.LastNameNumber;
                  customers.Add(x.Customer);
              });

            return customers;
        }
    }
}
