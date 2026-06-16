namespace TaskFlow.Domain.Common;

/// <summary>Domain errors for board management.</summary>
public static class BoardErrors
{
    /// <summary>Returned when board name is empty or whitespace.</summary>
    public static readonly Error NameRequired = new("Board.NameRequired", "Board name is required.");

    /// <summary>Returned when board name exceeds 100 characters.</summary>
    public static readonly Error NameTooLong = new("Board.NameTooLong", "Board name must not exceed 100 characters.");

    /// <summary>Returned when the requested board does not exist.</summary>
    public static readonly Error NotFound = new("Board.NotFound", "Board was not found.");

    /// <summary>Returned when column name is empty or whitespace.</summary>
    public static readonly Error ColumnNameRequired = new("Board.ColumnNameRequired", "Column name is required.");

    /// <summary>Returned when column name exceeds 100 characters.</summary>
    public static readonly Error ColumnNameTooLong = new("Board.ColumnNameTooLong", "Column name must not exceed 100 characters.");

    /// <summary>Returned when a column with the given id does not exist on the board.</summary>
    public static readonly Error ColumnNotFound = new("Board.ColumnNotFound", "Column was not found.");

    /// <summary>Returned when trying to add a column with an order value already in use.</summary>
    public static readonly Error DuplicateColumnOrder = new("Board.DuplicateColumnOrder", "Column order must be unique within a board.");
}
