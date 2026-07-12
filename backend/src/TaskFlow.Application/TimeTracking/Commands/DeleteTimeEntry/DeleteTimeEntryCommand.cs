using MediatR;
using TaskFlow.Domain.Common;

namespace TaskFlow.Application.TimeTracking.Commands.DeleteTimeEntry;

/// <summary>Command to delete a time entry. Only the owner may delete it.</summary>
public sealed record DeleteTimeEntryCommand(Guid EntryId, Guid RequesterId) : IRequest<Result>;
