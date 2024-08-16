using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AlphaMDHealth.IntegrationServiceLayer
{
    public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            //if (string.Equals(context.ApiDescription.HttpMethod, HttpMethod.Post.Method, StringComparison.InvariantCultureIgnoreCase))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Device-Unique-ID",
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("6b1e115a-4ca1-4176-b202-9fcc93f08f247971651248708201]")

                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Device-Type",
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("W")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Device-Platform",
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("Web")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Device-Information",
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("Chrome,Chrome,Mac,100.0.4896.127")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "Client-Identifier",
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("PersonalHealthWeb")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Signature",
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("DOnicYQh9RdluDjTIiw7RKxM7u2u5kueoQ+08f2A7aA=")
                });
            }
        }
    }
}