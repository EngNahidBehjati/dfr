using DataFlowRunner.TableQuery.Abstractions;
using DataFlowRunner.TableQuery.Controllers;
using DataFlowRunner.TableQuery.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using NSubstitute;

namespace DataFlowRunner.Tests.TableQuery.Controllers;

public sealed class NodeTableQueryControllerTests
{
	private readonly ITableQueryFacade _tableQueryFacade;
	private readonly IQueryParametersExtractor _queryParametersExtractor;
	private readonly NodeTableQueryController _sut;

	public NodeTableQueryControllerTests()
	{
		_tableQueryFacade = Substitute.For<ITableQueryFacade>();
		_queryParametersExtractor = Substitute.For<IQueryParametersExtractor>();
		_sut = new NodeTableQueryController(_tableQueryFacade, _queryParametersExtractor);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenTableQueryFacadeIsNull()
	{
		// Arrange
		ITableQueryFacade tableQueryFacade = null!;

		// Act
		Action act = () =>
		{
			_ = new NodeTableQueryController(tableQueryFacade, _queryParametersExtractor);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("tableQueryFacade");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenQueryParametersExtractorIsNull()
	{
		// Arrange
		IQueryParametersExtractor queryParametersExtractor = null!;

		// Act
		Action act = () =>
		{
			_ = new NodeTableQueryController(_tableQueryFacade, queryParametersExtractor);
		};

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("queryParametersExtractor");
	}

	[Fact]
	public async Task Query_ShouldQueryTableWithExtractedQueryParameters_Whenever()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		const bool includeSchema = true;

		var queryCollection = new QueryCollection(new Dictionary<string, StringValues>());
		SetupControllerContext(queryCollection);
		var extractedParameters = new Dictionary<string, string> { { "filter", "test" }, { "page", "1" } };
		_queryParametersExtractor.GetParameters(queryCollection).Returns(extractedParameters);

		var expectedResponseDto = new TableQueryResponseDto
		{
			TableSchema = new List<TableColumnDto>(),
			Rows = new List<RowDto>()
		};

		_tableQueryFacade.QueryTableAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			includeSchema,
			extractedParameters,
			CancellationToken.None)
			.Returns(Task.FromResult(expectedResponseDto));

		// Act
		TableQueryResponseDto actual = await _sut.Query(
			dataFlowExecutionId, nodeId, outputPortId, includeSchema, CancellationToken.None);

		// Assert
		actual.Should().BeSameAs(expectedResponseDto);
	}

	[Fact]
	public async Task Count_ShouldCountRowsWithExtractedParameters_Whenever()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		var outputPortId = Guid.NewGuid();
		CancellationToken cancellationToken = CancellationToken.None;
		const long expectedRowCount = 10L;

		var queryCollection = new QueryCollection(new Dictionary<string, StringValues>
		{
			{ "includeSchema", "true" },
			{ "filter", "test" }
		});
		SetupControllerContext(queryCollection);
		var extractedParameters = new Dictionary<string, string> { { "filter", "test" } };
		_queryParametersExtractor.GetParameters(queryCollection).Returns(extractedParameters);

		_tableQueryFacade.GetRowCountAsync(
			dataFlowExecutionId,
			nodeId,
			outputPortId,
			extractedParameters,
			cancellationToken)
			.Returns(Task.FromResult(expectedRowCount));

		// Act
		var actual = await _sut.Count(dataFlowExecutionId, nodeId, outputPortId, cancellationToken);

		// Assert
		actual.Should().Be(expectedRowCount);
	}

	private void SetupControllerContext(IQueryCollection queryCollection)
	{
		HttpContext httpContext = Substitute.For<HttpContext>();
		httpContext.Request.Query.Returns(queryCollection);

		_sut.ControllerContext = new ControllerContext
		{
			HttpContext = httpContext
		};
	}
}
