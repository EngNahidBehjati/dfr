using DataFlowRunner.Common.Configuration;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Exceptions.Conflict;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.Language.LanguageDictionary;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public sealed class DataFlowVersionConflictHandlerTests
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IDataFlowInvalidator _dataFlowInvalidator;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider = LanguageDictionaryProvider.Instance;
	private readonly IOptions<DataFlowExecutionFeatureFlags> _featureFlagsOptions;
	private readonly DataFlowVersionConflictHandler _sut;

	public DataFlowVersionConflictHandlerTests()
	{
		_dataFlowExecutionContextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_dataFlowInvalidator = Substitute.For<IDataFlowInvalidator>();
		_featureFlagsOptions = Substitute.For<IOptions<DataFlowExecutionFeatureFlags>>();
		_featureFlagsOptions.Value.Returns(new DataFlowExecutionFeatureFlags { WaitForDataFlowVersionUpdate = false });
		_sut = new DataFlowVersionConflictHandler(_dataFlowExecutionContextProvider, _languageDictionaryProvider
			, _dataFlowInvalidator, _featureFlagsOptions);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionContextProviderIsNull()
	{
		// Arrange
		IDataFlowExecutionContextProvider provider = null!;

		// Act
		Action act = () => _ = new DataFlowVersionConflictHandler(
			provider, _languageDictionaryProvider, _dataFlowInvalidator,
			Options.Create(new DataFlowExecutionFeatureFlags()));

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenLanguageDictionaryProviderIsNull()
	{
		// Arrange
		ILanguageDictionaryProvider languageDictionaryProvider = null!;

		// Act
		Action act = () => _ = new DataFlowVersionConflictHandler(
			_dataFlowExecutionContextProvider, languageDictionaryProvider, _dataFlowInvalidator,
			Options.Create(new DataFlowExecutionFeatureFlags()));

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowInvalidatorIsNull()
	{
		// Arrange
		IDataFlowInvalidator dataFlowInvalidator = null!;

		// Act
		Action act = () => _ = new DataFlowVersionConflictHandler(
			_dataFlowExecutionContextProvider, _languageDictionaryProvider, dataFlowInvalidator,
			Options.Create(new DataFlowExecutionFeatureFlags()));

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenFeatureFlagsOptionsIsNull()
	{
		// Arrange

		// Act
		Action act = () => _ = new DataFlowVersionConflictHandler(
			_dataFlowExecutionContextProvider, _languageDictionaryProvider, _dataFlowInvalidator,
			null!);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public async Task HandleVersionConflictAsync_ShouldUpdateDataFlowExecutionContext_WhenExecutionNotFound()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var dataFlowVersion = new DataFlowVersion(1);
		CancellationToken cancellationToken = CancellationToken.None;
		_dataFlowExecutionContextProvider
			.TryGetRunningDataFlowExecutionContext(executionId, out Arg.Any<DataFlowExecutionContext?>())
			.Returns(false);

		// Act
		await _sut.HandleVersionConflictAsync(executionId, dataFlowVersion, cancellationToken);

		// Assert
		await _dataFlowExecutionContextProvider.Received(1)
			.UpdateDataFlowExecutionContextAsync(executionId, cancellationToken, dataFlowVersion);
	}

	[Fact]
	public async Task HandleVersionConflictAsync_ShouldThrowDataFlowVersionConflictException_WhenRunningVersionIsGreater()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var dataFlowVersion = new DataFlowVersion(1);
		CancellationToken cancellationToken = CancellationToken.None;
		var runningContext = new DataFlowExecutionContext
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(2),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider
			.TryGetRunningDataFlowExecutionContext(executionId, out Arg.Any<DataFlowExecutionContext?>())
			.Returns(x =>
			{
				x[1] = runningContext;
				return true;
			});

		// Act
		Func<Task> act = () => _sut.HandleVersionConflictAsync(executionId, dataFlowVersion, cancellationToken);

		// Assert
		await act.Should().ThrowAsync<DataFlowVersionConflictException>();
		await _dataFlowExecutionContextProvider.DidNotReceiveWithAnyArgs()
			.UpdateDataFlowExecutionContextAsync(default, CancellationToken.None);
	}

	[Fact]
	public async Task HandleVersionConflictAsync_ShouldWaitUntilVersionIsUpdatedByChangeHandlers_WhenRunningVersionIsLessAndWaitForDataFlowVersionUpdateIsTrue()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var flowVersion = new DataFlowVersion(2);
		CancellationToken cancellationToken = CancellationToken.None;
		var getRunningCallCount = 0;
		IGraph graph = Substitute.For<IGraph>();
		var staleContext = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		var updatedContext = new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowExecutionId = executionId,
			DataFlowVersion = flowVersion,
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider
			.TryGetRunningDataFlowExecutionContext(executionId, out Arg.Any<DataFlowExecutionContext?>())
			.Returns(x =>
			{
				x[1] = staleContext;
				return true;
			});
		_dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(executionId, Arg.Any<CancellationToken>())
			.Returns(_ =>
			{
				getRunningCallCount++;
				return getRunningCallCount == 1 ? staleContext : updatedContext;
			});
		_featureFlagsOptions.Value.Returns(new DataFlowExecutionFeatureFlags { WaitForDataFlowVersionUpdate = true });

		// Act
		await _sut.HandleVersionConflictAsync(executionId, flowVersion, cancellationToken);

		// Assert
		getRunningCallCount.Should().BeGreaterThan(1);
		await _dataFlowExecutionContextProvider.DidNotReceiveWithAnyArgs()
			.UpdateDataFlowExecutionContextAsync(default, CancellationToken.None);
	}

	[Fact]
	public async Task HandleVersionConflictAsync_ShouldInvalidateDataFlowAndUpdateDataFlowExecutionContext_WhenRunningVersionIsLessAndWaitForDataFlowVersionUpdateIsFalse()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var flowVersion = new DataFlowVersion(2);
		CancellationToken cancellationToken = CancellationToken.None;
		var staleContext = new DataFlowExecutionContext
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider
			.TryGetRunningDataFlowExecutionContext(executionId, out Arg.Any<DataFlowExecutionContext?>())
			.Returns(x =>
			{
				x[1] = staleContext;
				return true;
			});
		_featureFlagsOptions.Value.Returns(new DataFlowExecutionFeatureFlags { WaitForDataFlowVersionUpdate = false });

		// Act
		await _sut.HandleVersionConflictAsync(executionId, flowVersion, cancellationToken);

		// Assert
		await _dataFlowInvalidator.Received(1)
			.InvalidateDataFlowAsync(staleContext, cancellationToken);
		await _dataFlowExecutionContextProvider.Received(1)
			.UpdateDataFlowExecutionContextAsync(executionId, cancellationToken, flowVersion);
	}

	[Fact]
	public async Task HandleVersionConflictAsync_ShouldDoNothing_WhenVersionsAreEqual()
	{
		// Arrange
		var executionId = new DataFlowExecutionId(Guid.NewGuid());
		var flowVersion = new DataFlowVersion(1);
		CancellationToken cancellationToken = CancellationToken.None;
		var runningContext = new DataFlowExecutionContext
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = executionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider
			.TryGetRunningDataFlowExecutionContext(executionId, out Arg.Any<DataFlowExecutionContext?>())
			.Returns(x =>
			{
				x[1] = runningContext;
				return true;
			});

		// Act
		await _sut.HandleVersionConflictAsync(executionId, flowVersion, cancellationToken);

		// Assert
		await _dataFlowExecutionContextProvider.DidNotReceiveWithAnyArgs()
			.UpdateDataFlowExecutionContextAsync(default, CancellationToken.None, null);
	}
}
