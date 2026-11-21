namespace DataFlowRunner.TableQuery.Models;

/// <summary>
/// Represents a column definition in a table schema.
/// Contains metadata about a single column including its name and data type.
/// </summary>
public sealed record TableColumnDto
{
	/// <summary>
	/// Column identifier name.
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// Column display name.
	/// </summary>
	public required string DisplayName { get; init; }

	/// <summary>
	/// Data type of the column.
	/// </summary>
	public required ColumnTypeDto DataType { get; init; }
}
