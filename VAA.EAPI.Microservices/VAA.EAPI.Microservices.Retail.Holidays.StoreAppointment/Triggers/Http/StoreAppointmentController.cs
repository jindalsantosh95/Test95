using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Controllers.Extensions;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Triggers.Http;

public class StoreAppointmentController
{
    private readonly IStoreAppointmentHandler _appointmentHandler;
    private readonly IStoreHandler _storeLocatorHandler;

    public StoreAppointmentController(IStoreAppointmentHandler appointmentHandler, IStoreHandler storeLocatorHandler)
    {
        _appointmentHandler = appointmentHandler;
        _storeLocatorHandler = storeLocatorHandler;
    }

    [Function("CreateStoreAppointment")]
    [OpenApiOperation(operationId: "CreateStoreAppointment", tags: new[] { "StoreAppointment" }, Summary = "Create a store appointment", Description = "Create a Holidays store appointment.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(AppointmentBookingRequest), Required = true, Description = "Store appointment payload")]
    [OpenApiResponseWithBody(HttpStatusCode.Accepted, "application/json", typeof(AppointmentBookingResponse), Description = "Appointment accepted")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> CreateStoreAppointment([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Holidays/Storeappointments")] HttpRequestData req)
    {
        var result = await _appointmentHandler.SaveAppointmentAsync(Json.Deserialize<AppointmentBookingRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await ResultExtention<AppointmentBookingResponse>.ExecuteResultAsync(req, result);
    }

    [Function("GetNearbyStores")]
    [OpenApiOperation(operationId: "GetNearbyStores", tags: new[] { "StoreLocator" }, Summary = "Get nearby stores", Description = "Returns nearby stores with Google Distance Matrix driving time and distance.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(StoreSearchRequest), Required = true, Description = "Store search by postcode/city or coordinates")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(NearbyStoreResult), Description = "Nearby stores")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetNearbyStores([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Stores/GetAllStores")] HttpRequestData req)
    {
        var result = await _storeLocatorHandler.GetNearByStoresAsync(Json.Deserialize<StoreSearchRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await ResultExtention<NearbyStoreResult>.ExecuteResultAsync(req, result);
    }

    [Function("GetStoreById")]
    [OpenApiOperation(operationId: "GetStoreById", tags: new[] { "StoreLocator" }, Summary = "Get store by id", Description = "Returns store details by store ID with Google Distance Matrix driving time and distance.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(StoreSearchRequest), Required = true, Description = "Store search by storeId and origin")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StoreResult), Description = "Store details")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetStoreById([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Stores/GetStoresById")] HttpRequestData req)
    {
        var result = await _storeLocatorHandler.GetStoreByIdAsync(Json.Deserialize<StoreSearchRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await ResultExtention<StoreResult>.ExecuteResultAsync(req, result);
    }

    [Function("GetStoreSlotsByDate")]
    [OpenApiOperation(operationId: "GetStoreSlotsByDate", tags: new[] { "StoreLocator" }, Summary = "Get store slots by date", Description = "Returns one-hour slots for a selected date including availability for the requested store.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(StoreSearchRequest), Required = true, Description = "Store search by storeId and selectedDate")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StoreSlotsResult), Description = "Store slots")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetStoreSlotsByDate([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Stores/GetSlotsByDate")] HttpRequestData req)
    {
        var result = await _storeLocatorHandler.GetStoreSlotsByDateAsync(Json.Deserialize<StoreSearchRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await ResultExtention<StoreSlotsResult>.ExecuteResultAsync(req, result);
    }

    [Function("GetStoreCalendarAvailability")]
    [OpenApiOperation(operationId: "GetStoreCalendarAvailability", tags: new[] { "StoreLocator" }, Summary = "Get store calendar availability", Description = "Returns day-level slot availability for a selected month for present and future dates.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(StoreSearchRequest), Required = true, Description = "Store search by storeId, year and month")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StoreCalendarAvailabilityResult), Description = "Store calendar day availability")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetStoreCalendarAvailability([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Stores/GetCalendarAvailability")] HttpRequestData req)
    {
        var result = await _storeLocatorHandler.GetStoreCalendarAvailabilityAsync(Json.Deserialize<StoreSearchRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await ResultExtention<StoreCalendarAvailabilityResult>.ExecuteResultAsync(req, result);
    }
}