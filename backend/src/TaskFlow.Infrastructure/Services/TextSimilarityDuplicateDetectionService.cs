using System.Text.RegularExpressions;
using TaskFlow.Application.DuplicateDetection;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Services;

/// <summary>
/// Duplicate detection based on Jaccard similarity over normalised word sets.
/// Deterministic and dependency-free; can be replaced by an embedding-based
/// implementation without changing callers.
/// </summary>
public sealed partial class TextSimilarityDuplicateDetectionService : IDuplicateTaskDetectionService
{
    // Very common words that carry little signal for duplicate detection.
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "and", "or", "but", "to", "of", "in", "on", "for", "with",
        "is", "are", "be", "as", "at", "by", "it", "this", "that", "we", "our", "add",
        "fix", "update", "task", "new", "make", "should", "when", "from", "into",
    };

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();

    /// <inheritdoc/>
    public IReadOnlyList<DuplicateMatch> FindDuplicates(
        string candidateTitle,
        string? candidateDescription,
        IEnumerable<(Guid Id, string Title, string? Description)> existing,
        double threshold = 0.35)
    {
        var candidateTokens = Tokenize($"{candidateTitle} {candidateDescription}");
        if (candidateTokens.Count == 0)
            return [];

        var matches = new List<DuplicateMatch>();

        foreach (var (id, title, description) in existing)
        {
            var tokens = Tokenize($"{title} {description}");
            if (tokens.Count == 0)
                continue;

            var score = Jaccard(candidateTokens, tokens);
            if (score >= threshold)
                matches.Add(new DuplicateMatch(id, title, Math.Round(score, 3)));
        }

        return matches
            .OrderByDescending(m => m.Score)
            .ToList();
    }

    private static HashSet<string> Tokenize(string text)
    {
        var tokens = NonAlphanumeric().Split(text.ToLowerInvariant());
        return tokens
            .Where(t => t.Length >= 3 && !StopWords.Contains(t))
            .Select(Stem)
            .ToHashSet();
    }

    // Very light stemming: collapse simple plurals so "emails"/"email" and
    // "characters"/"character" match. Avoids pulling in a full stemmer.
    private static string Stem(string token)
    {
        if (token.Length >= 4 && token.EndsWith('s') && !token.EndsWith("ss"))
            return token[..^1];
        return token;
    }

    private static double Jaccard(HashSet<string> a, HashSet<string> b)
    {
        var intersection = a.Count(b.Contains);
        var union = a.Count + b.Count - intersection;
        return union == 0 ? 0 : (double)intersection / union;
    }
}
