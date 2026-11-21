using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowAuthorization.Business;
using DataFlowRunner.DataFlowAuthorization.Exceptions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.Language.LanguageDictionary;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowAuthorization.Business;

public sealed class DataFlowExecutionUserIdStoreTests
{
	private readonly DataFlowExecutionUserIdStore _sut;

	public DataFlowExecutionUserIdStoreTests()
	{
		_sut = new DataFlowExecutionUserIdStore(LanguageDictionaryProvider.Instance);
	}

	[Fact]
	public void GetDataFlowExecutionUserId_ShouldReturnUserId_WhenExecutionIdExists()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var expectedUserId = new UserId(Guid.NewGuid());
		_sut.StoreDataFlowExecutionUserId(dataFlowExecutionId, expectedUserId);

		// Act
		UserId result = _sut.GetDataFlowExecutionUserId(dataFlowExecutionId);

		// Assert
		result.Should().Be(expectedUserId);
	}

	[Fact]
	public void GetDataFlowExecutionUserId_ShouldThrowDataFlowExecutionUserIdNotFoundException_WhenExecutionIdDoesNotExist()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		// Act
		Action act = () => _sut.GetDataFlowExecutionUserId(dataFlowExecutionId);

		// Assert
		act.Should().Throw<DataFlowExecutionUserIdNotFoundException>();
	}

	[Fact]
	public void StoreDataFlowExecutionUserId_ShouldStoreUserId_Whenever()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var expectedUserId = new UserId(Guid.NewGuid());

		// Act
		_sut.StoreDataFlowExecutionUserId(dataFlowExecutionId, expectedUserId);
		UserId result = _sut.GetDataFlowExecutionUserId(dataFlowExecutionId);

		// Assert
		result.Should().Be(expectedUserId);
	}

	[Fact]
	public void TryGetDataFlowExecutionUserId_ShouldReturnTrue_WhenExecutionIdExists()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var userId = new UserId(Guid.NewGuid());
		_sut.StoreDataFlowExecutionUserId(dataFlowExecutionId, userId);

		// Act
		var result = _sut.TryGetDataFlowExecutionUserId(dataFlowExecutionId, out UserId retrievedUserId);

		// Assert
		result.Should().BeTrue();
		retrievedUserId.Should().Be(userId);
	}

	[Fact]
	public void TryGetDataFlowExecutionUserId_ShouldReturnFalse_WhenExecutionIdDoesNotExist()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());

		// Act
		var result = _sut.TryGetDataFlowExecutionUserId(dataFlowExecutionId, out UserId userId);

		// Assert
		result.Should().BeFalse();
		userId.Should().Be(default);
	}
}
