using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Infrastructure.Controllers.Extensions
{
    public static class RequestExtension
    {
        public static async Task<T> ParseAndValidateRequestAsync<T>(this HttpRequestData req, bool bodyRequired)
        {
            using var reader = new StreamReader(req.Body);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                if (bodyRequired)
                {
                    throw new ValidationException("Request body is required.");
                }

                return default;
            }

            T request;
            try
            {
                request = Json.Deserialize<T>(body);
            }
            catch (JsonException ex)
            {
                throw new ValidationException("Request body is malformed.", ex);
            }

            if (request is null)
            {
                throw new ValidationException("Request body is required.");
            }

            Validator.ValidateObject(request, new ValidationContext(request), true);
            return request;
        }
    }
}
