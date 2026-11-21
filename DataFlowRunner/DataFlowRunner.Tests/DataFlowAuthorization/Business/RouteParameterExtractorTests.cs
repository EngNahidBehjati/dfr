using DataFlowRunner.DataFlowAuthorization.Business;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowAuthorization.Business;

public sealed class RouteParameterExtractorTests
{
	private readonly RouteParameterExtractor _sut;

	public RouteParameterExtractorTests()
	{
		_sut = new RouteParameterExtractor(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void ExtractDataFlowExecutionId_ShouldThrowDataFlowExecutionIdNotFoundInRouteException_WhenRouteValueDoesNotExist()
	{
		// Arrange
		HttpContext context = CreateHttpContext(new RouteValueDictionary());

		// Act
		Action act = () => _sut.ExtractDataFlowExecutionId(context);

		// Assert
		act.Should().Throw<DataFlowExecutionIdNotFoundInRouteException>();
	}

	[Fact]
	public void ExtractDataFlowExecutionId_ShouldReturnDataFlowExecutionId_WhenRouteValueIsStringGuid()
	{
		// Arrange
		var expectedId = Guid.NewGuid();
		var routeValues = new RouteValueDictionary { ["dataFlowExecutionId"] = expectedId.ToString() };
		HttpContext context = CreateHttpContext(routeValues);

		// Act
		DataFlowExecutionId result = _sut.ExtractDataFlowExecutionId(context);

		// Assert
		result.Value.Should().Be(expectedId);
	}

	[Fact]
	public void ExtractDataFlowExecutionId_ShouldThrowDataFlowExecutionIdNotFoundInRouteException_WhenRouteValueIsInvalidGuid()
	{
		// Arrange
		var routeValues = new RouteValueDictionary { ["dataFlowExecutionId"] = "invalid-guid" };
		HttpContext context = CreateHttpContext(routeValues);

		// Act
		Action act = () => _sut.ExtractDataFlowExecutionId(context);

		// Assert
		act.Should().Throw<DataFlowExecutionIdNotFoundInRouteException>();
	}

	[Fact]
	public void ExtractDataFlowExecutionId_ShouldReturnDataFlowExecutionId_WhenRouteValueIsEmptyGuid()
	{
		// Arrange
		var routeValues = new RouteValueDictionary { ["dataFlowExecutionId"] = Guid.Empty };
		HttpContext context = CreateHttpContext(routeValues);

		// Act
		DataFlowExecutionId result = _sut.ExtractDataFlowExecutionId(context);

		// Assert
		result.Value.Should().Be(Guid.Empty);
	}

	[Fact]
	public void ExtractDataFlowExecutionId_ShouldThrowArgumentNullException_WhenContextIsNull()
	{
		// Arrange
		HttpContext? context = null;

		// Act
		Action act = () => _sut.ExtractDataFlowExecutionId(context!);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("context");
	}

	private static HttpContext CreateHttpContext(RouteValueDictionary routeValues)
	{
		HttpContext context = Substitute.For<HttpContext>();
		context.Request.RouteValues.Returns(routeValues);
		return context;
	}
}
