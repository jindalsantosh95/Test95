using System.Globalization;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.StoreManager;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Triggers.Http;

public class StoreManagerController
{
    private readonly IStoreManagerService _storeManagerService;

    public StoreManagerController(IStoreManagerService storeManagerService)
    {
        _storeManagerService = storeManagerService;
    }

    [Function("GetStore")]
    [OpenApiOperation(operationId: "getStore", tags: new[] { "Stores" }, Summary = "Get a store by id", Description = "Returns store details by store ID.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StoreResponse), Description = "Store details")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Store not found")]
    public async Task<HttpResponseData> GetStore([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{id}")] HttpRequestData req, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetStore(id);
        return await result.ExecuteAsync(req);
    }

    [Function("UpdateStore")]
    [OpenApiOperation(operationId: "updateStore", tags: new[] { "Stores" }, Summary = "Update a store", Description = "Updates store details by store ID.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(StoreRequest), Required = true, Description = "Store update payload")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StoreResponse), Description = "Updated store details")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Store not found")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Conflict, Description = "Store update would affect active bookings")]
    public async Task<HttpResponseData> UpdateStore([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "stores/{id}")] HttpRequestData req, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.UpdateStore(id, Json.Deserialize<StoreRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await result.ExecuteAsync(req);
    }

    [Function("GetOpeningHours")]
    [OpenApiOperation(operationId: "getOpeningHours", tags: new[] { "Stores" }, Summary = "Get weekly opening hours for a store", Description = "Returns weekly opening hours for a store.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(OpeningHoursEntryResponse[]), Description = "Weekly opening hours")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetOpeningHours([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{id}/opening-hours")] HttpRequestData req, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetOpeningHours(id);
        return await result.ExecuteAsync(req);
    }

    [Function("ReplaceOpeningHours")]
    [OpenApiOperation(operationId: "replaceOpeningHours", tags: new[] { "Stores" }, Summary = "Replace weekly opening hours for a store", Description = "Replaces weekly opening hours for a store.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(OpeningHoursReplaceRequest), Required = true, Description = "Opening hours replacement payload")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(OpeningHoursEntryResponse[]), Description = "Updated weekly opening hours")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Conflict, Description = "Opening-hours update would affect active bookings")]
    public async Task<HttpResponseData> ReplaceOpeningHours([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "stores/{id}/opening-hours")] HttpRequestData req, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.ReplaceOpeningHours(id, Json.Deserialize<OpeningHoursReplaceRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await result.ExecuteAsync(req);
    }

    [Function("GetSlotRules")]
    [OpenApiOperation(operationId: "getSlotRules", tags: new[] { "Store Slots" }, Summary = "Get weekly slot rules for a store", Description = "Returns weekly slot rules for a store.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SlotRuleResponse[]), Description = "Weekly slot rules")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetSlotRules([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/slot-rules")] HttpRequestData req, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetSlotRules(storeId);
        return await result.ExecuteAsync(req);
    }

    [Function("ReplaceSlotRules")]
    [OpenApiOperation(operationId: "replaceSlotRules", tags: new[] { "Store Slots" }, Summary = "Replace weekly slot rules for a store", Description = "Replaces weekly slot rules for a store.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(SlotRuleReplaceRequest), Required = true, Description = "Slot rule replacement payload")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SlotRuleResponse[]), Description = "Updated weekly slot rules")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Conflict, Description = "Slot-rule update would affect active bookings")]
    public async Task<HttpResponseData> ReplaceSlotRules([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "stores/{storeId}/slot-rules")] HttpRequestData req, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.ReplaceSlotRules(storeId, Json.Deserialize<SlotRuleReplaceRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await result.ExecuteAsync(req);
    }

    [Function("GetWindows")]
    [OpenApiOperation(operationId: "getWindows", tags: new[] { "Store Slots" }, Summary = "List slots in a date-time range", Description = "Returns store slots within the requested date-time range.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "from", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Start date-time for the slot range.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "to", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "End date-time for the slot range.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(WindowResponse[]), Description = "Store slots")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetWindows([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/slots")] HttpRequestData req, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        if (!TryGetQueryDateTimeOffset(req, "from", out var from) || !TryGetQueryDateTimeOffset(req, "to", out var to))
        {
            return await ServiceResult<object>.ValidationError("Query parameters 'from' and 'to' are required and must be valid date-times.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetWindows(storeId, from, to);
        return await result.ExecuteAsync(req);
    }

    [Function("UpdateSlotStatus")]
    [OpenApiOperation(operationId: "updateSlotStatus", tags: new[] { "Store Slots" }, Summary = "Update status for a slot", Description = "Updates the status for a store slot.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "slotId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Slot ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(SlotStatusUpdateRequest), Required = true, Description = "Slot status update payload")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(WindowResponse), Description = "Updated slot")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Slot not found")]
    public async Task<HttpResponseData> UpdateSlotStatus([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "stores/{storeId}/slots/{slotId}/status")] HttpRequestData req, string storeId, string slotId)
    {
        if (string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(slotId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id or slot id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.UpdateSlotStatus(storeId, slotId, Json.Deserialize<SlotStatusUpdateRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await result.ExecuteAsync(req);
    }

    [Function("GetSlotAllocations")]
    [OpenApiOperation(operationId: "getSlotAllocations", tags: new[] { "Store Slots" }, Summary = "Get allocation rows for a slot", Description = "Returns per-capacity allocation rows for a store slot on a selected date.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "slotId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Slot ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "startAt", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Selected slot start date-time.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SlotAllocationResponse), Description = "Slot allocations")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Slot not found")]
    public async Task<HttpResponseData> GetSlotAllocations([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/slots/{slotId}/allocations")] HttpRequestData req, string storeId, string slotId)
    {
        if (string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(slotId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id or slot id.").ExecuteAsync(req);
        }

        if (!TryGetQueryDateTimeOffset(req, "startAt", out var startAt))
        {
            return await ServiceResult<object>.ValidationError("Query parameter 'startAt' is required and must be a valid date-time.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetSlotAllocations(storeId, slotId, startAt);
        return await result.ExecuteAsync(req);
    }

    [Function("UpdateSlotAllocationStatus")]
    [OpenApiOperation(operationId: "updateSlotAllocationStatus", tags: new[] { "Store Slots" }, Summary = "Update status for one slot allocation", Description = "Blocks or releases a single allocation row for a store slot.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "slotId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Slot ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "allocationId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Allocation ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(SlotAllocationStatusUpdateRequest), Required = true, Description = "Slot allocation status update payload")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(SlotAllocationResponse), Description = "Updated slot allocations")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Slot not found")]
    public async Task<HttpResponseData> UpdateSlotAllocationStatus([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "stores/{storeId}/slots/{slotId}/allocations/{allocationId}/status")] HttpRequestData req, string storeId, string slotId, string allocationId)
    {
        if (string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(slotId) || string.IsNullOrWhiteSpace(allocationId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id, slot id or allocation id.").ExecuteAsync(req);
        }

        using var reader = new StreamReader(req.Body);
        var requestBody = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return await ServiceResult<object>.ValidationError("Slot allocation status update request is required.").ExecuteAsync(req);
        }

        SlotAllocationStatusUpdateRequest request;
        try
        {
            request = Json.Deserialize<SlotAllocationStatusUpdateRequest>(requestBody);
        }
        catch (JsonException)
        {
            return await ServiceResult<object>.ValidationError("Invalid slot allocation status update request.").ExecuteAsync(req);
        }

        if (request is null)
        {
            return await ServiceResult<object>.ValidationError("Slot allocation status update request is required.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.UpdateSlotAllocationStatus(storeId, slotId, allocationId, request);
        return await result.ExecuteAsync(req);
    }

    [Function("GenerateSlots")]
    [OpenApiOperation(operationId: "generateSlots", tags: new[] { "Store Slots" }, Summary = "Regenerate slots from weekly slot rules", Description = "Regenerates store slots from the configured weekly slot rules.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(WindowResponse[]), Description = "Generated slots")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GenerateSlots([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stores/{storeId}/slots/generate")] HttpRequestData req, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GenerateSlots(storeId);
        return await result.ExecuteAsync(req, HttpStatusCode.Created);
    }

    [Function("GetAvailability")]
    [OpenApiOperation(operationId: "getAvailability", tags: new[] { "Store Slots" }, Summary = "Get available slots for a date", Description = "Returns available slots for a store on the requested date.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "date", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Availability date in yyyy-MM-dd format.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AvailabilityWindowResponse[]), Description = "Available slots")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetAvailability([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/availability")] HttpRequestData req, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        if (!TryGetQueryDate(req, "date", out var date))
        {
            return await ServiceResult<object>.ValidationError("Query parameter 'date' is required and must be a valid date.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetAvailability(storeId, date);
        return await result.ExecuteAsync(req);
    }

    [Function("GetAvailabilityRange")]
    [OpenApiOperation(operationId: "getAvailabilityRange", tags: new[] { "Store Slots" }, Summary = "Get available slots for a date range", Description = "Returns available slots for a store in the requested date range.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "from", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Start date in yyyy-MM-dd format.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "to", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "End date in yyyy-MM-dd format.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(AvailabilityWindowResponse[]), Description = "Available slots")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetAvailabilityRange([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/availability/range")] HttpRequestData req, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        if (!TryGetQueryDate(req, "from", out var from) || !TryGetQueryDate(req, "to", out var to))
        {
            return await ServiceResult<object>.ValidationError("Query parameters 'from' and 'to' are required and must be valid dates.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetAvailability(storeId, from, to);
        return await result.ExecuteAsync(req);
    }

    [Function("GetClosures")]
    [OpenApiOperation(operationId: "getClosures", tags: new[] { "Store Closures" }, Summary = "List store closures in a date range", Description = "Returns store closures in the requested date range.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "from", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Start date in yyyy-MM-dd format.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "to", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "End date in yyyy-MM-dd format.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StoreClosureResponse[]), Description = "Store closures")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetClosures([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/closures")] HttpRequestData req, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        if (!TryGetQueryDate(req, "from", out var from) || !TryGetQueryDate(req, "to", out var to))
        {
            return await ServiceResult<object>.ValidationError("Query parameters 'from' and 'to' are required and must be valid dates.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetClosures(storeId, from, to);
        return await result.ExecuteAsync(req);
    }

    [Function("CreateClosure")]
    [OpenApiOperation(operationId: "createClosure", tags: new[] { "Store Closures" }, Summary = "Create a store closure", Description = "Creates a closure for a store.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(StoreClosureRequest), Required = true, Description = "Store closure payload")]
    [OpenApiResponseWithBody(HttpStatusCode.Created, "application/json", typeof(StoreClosureResponse), Description = "Created store closure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Conflict, Description = "Store closure cannot be created")]
    public async Task<HttpResponseData> CreateClosure([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stores/{storeId}/closures")] HttpRequestData req, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.CreateClosure(storeId, Json.Deserialize<StoreClosureRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await result.ExecuteAsync(req, HttpStatusCode.Created);
    }

    [Function("DeleteClosure")]
    [OpenApiOperation(operationId: "deleteClosure", tags: new[] { "Store Closures" }, Summary = "Delete a store closure", Description = "Deletes a closure for a store.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "closureId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Closure ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithoutBody(HttpStatusCode.NoContent, Description = "Store closure deleted")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Store closure not found")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Conflict, Description = "Store closure cannot be deleted")]
    public async Task<HttpResponseData> DeleteClosure([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "stores/{storeId}/closures/{closureId}")] HttpRequestData req, string storeId, string closureId)
    {
        if (string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(closureId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id or closure id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.DeleteClosure(storeId, closureId);
        return await result.ExecuteDeleteAsync(req);
    }

    [Function("GetBookings")]
    [OpenApiOperation(operationId: "getBookings", tags: new[] { "Store Bookings" }, Summary = "List bookings for a store on a date", Description = "Returns bookings for a store on the requested date, optionally filtered by status or query text.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "date", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Booking date in yyyy-MM-dd format.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "status", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Optional booking status filter.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "q", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "Optional booking search text.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ManageBookingResponse[]), Description = "Store bookings")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetBookings([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "stores/{storeId}/bookings")] HttpRequestData req, string storeId)
    {
        if (string.IsNullOrWhiteSpace(storeId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        if (!TryGetQueryDate(req, "date", out var date))
        {
            return await ServiceResult<object>.ValidationError("Query parameter 'date' is required and must be a valid date.").ExecuteAsync(req);
        }

        var status = GetQueryValue(req, "status");
        var q = GetQueryValue(req, "q");

        if (!string.IsNullOrWhiteSpace(status) && status.Length > 32)
        {
            return await ServiceResult<object>.ValidationError("Query parameter 'status' must be 32 characters or less.").ExecuteAsync(req);
        }

        if (!string.IsNullOrWhiteSpace(q) && q.Length > 200)
        {
            return await ServiceResult<object>.ValidationError("Query parameter 'q' must be 200 characters or less.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetBookings(storeId, date, status, q);
        return await result.ExecuteAsync(req);
    }

    [Function("CancelBooking")]
    [OpenApiOperation(operationId: "cancelBooking", tags: new[] { "Store Bookings" }, Summary = "Cancel a store appointment", Description = "Soft-cancels a booked appointment after customer contact has been confirmed.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "storeId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "bookingId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Appointment booking ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(BookingCancellationRequest), Required = true, Description = "Cancellation audit payload")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ManageBookingResponse), Description = "Cancelled booking")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Booking not found for store")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Conflict, Description = "Booking cannot be cancelled")]
    public async Task<HttpResponseData> CancelBooking(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "stores/{storeId}/bookings/{bookingId}/cancel")] HttpRequestData req,
        string storeId,
        string bookingId)
    {
        if (string.IsNullOrWhiteSpace(storeId) || string.IsNullOrWhiteSpace(bookingId))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id or booking id.").ExecuteAsync(req);
        }

        using var reader = new StreamReader(req.Body);
        var requestBody = await reader.ReadToEndAsync();
        if (string.IsNullOrWhiteSpace(requestBody))
        {
            return await ServiceResult<object>.ValidationError("Cancellation request is required.").ExecuteAsync(req);
        }

        BookingCancellationRequest request;
        try
        {
            request = Json.Deserialize<BookingCancellationRequest>(requestBody);
        }
        catch (JsonException)
        {
            return await ServiceResult<object>.ValidationError("Invalid cancellation request.").ExecuteAsync(req);
        }

        if (request is null)
        {
            return await ServiceResult<object>.ValidationError("Cancellation request is required.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.CancelBooking(storeId, bookingId, request);
        return await result.ExecuteAsync(req);
    }


    private static readonly string[] DateTimeOffsetFormats =
    {
        "O",
        "yyyy-MM-ddTHH:mm:ssK",
        "yyyy-MM-ddTHH:mm:ss.FFFK",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"
    };

    private static bool TryGetQueryDate(HttpRequestData request, string key, out DateOnly parsedDate)
    {
        parsedDate = default;
        return GetQueryValues(request).TryGetValue(key, out var value)
            && DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate);
    }

    private static bool TryGetQueryDateTimeOffset(HttpRequestData request, string key, out DateTimeOffset parsedDateTime)
    {
        parsedDateTime = default;
        return GetQueryValues(request).TryGetValue(key, out var value)
            && DateTimeOffset.TryParseExact(value, DateTimeOffsetFormats, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out parsedDateTime);
    }

    private static string? GetQueryValue(HttpRequestData request, string key)
    {
        return GetQueryValues(request).TryGetValue(key, out var value) ? value : null;
    }

    private static IReadOnlyDictionary<string, string> GetQueryValues(HttpRequestData request)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var query = request.Url.Query;
        if (string.IsNullOrWhiteSpace(query))
        {
            return values;
        }

        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            values[key] = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
        }

        return values;
    }
}
