using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Response;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper.GetBooking
{
    public class CustomerDetailMapper
    {
        public static List<VaaCustomerDetail> MapCustomerDetails(IEnumerable<OrderDetail> orderDetail)
        {
            List<VaaCustomerDetail> customerDetails = new();
            foreach (var order in orderDetail)
            {
                if (order.SegmentDetails != null)
                {
                    foreach (var customer in order.CustomerDetails)
                    {
                        bool checkIfCustomerExists = customerDetails.Any(x => $"{x.FirstName}{" "}{x.LastName}" == $"{customer.FirstName}{" "}{customer.LastName}");
                        if (!checkIfCustomerExists)
                        {
                            customerDetails.Add(new VaaCustomerDetail()
                            {
                                VaaScvId = customer.VAASCVID,
                                BookingType = customer.Bookingtype,
                                ContactTypeText = customer.ContactTypeText,
                                FirstName = customer.FirstName,
                                LastName = customer.LastName,
                                FirstNameNumber = customer.FirstNameNumber,
                                LastNameNumber = customer.LastNameNumber,
                                CustomerId = customer.CustID,
                                Title = customer.Title,
                                FrequentFlyer = new()
                                {
                                    FirstName = customer.FrequentFlyer.FirstName,
                                    AccountNumber = customer.FrequentFlyer.AccountNumber,
                                    AirlineCode = customer.FrequentFlyer.AirlineCode,
                                    LastName = customer.FrequentFlyer.LastName,
                                    OsiNumber = customer.FrequentFlyer.OsiNumber
                                },
                                CustomerContacts = MapContactDetails(customer)
                            });
                        }
                    }
                }
            }
            return customerDetails;
        }

        public static List<CustomerContact> MapContactDetails(CustomerDetail customerDetail)
        {
            List<CustomerContact> customerContacts = new();
            foreach (var contact in customerDetail.CustomerContacts.Contacts)
            {
                customerContacts.Add(new CustomerContact()
                {
                    Countrycode = contact.Countrycode,
                    Email = contact.Email,
                    Phone = contact.Phone
                });
            }

            return customerContacts;
        }
    }
}


