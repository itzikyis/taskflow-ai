using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.DevelopmentLinks.Commands.RemoveDevelopmentLink;

/// <summary>Command to remove a development link.</summary>
public sealed record RemoveDevelopmentLinkCommand(Guid LinkId) : IRequest<Result>;
