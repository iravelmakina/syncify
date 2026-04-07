using MediatR;
using Syncify.Shared.Results;

namespace Syncify.Connections.Application.Queries.GenerateAuthUrl;

public sealed record GenerateAuthUrlQuery : IRequest<Result<string>>;
