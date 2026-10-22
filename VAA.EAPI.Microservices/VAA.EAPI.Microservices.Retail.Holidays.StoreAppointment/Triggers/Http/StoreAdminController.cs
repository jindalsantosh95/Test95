using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.StoreManager;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Triggers.Http;

public class StoreAdminController
{
    private readonly IStoreManagerService _storeManagerService;

    public StoreAdminController(IStoreManagerService storeManagerService)
    {
        _storeManagerService = storeManagerService;
    }

    [Function("GetAdminStores")]
    [OpenApiOperation(operationId: "getAdminStores", tags: new[] { "Store Manage" }, Summary = "List all stores (platform admin only)", Description = "Returns all Holidays stores for platform administration.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StoreResponse[]), Description = "Store list")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    public async Task<HttpResponseData> GetAdminStores([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/admin/stores")] HttpRequestData req)
    {
        return await ServiceResult<IReadOnlyCollection<StoreResponse>>.Success(await _storeManagerService.GetStores()).ExecuteAsync(req);
    }

    [Function("GetAdminStore")]
    [OpenApiOperation(operationId: "getAdminStoreById", tags: new[] { "Store Manage" }, Summary = "Get store by id (platform admin only)", Description = "Returns store details by store ID for platform administration.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StoreResponse), Description = "Store details")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Store not found")]
    public async Task<HttpResponseData> GetAdminStore([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "manage/admin/stores/{id}")] HttpRequestData req, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.GetStore(id);
        return await result.ExecuteAsync(req);
    }

    [Function("UpdateAdminStore")]
    [OpenApiOperation(operationId: "updateAdminStoreById", tags: new[] { "Store Manage" }, Summary = "Update store by id (platform admin only)", Description = "Updates store details by store ID for platform administration.", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "Store ID", Visibility = OpenApiVisibilityType.Important)]
    [OpenApiRequestBody("application/json", typeof(StoreRequest), Required = true, Description = "Store update payload")]
    [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(StoreResponse), Description = "Updated store details")]
    [OpenApiResponseWithoutBody(HttpStatusCode.BadRequest, Description = "Validation failure")]
    [OpenApiResponseWithoutBody(HttpStatusCode.NotFound, Description = "Store not found")]
    [OpenApiResponseWithoutBody(HttpStatusCode.Conflict, Description = "Store update would affect active bookings")]
    public async Task<HttpResponseData> UpdateAdminStore([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "manage/admin/stores/{id}")] HttpRequestData req, string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return await ServiceResult<object>.ValidationError("Invalid store id.").ExecuteAsync(req);
        }

        var result = await _storeManagerService.UpdateStore(id, Json.Deserialize<StoreRequest>(await new StreamReader(req.Body).ReadToEndAsync()));
        return await result.ExecuteAsync(req);
    }
}
