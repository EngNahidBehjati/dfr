namespace DataFlowRunner.TableQuery.Reports;

/// <summary>
/// Represents a single table row in a query table report.
/// Each entry in <see cref="Cells"/> maps a column name to its value,
/// preserving the original data types.
/// </summary>
public sealed record TableQueryReportRow
{
	/// <summary>
	/// Column display name to value mapping for the row.
	/// </summary>
	public required IReadOnlyDictionary<string, object?> Cells { get; init; }
}
