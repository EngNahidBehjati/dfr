using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Abstractions;
using DataFlowRunner.DataFlowAuthorization.Business;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowAuthorization.Business;

public sealed class DataFlowExecutionUserAuthorizerTests
{
	private readonly IDataFlowExecutionUserIdStore _dataFlowExecutionUserIdStore;
	private readonly IDataFlowExecutionAuthorizerClient _dataFlowExecutionAuthorizerClient;
	private readonly IAuthenticatedUserProvider _authenticatedUserProvider;
	private readonly DataFlowExecutionUserAuthorizer _sut;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public DataFlowExecutionUserAuthorizerTests()
	{
		_dataFlowExecutionUserIdStore = Substitute.For<IDataFlowExecutionUserIdStore>();
		_dataFlowExecutionAuthorizerClient = Substitute.For<IDataFlowExecutionAuthorizerClient>();
		_authenticatedUserProvider = Substitute.For<IAuthenticatedUserProvider>();
		_languageDictionaryProvider = LanguageDictionaryProvider.Instance;

		_sut = new DataFlowExecutionUserAuthorizer(
			_dataFlowExecutionUserIdStore, _dataFlowExecutionAuthorizerClient,
			_authenticatedUserProvider, _languageDictionaryProvider);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenUserIdStoreIsNull()
	{
		// Arrange
		IDataFlowExecutionUserIdStore? dataFlowExecutionUserIdStore = null;

		// Act
		Action act = () => _ = new DataFlowExecutionUserAuthorizer(
			dataFlowExecutionUserIdStore!, _dataFlowExecutionAuthorizerClient,
			_authenticatedUserProvider, _languageDictionaryProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("dataFlowExecutionUserIdStore");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionAuthorizerIsNull()
	{
		// Arrange
		IDataFlowExecutionAuthorizerClient? dataFlowExecutionAuthorizer = null;

		// Act
		Action act = () => _ = new DataFlowExecutionUserAuthorizer(
			_dataFlowExecutionUserIdStore, dataFlowExecutionAuthorizer!,
			_authenticatedUserProvider, _languageDictionaryProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("dataFlowExecutionAuthorizerClient");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenAuthenticatedUserProviderIsNull()
	{
		// Arrange
		IAuthenticatedUserProvider? authenticatedUserProvider = null;

		// Act
		Action act = () => _ = new DataFlowExecutionUserAuthorizer(
			_dataFlowExecutionUserIdStore, _dataFlowExecutionAuthorizerClient,
			authenticatedUserProvider!, _languageDictionaryProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("authenticatedUserProvider");
	}

	[Fact]
	public async Task AuthorizeUserAccessAsync_ShouldThrowDataFlowExecutionUserIdConflictException_WhenUserMappingExistsButDoesNotMatch()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var currentUserId = new UserId(Guid.NewGuid());
		var existingUserId = new UserId(Guid.NewGuid());
		_dataFlowExecutionUserIdStore.TryGetDataFlowExecutionUserId(dataFlowExecutionId, out Arg.Any<UserId>())
			.Returns(callInfo =>
			{
				callInfo[1] = existingUserId;
				return true;
			});
		_authenticatedUserProvider.GetCurrentUserId(Arg.Any<CancellationToken>()).Returns(currentUserId);

		// Act
		Func<Task> act = async () => await _sut.AuthorizeUserAccessAsync(
			dataFlowExecutionId,
			CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionUserIdConflictException>();
	}

	[Fact]
	public async Task AuthorizeUserAccessAsync_ShouldStoreUserMapping_WhenUserMappingDoesNotExistAndAuthorizationSucceeds()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var currentUserId = new UserId(Guid.NewGuid());
		_dataFlowExecutionUserIdStore.TryGetDataFlowExecutionUserId(dataFlowExecutionId, out Arg.Any<UserId>())
			.Returns(false);
		_authenticatedUserProvider.GetCurrentUserId(Arg.Any<CancellationToken>()).Returns(currentUserId);

		// Act
		await _sut.AuthorizeUserAccessAsync(
			dataFlowExecutionId,
			CancellationToken.None);

		// Assert
		await _dataFlowExecutionAuthorizerClient.Received(1).AuthorizeDataFlowExecutionAsync(
			Arg.Is(dataFlowExecutionId),
			Arg.Any<CancellationToken>());
		_dataFlowExecutionUserIdStore.Received(1).StoreDataFlowExecutionUserId(dataFlowExecutionId, currentUserId);
	}

	[Fact]
	public async Task AuthorizeUserAccessAsync_ShouldNotStoreUserMappingOrAuthorize_WhenUserMappingExistsAndMatches()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var currentUserId = new UserId(Guid.NewGuid());
		_dataFlowExecutionUserIdStore.TryGetDataFlowExecutionUserId(dataFlowExecutionId, out Arg.Any<UserId>())
			.Returns(callInfo =>
			{
				callInfo[1] = currentUserId;
				return true;
			});
		_authenticatedUserProvider.GetCurrentUserId(Arg.Any<CancellationToken>()).Returns(currentUserId);

		// Act
		await _sut.AuthorizeUserAccessAsync(
			dataFlowExecutionId,
			CancellationToken.None);

		// Assert
		await _dataFlowExecutionAuthorizerClient.DidNotReceiveWithAnyArgs()
			.AuthorizeDataFlowExecutionAsync(default, Arg.Any<CancellationToken>());
		_dataFlowExecutionUserIdStore.DidNotReceive().StoreDataFlowExecutionUserId(dataFlowExecutionId, currentUserId);
	}

	[Fact]
	public async Task AuthorizeUserAccessAsync_ShouldPropagateExceptionAndNotStoreUserMapping_WhenAuthorizationFails()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var currentUserId = new UserId(Guid.NewGuid());
		var exception = new DataFlowExecutionUnauthorizedException();
		_dataFlowExecutionUserIdStore.TryGetDataFlowExecutionUserId(dataFlowExecutionId, out Arg.Any<UserId>())
			.Returns(false);
		_authenticatedUserProvider.GetCurrentUserId(Arg.Any<CancellationToken>()).Returns(currentUserId);
		_dataFlowExecutionAuthorizerClient.AuthorizeDataFlowExecutionAsync(
				Arg.Is(dataFlowExecutionId),
				Arg.Any<CancellationToken>())
			.Returns(_ => throw exception);

		// Act
		Func<Task> act = async () => await _sut.AuthorizeUserAccessAsync(
			dataFlowExecutionId,
			CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<DataFlowExecutionUnauthorizedException>();
		_dataFlowExecutionUserIdStore.DidNotReceive().StoreDataFlowExecutionUserId(dataFlowExecutionId, currentUserId);
	}
}
