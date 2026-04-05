using MediatR;
using Syncify.Connections.Application.Ports;
using Syncify.Shared;

namespace Syncify.Connections.Application.Queries.GenerateAuthUrl;

public sealed class GenerateAuthUrlQueryHandler(IOAuthProvider oAuthProvider)
    : IRequestHandler<GenerateAuthUrlQuery, Result<string>>
{
    public Task<Result<string>> Handle(GenerateAuthUrlQuery request, CancellationToken ct)
        => Task.FromResult(Result<string>.Success(oAuthProvider.GenerateAuthUrl()));
}