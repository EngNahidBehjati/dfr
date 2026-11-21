namespace DataFlowRunner.TableQuery.Models;

/// <summary>
/// Represents query response containing table data including schema (column definitions) and rows.
/// </summary>
public sealed record TableQueryResponseDto
{
	/// <summary>
	/// Table schema containing column definitions.
	/// Each element represents a column with its name and data type.
	/// </summary>
	public required IReadOnlyList<TableColumnDto>? TableSchema { get; init; }

	/// <summary>
	/// Table rows containing the actual data.
	/// </summary>
	public required IReadOnlyList<RowDto> Rows { get; init; }
}
