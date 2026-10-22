using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Repository
{
    public class AddressLookUpRepoHandler : IRequestHandler<AddressLookUpRepoCommand, bool>
    {
        private readonly ILogger<AddressLookUpRepoHandler> _logger;
        private readonly IGbgService _gbgService;
        public AddressLookUpRepoHandler(ILogger<AddressLookUpRepoHandler> logger, IGbgService gbgService)
        {
            _logger = logger;
            _gbgService = gbgService;
        }

        public async Task<bool> Handle(AddressLookUpRepoCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            string addressText = $"{request.Street} {request.PostalCode}";

            List<AddressDetail> gbgAddressDetails = new();

            var gbgFindResponse = await _gbgService.Find(addressText, request.Country, new CancellationToken());

            if (gbgFindResponse.Items.Any())
            {
                var retrieveTasks = gbgFindResponse.Items.Where(item => item.Type == "Address")                    
                    .Select(async item =>
                    {
                        var addressResult = await _gbgService.Retrieve(item.Id, new CancellationToken());
                        if (addressResult != null)
                        {
                            return AddressDetailDataMapper.GetAddressDetails(addressResult);
                        }
                        return null;
                    });

                var gbgRetrieveResult = await Task.WhenAll(retrieveTasks);

                gbgAddressDetails.AddRange(gbgRetrieveResult.Where(r => r != null).SelectMany(r => r));
                if (gbgAddressDetails.Any())
                {
                    var address = request.Request.AddressDetails.First(x => x.PreferredFlag.Equals("Y", StringComparison.Ordinal));

                    var addressDetails = gbgAddressDetails.First(addr => addr.PostalCode == address.PostalCode &&  (address.AddressLine1.Contains(addr.AddressLine1) || addr.AddressLine1.Contains(address.AddressLine1)));

                    address.City = addressDetails?.City;
                    address.Region = addressDetails?.Region;
                    address.PostalCode = addressDetails?.PostalCode;
                    address.AddressLine1 = addressDetails?.AddressLine1;
                    address.AddressLine2 = addressDetails?.AddressLine2;

                    address.Valid = "Y";
                    return true;
                }
            }

            request.Request.AddressDetails.First(x => x.PreferredFlag.Is("Y")).Valid = "Y";
            return false;

        }
    }
}
