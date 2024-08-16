using AlphaMDHealth.Utility;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AlphaMDHealth.ServiceLayer
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (string.Equals(context.ApiDescription.HttpMethod, HttpMethod.Post.Method, StringComparison.InvariantCultureIgnoreCase))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = Constants.SE_DEVICE_UNIQUE_ID_HEADER_KEY,
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("6b1e115a-4ca1-4176-b202-9fcc93f08f247971651248708201]")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = Constants.SE_DEVICE_TYPE_HEADER_KEY,
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("W")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = Constants.SE_DEVICE_PLATFORM_HEADER_KEY,
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("Web")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = Constants.SE_DEVICE_INFORMATION_HEADER_KEY,
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("Chrome,Chrome,Mac,100.0.4896.127")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = Constants.SE_CLIENT_IDENTIFIER_HEADER_KEY,
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("PersonalHealthWeb")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = Constants.SE_HMAC_SIGNATURE_HEADER_KEY,
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("DOnicYQh9RdluDjTIiw7RKxM7u2u5kueoQ+08f2A7aA=")
                });
            }
        }
    }
}