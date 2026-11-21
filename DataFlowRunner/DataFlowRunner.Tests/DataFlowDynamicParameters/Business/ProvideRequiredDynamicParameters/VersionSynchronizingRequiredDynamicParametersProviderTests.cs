using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

public sealed class VersionSynchronizingRequiredDynamicParametersProviderTests
{
	private readonly IRequiredDynamicParametersProvider _inner;
	private readonly IDataFlowVersionConflictHandler _versionConflictHandler;
	private readonly VersionSynchronizingRequiredDynamicParametersProvider _sut;

	public VersionSynchronizingRequiredDynamicParametersProviderTests()
	{
		_inner = Substitute.For<IRequiredDynamicParametersProvider>();
		_versionConflictHandler = Substitute.For<IDataFlowVersionConflictHandler>();
		_sut = new VersionSynchronizingRequiredDynamicParametersProvider(_inner, _versionConflictHandler);
	}

	[Fact]
	public async Task ProvideDataFlowDynamicParametersAsync_ShouldCallVersionConflictHandlerThenDelegateToInner_WhenVersionConflictHandlerSucceeds()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		const long dataFlowVersion = 3;
		CancellationToken cancellationToken = CancellationToken.None;
		var expected = new List<DynamicParameterDto>();
		_inner.ProvideDataFlowDynamicParametersAsync(dataFlowExecutionId, dataFlowVersion, cancellationToken)
			.Returns(expected);

		// Act
		List<DynamicParameterDto> actual =
			await _sut.ProvideDataFlowDynamicParametersAsync(dataFlowExecutionId, dataFlowVersion, cancellationToken);

		// Assert
		actual.Should().BeSameAs(expected);
		await _versionConflictHandler.Received(1).HandleVersionConflictAsync(
			new DataFlowExecutionId(dataFlowExecutionId),
			new DataFlowVersion(dataFlowVersion),
			cancellationToken);
		await _inner.Received(1).ProvideDataFlowDynamicParametersAsync(
			dataFlowExecutionId, dataFlowVersion, cancellationToken);
	}

	[Fact]
	public async Task ProvideDataFlowDynamicParametersAsync_ShouldNotCallInner_WhenVersionConflictHandlerThrows()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		const long dataFlowVersion = 3;
		CancellationToken cancellationToken = CancellationToken.None;
		_versionConflictHandler
			.HandleVersionConflictAsync(
				new DataFlowExecutionId(dataFlowExecutionId),
				new DataFlowVersion(dataFlowVersion),
				cancellationToken)
			.ThrowsAsync(new InvalidOperationException());

		// Act
		Func<Task> act = () => _sut.ProvideDataFlowDynamicParametersAsync(
			dataFlowExecutionId, dataFlowVersion, cancellationToken);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
		await _inner.DidNotReceive().ProvideDataFlowDynamicParametersAsync(
			dataFlowExecutionId, dataFlowVersion, cancellationToken);
	}

	[Fact]
	public async Task ProvideDataFlowDynamicParametersForRunningNodeAsync_ShouldCallVersionConflictHandlerThenDelegateToInner_WhenVersionConflictHandlerSucceeds()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		const long dataFlowVersion = 3;
		CancellationToken cancellationToken = CancellationToken.None;
		var expected = new List<DynamicParameterDto>();
		_inner.ProvideDataFlowDynamicParametersForRunningNodeAsync(dataFlowExecutionId, nodeId, dataFlowVersion, cancellationToken)
			.Returns(expected);

		// Act
		List<DynamicParameterDto> actual = await _sut.ProvideDataFlowDynamicParametersForRunningNodeAsync(
			dataFlowExecutionId, nodeId, dataFlowVersion, cancellationToken);

		// Assert
		actual.Should().BeSameAs(expected);
		await _versionConflictHandler.Received(1).HandleVersionConflictAsync(
			new DataFlowExecutionId(dataFlowExecutionId),
			new DataFlowVersion(dataFlowVersion),
			cancellationToken);
		await _inner.Received(1).ProvideDataFlowDynamicParametersForRunningNodeAsync(
			dataFlowExecutionId, nodeId, dataFlowVersion, cancellationToken);
	}

	[Fact]
	public async Task ProvideDataFlowDynamicParametersForRunningNodeAsync_ShouldNotCallInner_WhenVersionConflictHandlerThrows()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		const long dataFlowVersion = 3;
		CancellationToken cancellationToken = CancellationToken.None;
		_versionConflictHandler
			.HandleVersionConflictAsync(
				new DataFlowExecutionId(dataFlowExecutionId),
				new DataFlowVersion(dataFlowVersion),
				cancellationToken)
			.ThrowsAsync(new InvalidOperationException());

		// Act
		Func<Task> act = () => _sut.ProvideDataFlowDynamicParametersForRunningNodeAsync(
			dataFlowExecutionId, nodeId, dataFlowVersion, cancellationToken);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>();
		await _inner.DidNotReceive().ProvideDataFlowDynamicParametersForRunningNodeAsync(
			dataFlowExecutionId, nodeId, dataFlowVersion, cancellationToken);
	}
}
