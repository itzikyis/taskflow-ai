using MediatR;
using TaskFlow.Domain.Common;
using TaskFlow.Domain.ValueObjects;

namespace TaskFlow.Application.DevelopmentLinks.Commands.LinkDevelopment;

/// <summary>Command to manually link a source-control reference to a task.</summary>
public sealed record LinkDevelopmentCommand(
    Guid TaskId,
    string Repository,
    DevelopmentRefType RefType,
    string Title,
    string Url,
    DevelopmentLinkStatus Status = DevelopmentLinkStatus.None,
    string? ExternalId = null) : IRequest<Result<Guid>>;
