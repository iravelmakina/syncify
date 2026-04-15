using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Syncify.Shared.OpenApi;

public sealed class UserIdHeaderTransformer : IOpenApiOperationTransformer
{
    private static readonly string[] SkipPaths = ["health"];

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var path = context.Description.RelativePath;
        if (path != null && SkipPaths.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase)))
        {
            return Task.CompletedTask;
        }

        operation.Parameters ??= new List<IOpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-User-ID",
            In = ParameterLocation.Header,
            Required = true,
            Description = "The User ID (Guid)",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uuid"
            }
        });

        return Task.CompletedTask;
    }
}
