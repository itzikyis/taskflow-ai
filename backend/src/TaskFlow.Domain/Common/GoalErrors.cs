namespace TaskFlow.Domain.Common;

/// <summary>Domain errors for the Goals / OKR feature.</summary>
public static class GoalErrors
{
    /// <summary>Returned when a goal title is missing or blank.</summary>
    public static readonly Error TitleRequired = new("Goal.TitleRequired", "Goal title is required.");

    /// <summary>Returned when a goal title exceeds the maximum length.</summary>
    public static readonly Error TitleTooLong = new("Goal.TitleTooLong", "Goal title must not exceed 200 characters.");

    /// <summary>Returned when a requested goal cannot be found.</summary>
    public static readonly Error NotFound = new("Goal.NotFound", "The requested goal was not found.");

    /// <summary>Returned when a progress percentage is out of range.</summary>
    public static readonly Error InvalidProgress = new("Goal.InvalidProgress", "Progress percent must be between 0 and 100.");

    /// <summary>Returned when a key result title is missing or blank.</summary>
    public static readonly Error KeyResultTitleRequired = new("Goal.KeyResultTitleRequired", "Key result title is required.");
}
