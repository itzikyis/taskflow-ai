using TaskFlow.Application.Search;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Translates a free-text natural-language query into a structured
/// <see cref="TaskSearchFilter"/>. The default implementation is a deterministic
/// keyword parser; it can be replaced by a Claude-backed translator later.
/// </summary>
public interface ITaskSearchInterpreter
{
    /// <summary>Interprets the query into a structured filter.</summary>
    TaskSearchFilter Interpret(string query);

    /// <summary>Builds a short human-readable description of the interpreted filter.</summary>
    string Describe(TaskSearchFilter filter);
}
