using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Interface;
using VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Model;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories.SCVReferenceDB.Services
{
    public class SCVRefDBService : ISCVRefDBService
    {
        private readonly SCVReferenceDBContext _refDbContext;
        private readonly ILogger<SCVRefDBService> _logger;

        public SCVRefDBService(SCVReferenceDBContext refDbContext, ILogger<SCVRefDBService> logger)
        {
            _refDbContext = refDbContext ?? throw new ArgumentNullException(nameof(refDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<Orders>> GetCustomerOrders(BookingQuery query)
        {
            try
            {
                List<Orders> orders = new();

                string sql = Constants.Application.GetOrderIdQuery;
                List<SqlParameter> sqlParameters = new List<SqlParameter>
                    {
                        new SqlParameter { ParameterName = "@CustomerID", Value = query.FCNumber },
                        new SqlParameter { ParameterName = "@CustomerPhone", Value = query.PhoneNo }

                    };
                orders = await _refDbContext.Orders.FromSqlRaw(sql, sqlParameters.ToArray()).ToListAsync();
                return orders;
            }
            catch (SqlException)
            {
                _logger.LogError("SCV Reference DB customer orders query failed");
                throw;
            }
        }
    }
}
