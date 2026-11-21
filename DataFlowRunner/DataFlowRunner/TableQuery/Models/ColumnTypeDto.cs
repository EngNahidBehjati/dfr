namespace DataFlowRunner.TableQuery.Models;

/// <summary>
/// Represents the data type of a table column.
/// Used to define the type of data stored in each column of a table.
/// </summary>
public enum ColumnTypeDto
{
	/// <summary>
	/// Floating-point/decimal numeric data type.
	/// </summary>
	Float = 0,

	/// <summary>
	/// Integer/numeric data type.
	/// </summary>
	Integer = 1,

	/// <summary>
	/// Text/string data type.
	/// </summary>
	Text = 2,

	/// <summary>
	/// Date data type (date only, without time).
	/// </summary>
	Date = 3,

	/// <summary>
	/// Time data type (time only, without date).
	/// </summary>
	Time = 4,

	/// <summary>
	/// Date and time data type.
	/// </summary>
	DateTime = 5,

	/// <summary>
	/// Boolean (true/false) data type.
	/// </summary>
	Boolean = 6,

	/// <summary>
	/// Globally unique identifier (GUID).
	/// </summary>
	Guid = 7,

	/// <summary>
	/// Binary data type (e.g., images, files).
	/// </summary>
	Binary = 8
}
