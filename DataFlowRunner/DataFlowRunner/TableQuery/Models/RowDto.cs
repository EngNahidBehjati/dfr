namespace DataFlowRunner.TableQuery.Models;

/// <summary>
/// Represents a single table row.
/// Each entry in <see cref="Cells"/> maps a column name to its value,
/// preserving the original data types.
/// </summary>
public sealed record RowDto
{
	/// <summary>
	/// Column name to value mapping for the row.
	/// </summary>
	public required IReadOnlyDictionary<string, object?> Cells { get; init; }
}
