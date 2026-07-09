using System.Text.RegularExpressions;

namespace TaskFlow.Application.DevelopmentLinks;

/// <summary>
/// Extracts TaskFlow task references from free text such as commit messages,
/// pull-request titles and branch names.
/// </summary>
/// <remarks>
/// A task is referenced by its full GUID appearing anywhere in the text, for
/// example a branch named <c>feature/6cc43221-c2ec-4e32-ba25-543095f5b1cf-login</c>
/// or a commit message like <c>Fixes 6cc43221-c2ec-4e32-ba25-543095f5b1cf</c>.
/// </remarks>
public static partial class TaskReferenceExtractor
{
    [GeneratedRegex(
        @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
        RegexOptions.Compiled)]
    private static partial Regex GuidPattern();

    /// <summary>Returns the distinct task ids referenced in the given text (may be empty).</summary>
    public static IReadOnlyList<Guid> Extract(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        var ids = new List<Guid>();
        foreach (Match match in GuidPattern().Matches(text))
        {
            if (Guid.TryParse(match.Value, out var id) && !ids.Contains(id))
                ids.Add(id);
        }

        return ids;
    }
}
