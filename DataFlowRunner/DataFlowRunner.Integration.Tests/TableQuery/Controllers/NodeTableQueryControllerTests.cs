using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.GraphCore.Models.NodeModels;
using DataFlowRunner.Integration.Tests.Common;
using DataFlowRunner.Integration.Tests.Common.Constants;
using DataFlowRunner.Integration.Tests.Common.Wiremock;
using DataFlowRunner.Integration.Tests.TableQuery.Controllers.Assertions;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Models;
using DataFlowRunner.TableQuery.WolverineIntegration.Messages;
using DataFlowRunner.TableQuery.Models;
using DataFlowRunner.TableQuery.Reports;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Wolverine.Tracking;

namespace DataFlowRunner.Integration.Tests.TableQuery.Controllers;

public sealed class NodeTableQueryControllerTests : IClassFixture<AmServerFixture>
{
	private readonly WebApplicationFactory<Program> _factory;
	private readonly HttpClient _httpClient;
	private readonly INodeExecutionStateStore _stateStore;

	public NodeTableQueryControllerTests(AmServerFixture amServerFixture)
	{
		_factory = WebApplicationFactoryCreator.Create(amAddress: amServerFixture.BaseAddress);
		_httpClient = _factory.CreateClient();
		_stateStore = _factory.Services.GetRequiredService<INodeExecutionStateStore>();
		_httpClient.DefaultRequestHeaders.Add("Cookie", AccessManagementTestConstants.AuthCookie);
	}

	[Fact]
	public async Task Query_ShouldReturnTable_Whenever()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		_stateStore.SetNodeExecutionState(new DataFlowExecutionId(dataFlowExecutionId), new NodeId(nodeId), NodeExecutionState.Succeeded);
		const bool includeSchema = true;

		var sampleGuid = Guid.NewGuid().ToString();
		const string sampleDate = "2024-12-31";
		const string sampleTime = "10:30:00";
		const string sampleDateTime = "2024-12-31T10:30:00Z";
		var sampleBytes = Convert.ToBase64String(new byte[] { 1, 2, 3 });

		var cells = new Dictionary<string, object?>
		{
			{ "RNDMF0ZWDS51MT", "Value1" },
			{ "RNDLYKZ2JH11ZP", 42 },
			{ "RNDXOFHER5W5YA", 3.14 },
			{ "RNDTMS25UVZ2N4", sampleDate },
			{ "RNDGZLULTNLCQ3", sampleTime },
			{ "RND3LYWLP3GBZS", sampleDateTime },
			{ "RNDUTN2ZM0ISCG", true },
			{ "RNDFRXFNPOM1A1", sampleGuid },
			{ "RNDMF0ZWDS51M2", sampleBytes },
			{ "RNDLYKZ2JH11Z3", null }
		};

		var reportRows = new List<TableQueryReportRow>
		{
			new()
			{
				Cells = cells
			}
		};

		var expectedRows = new List<RowDto>
		{
			new()
			{
				Cells = cells
			}
		};

		TableQueryResponseDto expectedResponseDto = CreateTableQueryResponseDto(expectedRows);
		Dictionary<string, string> queryParameters = CreateODataQueryParameters();
		ExecuteTableQueryWolverineCommand expectedExecuteTableQueryCommand = CreateExpectedTableQueryCommand(
			dataFlowExecutionId, nodeId, outputPortId, includeSchema, queryParameters);
		ExecuteTableQueryWolverineReport report = CreateTableQueryReport(dataFlowExecutionId, nodeId, outputPortId, reportRows);
		ExecuteTableQueryWolverineCommand? capturedCommand = null;

		_factory.Services.WolverineStubs(stubs =>
		{
			stubs.Stub<ExecuteTableQueryWolverineCommand, ExecuteTableQueryWolverineReport>(command =>
			{
				capturedCommand = command;
				return report;
			});
		});

		var url = BuildQueryUrl(dataFlowExecutionId, nodeId, outputPortId, includeSchema, queryParameters);

		// Act
		using HttpResponseMessage response = await SendQueryRequestAsync(url);

		// Assert
		await TableQueryResponseAssertions.AssertMatchesAsync(response, expectedResponseDto);
		capturedCommand.Should().BeEquivalentTo(expectedExecuteTableQueryCommand);
	}

	private static ExecuteTableQueryWolverineCommand CreateExpectedTableQueryCommand(Guid graphExecutionId,
		Guid nodeId,
		Guid outputPortId,
		bool includeSchema,
		IReadOnlyDictionary<string, string> query)
	{
		return new ExecuteTableQueryWolverineCommand
		{
			GraphExecutionId = graphExecutionId,
			NodeId = nodeId,
			OutputPortId = outputPortId,
			IncludeSchema = includeSchema,
			ODataQuery = query,
			UserId = Guid.Parse(AccessManagementTestConstants.TestRoleId, CultureInfo.InvariantCulture)
		};
	}

	[Fact]
	public async Task Query_ShouldReturnConflict_WhenNodeStateIsFailed()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();

		_stateStore.SetNodeExecutionState(new DataFlowExecutionId(dataFlowExecutionId), new NodeId(nodeId), NodeExecutionState.Failed);
		const bool includeSchema = false;
		Dictionary<string, string> queryParameters = CreateODataQueryParameters();

		var url = BuildQueryUrl(dataFlowExecutionId, nodeId, outputPortId, includeSchema, queryParameters);

		// Act
		using HttpResponseMessage response = await SendQueryRequestAsync(url);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.Conflict);
		ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem.Should().NotBeNull();
		problem.Type.Should().Be("Conflict");
	}

	[Fact]
	public async Task Query_ShouldReturnInternalServerError_WhenReportFails()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		_stateStore.SetNodeExecutionState(new DataFlowExecutionId(dataFlowExecutionId), new NodeId(nodeId), NodeExecutionState.Succeeded);
		const bool includeSchema = false;
		Dictionary<string, string> queryParameters = CreateODataQueryParameters();
		const string errorMessage = "error in table query report";
		const string stackTrace = "Stack trace";
		var failedReport = new ExecuteTableQueryWolverineReport
		{
			GraphExecutionId = dataFlowExecutionId,
			NodeId = nodeId,
			PortId = outputPortId,
			TableSchema = new List<TableQueryReportColumn>(),
			Rows = new List<TableQueryReportRow>(),
			IsSucceeded = false,
			Error = new GraphExecutionError(errorMessage, stackTrace)
		};

		_factory.Services.WolverineStubs(stubs =>
		{
			stubs.Stub<ExecuteTableQueryWolverineCommand, ExecuteTableQueryWolverineReport>(_ => failedReport);
		});

		var url = BuildQueryUrl(dataFlowExecutionId, nodeId, outputPortId, includeSchema, queryParameters);

		// Act
		using HttpResponseMessage response = await SendQueryRequestAsync(url);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
		ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem.Should().NotBeNull();
		problem.Type.Should().Be("TableQueryExecutionFailed");
		problem.Title.Should().Contain(errorMessage);
		problem.Detail.Should().Contain(stackTrace);
	}

	[Fact]
	public async Task Count_ShouldPassQueryParameters_Whenever()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		const long expectedRowCount = 10L;
		_stateStore.SetNodeExecutionState(new DataFlowExecutionId(dataFlowExecutionId), new NodeId(nodeId), NodeExecutionState.Succeeded);

		Dictionary<string, string> queryParameters = CreateODataQueryParameters();
		CalculateRowCountWolverineCommand expectedCalculateRowCountCommand =
			CreateExpectedRowCountQueryCommand(dataFlowExecutionId, nodeId, outputPortId, queryParameters);

		CalculateRowCountWolverineReport report = CreateRowCountQueryReport(dataFlowExecutionId, nodeId, outputPortId, expectedRowCount);
		CalculateRowCountWolverineCommand? capturedCommand = null;

		_factory.Services.WolverineStubs(stubs =>
		{
			stubs.Stub<CalculateRowCountWolverineCommand, CalculateRowCountWolverineReport>(command =>
			{
				capturedCommand = command;
				return report;
			});
		});

		var url = BuildGetRowCountUrl(dataFlowExecutionId, nodeId, outputPortId, queryParameters);

		// Act
		using HttpResponseMessage response = await SendRowCountRequestAsync(url);

		// Assert
		var rowCount = await response.Content.ReadFromJsonAsync<long>(ApiSerializationDefaults.Options);
		response.Should().BeSuccessful();
		rowCount.Should().Be(expectedRowCount);
		capturedCommand.Should().BeEquivalentTo(expectedCalculateRowCountCommand);
	}

	[Fact]
	public async Task Count_ShouldReturnInternalServerError_WhenReportFails()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		_stateStore.SetNodeExecutionState(new DataFlowExecutionId(dataFlowExecutionId), new NodeId(nodeId), NodeExecutionState.Succeeded);
		Dictionary<string, string> queryParameters = CreateODataQueryParameters();
		const string errorMessage = "error in row table query report";
		const string stackTrace = "Stack trace";
		var failedReport = new CalculateRowCountWolverineReport
		{
			GraphExecutionId = dataFlowExecutionId,
			NodeId = nodeId,
			PortId = outputPortId,
			RowCount = 0,
			IsSucceeded = false,
			Error = new GraphExecutionError(errorMessage, stackTrace)
		};

		_factory.Services.WolverineStubs(stubs =>
		{
			stubs.Stub<CalculateRowCountWolverineCommand, CalculateRowCountWolverineReport>(_ => failedReport);
		});

		var url = BuildGetRowCountUrl(dataFlowExecutionId, nodeId, outputPortId, queryParameters);

		// Act
		using HttpResponseMessage response = await SendRowCountRequestAsync(url);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
		ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
		problem.Should().NotBeNull();
		problem.Type.Should().Be("TableQueryExecutionFailed");
		problem.Title.Should().Contain(errorMessage);
		problem.Detail.Should().Contain(stackTrace);
	}

	private static CalculateRowCountWolverineCommand CreateExpectedRowCountQueryCommand(Guid dataFlowExecutionId,
		Guid nodeId,
		Guid outputPortId,
		Dictionary<string, string> query)
	{
		return new CalculateRowCountWolverineCommand
		{
			GraphExecutionId = dataFlowExecutionId,
			NodeId = nodeId,
			OutputPortId = outputPortId,
			ODataQuery = query,
			UserId = Guid.Parse(AccessManagementTestConstants.TestRoleId, CultureInfo.InvariantCulture)
		};
	}

	private static Dictionary<string, string> CreateODataQueryParameters()
	{
		return new Dictionary<string, string>
		{
			{ "$filter", "Column1 eq 'Value1'" },
			{ "$orderby", "Column2 desc" }
		};
	}

	private async Task<HttpResponseMessage> SendQueryRequestAsync(string url)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, url);
		return await _httpClient.SendAsync(request);
	}

	private async Task<HttpResponseMessage> SendRowCountRequestAsync(string url)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, url);
		return await _httpClient.SendAsync(request);
	}

	private static ExecuteTableQueryWolverineReport CreateTableQueryReport(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid portId,
		IReadOnlyList<TableQueryReportRow> rows)
	{
		var tableSchema = new List<TableQueryReportColumn>
		{
			new() { Name = "RNDMF0ZWDS51MT", DisplayName = "TextCol", DataType = ReportColumnType.Text },
			new() { Name = "RNDLYKZ2JH11ZP", DisplayName = "IntCol", DataType = ReportColumnType.Integer },
			new() { Name = "RNDXOFHER5W5YA", DisplayName = "FloatCol", DataType = ReportColumnType.Float },
			new() { Name = "RNDTMS25UVZ2N4", DisplayName = "DateCol", DataType = ReportColumnType.Date },
			new() { Name = "RNDGZLULTNLCQ3", DisplayName = "TimeCol", DataType = ReportColumnType.Time },
			new() { Name = "RND3LYWLP3GBZS", DisplayName = "DateTimeCol", DataType = ReportColumnType.DateTime },
			new() { Name = "RNDUTN2ZM0ISCG", DisplayName = "BoolCol", DataType = ReportColumnType.Boolean },
			new() { Name = "RNDFRXFNPOM1A1", DisplayName = "GuidCol", DataType = ReportColumnType.Guid },
			new() { Name = "RNDMF0ZWDS51M2", DisplayName = "BinaryCol", DataType = ReportColumnType.Binary },
			new() { Name = "RNDLYKZ2JH11Z3", DisplayName = "NullCol", DataType = ReportColumnType.Binary }
		};
		return new ExecuteTableQueryWolverineReport
		{
			GraphExecutionId = dataFlowExecutionId,
			NodeId = nodeId,
			PortId = portId,
			TableSchema = tableSchema,
			Rows = rows,
			IsSucceeded = true
		};
	}

	private static CalculateRowCountWolverineReport CreateRowCountQueryReport(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid portId,
		long rowCount)
	{
		return new CalculateRowCountWolverineReport
		{
			GraphExecutionId = dataFlowExecutionId,
			NodeId = nodeId,
			PortId = portId,
			RowCount = rowCount,
			IsSucceeded = true
		};
	}

	private static TableQueryResponseDto CreateTableQueryResponseDto(
		IReadOnlyList<RowDto> rows)
	{
		var tableSchema = new List<TableColumnDto>
		{
			new() { Name = "RNDMF0ZWDS51MT", DisplayName = "TextCol", DataType = ColumnTypeDto.Text },
			new() { Name = "RNDLYKZ2JH11ZP", DisplayName = "IntCol", DataType = ColumnTypeDto.Integer },
			new() { Name = "RNDXOFHER5W5YA", DisplayName = "FloatCol", DataType = ColumnTypeDto.Float },
			new() { Name = "RNDTMS25UVZ2N4", DisplayName = "DateCol", DataType = ColumnTypeDto.Date },
			new() { Name = "RNDGZLULTNLCQ3", DisplayName = "TimeCol", DataType = ColumnTypeDto.Time },
			new() { Name = "RND3LYWLP3GBZS", DisplayName = "DateTimeCol", DataType = ColumnTypeDto.DateTime },
			new() { Name = "RNDUTN2ZM0ISCG", DisplayName = "BoolCol", DataType = ColumnTypeDto.Boolean },
			new() { Name = "RNDFRXFNPOM1A1", DisplayName = "GuidCol", DataType = ColumnTypeDto.Guid },
			new() { Name = "RNDMF0ZWDS51M2", DisplayName = "BinaryCol", DataType = ColumnTypeDto.Binary },
			new() { Name = "RNDLYKZ2JH11Z3", DisplayName = "NullCol", DataType = ColumnTypeDto.Binary }
		};
		return new TableQueryResponseDto
		{
			TableSchema = tableSchema,
			Rows = rows
		};
	}

	private static string BuildQueryUrl(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid outputPortId,
		bool includeSchema,
		Dictionary<string, string> queryParameters)
	{
		var baseUrl = $"api/data-flow-executions/{dataFlowExecutionId}/nodes/{nodeId}/ports/{outputPortId}/table?includeSchema={includeSchema}";

		var queryString = string.Join("&",
			queryParameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
		return $"{baseUrl}&{queryString}";
	}

	private static string BuildGetRowCountUrl(
		Guid dataFlowExecutionId,
		Guid nodeId,
		Guid outputPortId,
		Dictionary<string, string> queryParameters)
	{
		var baseUrl = $"api/data-flow-executions/{dataFlowExecutionId}/nodes/{nodeId}/ports/{outputPortId}/table/row-count";

		var queryString = string.Join("&",
			queryParameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
		return $"{baseUrl}?{queryString}";
	}
}
