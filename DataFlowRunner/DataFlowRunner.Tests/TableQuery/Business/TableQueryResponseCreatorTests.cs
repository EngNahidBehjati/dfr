using DataFlowRunner.TableQuery.Business;
using DataFlowRunner.TableQuery.Mappers;
using DataFlowRunner.TableQuery.Models;
using DataFlowRunner.TableQuery.Reports;
using FluentAssertions;

namespace DataFlowRunner.Tests.TableQuery.Business;

public sealed class TableQueryResponseCreatorTests
{
	private readonly TableQueryResponseCreator _sut;

	public TableQueryResponseCreatorTests()
	{
		_sut = new TableQueryResponseCreator(new ReportColumnTypeMapper());
	}

	[Fact]
	public void CreateResponseDto_ShouldThrowArgumentNullException_WhenRowsAreNull()
	{
		// Arrange
		IReadOnlyList<TableQueryReportColumn> columns = new List<TableQueryReportColumn>();
		IReadOnlyList<TableQueryReportRow> rows = null!;

		// Act
		Action act = () => _sut.CreateResponseDto(columns, rows);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void CreateResponseDto_ShouldCreateResponse_WhenReportHasSchemaAndRows()
	{
		// Arrange
		IReadOnlyList<TableQueryReportColumn> columns = new List<TableQueryReportColumn>
		{
			new()
			{
				Name = "RNDMF0ZWDS51MT",
				DisplayName = "Column1",
				DataType = ReportColumnType.Text
			},
			new()
			{
				Name = "RNDLYKZ2JH11ZP",
				DisplayName = "Column2",
				DataType = ReportColumnType.Integer
			}
		};
		IReadOnlyList<TableQueryReportRow> rows = new List<TableQueryReportRow>
		{
			new()
			{
				Cells = new Dictionary<string, object?>
				{
					{ "Column1", "Value1" },
					{ "Column2", 42 }
				}
			}
		};

		var expected = new TableQueryResponseDto
		{
			TableSchema = new List<TableColumnDto>
			{
				new() { Name = "RNDMF0ZWDS51MT", DisplayName = "Column1", DataType = ColumnTypeDto.Text },
				new() { Name = "RNDLYKZ2JH11ZP", DisplayName = "Column2", DataType = ColumnTypeDto.Integer }
			},
			Rows = new List<RowDto>
			{
				new()
				{
					Cells = new Dictionary<string, object?>
					{
						{ "Column1", "Value1" },
						{ "Column2", 42 }
					}
				}
			}
		};

		// Act
		TableQueryResponseDto result = _sut.CreateResponseDto(columns, rows);

		// Assert
		result.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void CreateResponseDto_ShouldCreateResponseWithNullSchema_WhenReportHasNullSchema()
	{
		// Arrange
		IReadOnlyList<TableQueryReportColumn>? columns = null;
		IReadOnlyList<TableQueryReportRow> rows = new List<TableQueryReportRow>
		{
			new()
			{
				Cells = new Dictionary<string, object?>()
			}
		};

		var expected = new TableQueryResponseDto
		{
			TableSchema = null,
			Rows = new List<RowDto> { new() { Cells = new Dictionary<string, object?>() } }
		};

		// Act
		TableQueryResponseDto result = _sut.CreateResponseDto(columns, rows);

		// Assert
		result.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void CreateResponseDto_ShouldCreateTableQueryResponseWithEmptyRows_WhenReportHasEmptyRows()
	{
		// Arrange
		var report = new ExecuteTableQueryReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			PortId = Guid.NewGuid(),
			TableSchema = new List<TableQueryReportColumn>(),
			Rows = new List<TableQueryReportRow>(),
			IsSucceeded = true
		};

		var expected = new TableQueryResponseDto
		{
			TableSchema = new List<TableColumnDto>(),
			Rows = new List<RowDto>()
		};

		// Act
		TableQueryResponseDto result = _sut.CreateResponseDto(report.TableSchema, report.Rows);

		// Assert
		result.Should().BeEquivalentTo(expected);
	}

	[Theory]
	[InlineData(0, 0)]
	[InlineData(1, 1)]
	[InlineData(2, 2)]
	[InlineData(3, 3)]
	[InlineData(4, 4)]
	[InlineData(5, 5)]
	[InlineData(6, 6)]
	[InlineData(7, 7)]
	[InlineData(8, 8)]
	public void CreateResponseDto_ShouldMapAllColumnTypesCorrectly_WhenReportContainsDifferentColumnTypes(
		int reportTypeValue,
		int expectedModelTypeValue)
	{
		// Arrange
		var reportType = (ReportColumnType)reportTypeValue;
		var expectedModelType = (ColumnTypeDto)expectedModelTypeValue;
		IReadOnlyList<TableQueryReportColumn> columns = new List<TableQueryReportColumn>
		{
			new()
			{
				Name = "RNDXOFHER5W5YA",
				DisplayName = "TestColumn",
				DataType = reportType
			}
		};
		IReadOnlyList<TableQueryReportRow> rows = new List<TableQueryReportRow>();

		// Act
		TableQueryResponseDto result = _sut.CreateResponseDto(columns, rows);

		// Assert
		result.TableSchema.Should().NotBeNull();
		result.TableSchema![0].DataType.Should().Be(expectedModelType);
	}
}
