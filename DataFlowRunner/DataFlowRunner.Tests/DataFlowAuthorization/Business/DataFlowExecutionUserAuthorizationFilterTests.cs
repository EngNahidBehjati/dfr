using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowAuthorization.Business;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowAuthorization.Business;

public sealed class DataFlowExecutionUserAuthorizationFilterTests
{
	private readonly IRouteParameterExtractor _routeParameterExtractor;
	private readonly IDataFlowExecutionUserAuthorizer _authorizer;
	private readonly DefaultHttpContext _httpContext;
	private readonly AuthorizationFilterContext _filterContext;
	private readonly DataFlowExecutionUserAuthorizationFilter _sut;

	public DataFlowExecutionUserAuthorizationFilterTests()
	{
		_routeParameterExtractor = Substitute.For<IRouteParameterExtractor>();
		_authorizer = Substitute.For<IDataFlowExecutionUserAuthorizer>();
		_httpContext = new DefaultHttpContext();
		_filterContext = new AuthorizationFilterContext(
			new ActionContext(_httpContext, new RouteData(), new ActionDescriptor()),
			new List<IFilterMetadata>());

		_sut = new DataFlowExecutionUserAuthorizationFilter(
			_routeParameterExtractor,
			_authorizer);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenRouteParameterExtractorIsNull()
	{
		// Arrange
		IRouteParameterExtractor? routeParameterExtractor = null;

		// Act
		Action act = () => _ = new DataFlowExecutionUserAuthorizationFilter(
			routeParameterExtractor!,
			_authorizer);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("routeParameterExtractor");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenUserAuthorizerIsNull()
	{
		// Arrange
		IDataFlowExecutionUserAuthorizer? userAuthorizer = null;

		// Act
		Action act = () => _ = new DataFlowExecutionUserAuthorizationFilter(
			_routeParameterExtractor,
			userAuthorizer!);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("dataFlowExecutionUserAuthorizer");
	}

	[Fact]
	public async Task OnAuthorizationAsync_ShouldAuthorizeUserAccess_WhenAuthorizationSucceeds()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		_routeParameterExtractor.ExtractDataFlowExecutionId(_httpContext).Returns(dataFlowExecutionId);

		// Act
		await _sut.OnAuthorizationAsync(_filterContext);

		// Assert
		_routeParameterExtractor.Received(1).ExtractDataFlowExecutionId(_httpContext);
		await _authorizer.Received(1).AuthorizeUserAccessAsync(
			dataFlowExecutionId,
			_httpContext.RequestAborted);
	}

	[Fact]
	public async Task OnAuthorizationAsync_ShouldPropagateException_WhenAuthorizationFails()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		_routeParameterExtractor.ExtractDataFlowExecutionId(_httpContext).Returns(dataFlowExecutionId);
		var unauthorizedException = new DataFlowExecutionUnauthorizedException();
		_authorizer.AuthorizeUserAccessAsync(
				dataFlowExecutionId,
				Arg.Any<CancellationToken>())
			.Returns(_ => throw unauthorizedException);

		// Act
		Func<Task> act = async () => await _sut.OnAuthorizationAsync(_filterContext);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionUnauthorizedException>();
	}

	[Fact]
	public async Task OnAuthorizationAsync_ShouldThrowArgumentNullException_WhenContextIsNull()
	{
		// Arrange
		AuthorizationFilterContext? context = null;

		// Act
		Func<Task> act = async () => await _sut.OnAuthorizationAsync(context!);

		// Assert
		await act.Should().ThrowAsync<ArgumentNullException>()
			.WithParameterName("context");
	}
}

