using DataFlowRunner.DataFlowChangeEvents.Business.Handlers;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.Tests.Common;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Handlers;

public sealed class ChangeExecutionSettingsHandlerTests
{
	private readonly IDataFlowDisposer _dataFlowDisposer;
	private readonly ChangeExecutionSettingsHandler _sut;

	public ChangeExecutionSettingsHandlerTests()
	{
		_dataFlowDisposer = Substitute.For<IDataFlowDisposer>();
		_sut = new ChangeExecutionSettingsHandler(_dataFlowDisposer);
	}

	[Fact]
	public async Task HandleAsync_ShouldUpdateContextExecutionSettings_WhenCalled()
	{
		// Arrange
		DataFlowChangeContext context = CreateContext();
		var executorId = Guid.NewGuid();
		var changeEvent = new ChangeExecutionSettings(new DataFlowExecutionSettings("EXECUTORS_LIST", executorId));
		var expectedExecutionSettings = new ExecutionSettings
		{
			ExecutorType = "EXECUTORS_LIST",
			ExecutorId = executorId
		};

		// Act
		await _sut.HandleAsync(context, changeEvent, CancellationToken.None);

		// Assert
		context.ExecutionSettings.Should().BeEquivalentTo(expectedExecutionSettings);
	}

	[Fact]
	public async Task HandleAsync_ShouldDisposeDataFlow_WhenCalled()
	{
		// Arrange
		DataFlowChangeContext context = CreateContext();
		var changeEvent = new ChangeExecutionSettings(
			new DataFlowExecutionSettings("EXECUTORS_LIST", Guid.NewGuid()));

		// Act
		await _sut.HandleAsync(context, changeEvent, CancellationToken.None);

		// Assert
		await _dataFlowDisposer.Received(1).DisposeAsync(context.DataFlowExecutionId, CancellationToken.None);
	}

	private static DataFlowChangeContext CreateContext()
	{
		return new DataFlowChangeContext(
			DataFlowExecutionId.New(),
			new DataFlowVersion(5),
			new DataFlowVersion(10),
			Substitute.For<IGraph>(),
			DefaultProcessorInfoCreator.Create(),
			DefaultExecutionSettingsCreator.Create());
	}
}
