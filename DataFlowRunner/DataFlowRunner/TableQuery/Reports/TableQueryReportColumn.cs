namespace DataFlowRunner.TableQuery.Reports;

/// <summary>
/// Represents a column definition in a query table report schema.
/// Contains metadata about a single column including its name, display name and data type.
/// </summary>
public sealed record TableQueryReportColumn
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
	public required ReportColumnType DataType { get; init; }
}
