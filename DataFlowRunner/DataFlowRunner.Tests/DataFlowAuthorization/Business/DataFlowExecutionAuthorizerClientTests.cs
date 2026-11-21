using System.Net;
using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowAuthorization.Business;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;
using NSubstitute;
using Refit;

namespace DataFlowRunner.Tests.DataFlowAuthorization.Business;

public sealed class DataFlowExecutionAuthorizerClientTests
{
	private readonly IDataFlowAuthorizationClientApi _authorizationClientApi;
	private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;
	private readonly DataFlowExecutionAuthorizerClient _sut;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public DataFlowExecutionAuthorizerClientTests()
	{
		_authorizationClientApi = Substitute.For<IDataFlowAuthorizationClientApi>();
		_authenticationTokenAccessor = Substitute.For<IAuthenticationTokenAccessor>();
		_languageDictionaryProvider = LanguageDictionaryProvider.Instance;
		_sut = new DataFlowExecutionAuthorizerClient(
			_authorizationClientApi, _authenticationTokenAccessor, _languageDictionaryProvider);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowAuthorizationClientApiIsNull()
	{
		// Arrange
		IDataFlowAuthorizationClientApi? dataFlowAuthorizationClientApi = null;

		// Act
		Action act = () => _ = new DataFlowExecutionAuthorizerClient(
			dataFlowAuthorizationClientApi!, _authenticationTokenAccessor, _languageDictionaryProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenAuthenticationTokenAccessorIsNull()
	{
		// Arrange
		IAuthenticationTokenAccessor? authenticationTokenAccessor = null;

		// Act
		Action act = () => _ = new DataFlowExecutionAuthorizerClient(
			_authorizationClientApi, authenticationTokenAccessor!, _languageDictionaryProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public async Task AuthorizeDataFlowExecutionAsync_ShouldCallAuthorizationApi_WhenRequestIsValid()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var authenticationCookie = new AuthenticationCookie("test-cookie");
		_authenticationTokenAccessor.GetAuthenticationCookie().Returns(authenticationCookie);

		// Act
		await _sut.AuthorizeDataFlowExecutionAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		await _authorizationClientApi.Received(1).CheckDataFlowAuthorizationAsync(
			Arg.Is(dataFlowExecutionId.Value),
			Arg.Is<AuthenticationCookie>(cookie => cookie == authenticationCookie),
			Arg.Is<long?>(value => value == null),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task AuthorizeDataFlowExecutionAsync_ShouldThrowDataFlowExecutionUnauthorizedException_WhenAuthorizationApiReturns401()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var authenticationCookie = new AuthenticationCookie("test-cookie");
		_authenticationTokenAccessor.GetAuthenticationCookie().Returns(authenticationCookie);
		_authorizationClientApi.CheckDataFlowAuthorizationAsync(
				Arg.Is(dataFlowExecutionId.Value),
				Arg.Is<AuthenticationCookie>(cookie => cookie == authenticationCookie),
				Arg.Is<long?>(value => value == null),
				Arg.Any<CancellationToken>())
			.Returns(_ => throw CreateApiException(HttpStatusCode.Unauthorized));

		// Act
		Func<Task> act = async () => await _sut.AuthorizeDataFlowExecutionAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionUnauthorizedException>();
	}

	[Fact]
	public async Task AuthorizeDataFlowExecutionAsync_ShouldThrowDataFlowExecutionNotFoundOrAccessDeniedException_WhenAuthorizationApiReturns404()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var authenticationCookie = new AuthenticationCookie("test-cookie");
		_authenticationTokenAccessor.GetAuthenticationCookie().Returns(authenticationCookie);
		_authorizationClientApi.CheckDataFlowAuthorizationAsync(
				Arg.Is(dataFlowExecutionId.Value),
				Arg.Is<AuthenticationCookie>(cookie => cookie == authenticationCookie),
				Arg.Is<long?>(value => value == null),
				Arg.Any<CancellationToken>())
			.Returns(_ => throw CreateApiException(HttpStatusCode.NotFound));

		// Act
		Func<Task> act = async () => await _sut.AuthorizeDataFlowExecutionAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionNotFoundOrAccessDeniedException>();
	}

	[Fact]
	public async Task AuthorizeDataFlowExecutionAsync_ShouldThrowDataFlowExecutionAuthorizationFailedException_WhenAuthorizationApiReturns500()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var authenticationCookie = new AuthenticationCookie("test-cookie");
		ApiException apiException = CreateApiException(HttpStatusCode.InternalServerError);
		_authenticationTokenAccessor.GetAuthenticationCookie().Returns(authenticationCookie);
		_authorizationClientApi.CheckDataFlowAuthorizationAsync(
				Arg.Is(dataFlowExecutionId.Value),
				Arg.Is<AuthenticationCookie>(cookie => cookie == authenticationCookie),
				Arg.Is<long?>(value => value == null),
				Arg.Any<CancellationToken>())
			.Returns(_ => throw apiException);

		// Act
		Func<Task> act = async () => await _sut.AuthorizeDataFlowExecutionAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionAuthorizationFailedException>();
	}

	[Fact]
	public async Task AuthorizeDataFlowExecutionAsync_ShouldThrowDataFlowExecutionAuthorizationFailedException_WhenAuthorizationApiRequestFails()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var authenticationCookie = new AuthenticationCookie("test-cookie");
		_authenticationTokenAccessor.GetAuthenticationCookie().Returns(authenticationCookie);
		_authorizationClientApi.CheckDataFlowAuthorizationAsync(
				Arg.Is(dataFlowExecutionId.Value),
				Arg.Is<AuthenticationCookie>(cookie => cookie == authenticationCookie),
				Arg.Is<long?>(value => value == null),
				Arg.Any<CancellationToken>())
			.Returns(_ => throw new HttpRequestException("request failed"));

		// Act
		Func<Task> act = async () => await _sut.AuthorizeDataFlowExecutionAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionAuthorizationFailedException>();
	}

	[Fact]
	public async Task AuthorizeDataFlowExecutionAsync_ShouldThrowDataFlowExecutionAuthorizationFailedException_WhenAuthorizationApiRequestCanceled()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var authenticationCookie = new AuthenticationCookie("test-cookie");
		_authenticationTokenAccessor.GetAuthenticationCookie().Returns(authenticationCookie);
		_authorizationClientApi.CheckDataFlowAuthorizationAsync(
				Arg.Is(dataFlowExecutionId.Value),
				Arg.Is<AuthenticationCookie>(cookie => cookie == authenticationCookie),
				Arg.Is<long?>(value => value == null),
				Arg.Any<CancellationToken>())
			.Returns(_ => throw new TaskCanceledException("request failed"));

		// Act
		Func<Task> act = async () => await _sut.AuthorizeDataFlowExecutionAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionAuthorizationFailedException>();
	}

	private static ApiException CreateApiException(HttpStatusCode statusCode)
	{
		var httpResponseMessage = new HttpResponseMessage(statusCode);
		var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://test");
		return ApiException.Create(
			httpRequestMessage,
			HttpMethod.Get,
			httpResponseMessage,
			new RefitSettings()).GetAwaiter().GetResult();
	}
}
