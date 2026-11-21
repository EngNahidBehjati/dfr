using System.Text.Json;
using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Business.Facades;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.Tests.Common;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Facades;

public sealed class DataFlowChangeEventsFacadeTests
{
	private readonly IActionTypeExtractor _actionTypeExtractor;
	private readonly IDataFlowChangeActionRouter _actionRouter;
	private readonly IDataFlowExecutionIdProvider _dataFlowExecutionIdProvider;
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly IDataFlowExecutionContextContainer _dataFlowExecutionContextContainer;

	private readonly DataFlowChangeEventsFacade _sut;

	public DataFlowChangeEventsFacadeTests()
	{
		ILogger<DataFlowChangeEventsFacade> logger = Substitute.For<ILogger<DataFlowChangeEventsFacade>>();
		_actionTypeExtractor = Substitute.For<IActionTypeExtractor>();
		_actionRouter = Substitute.For<IDataFlowChangeActionRouter>();
		_dataFlowExecutionIdProvider = Substitute.For<IDataFlowExecutionIdProvider>();
		_dataFlowExecutionContextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_dataFlowExecutionContextContainer = Substitute.For<IDataFlowExecutionContextContainer>();

		_sut = new DataFlowChangeEventsFacade(
			logger,
			_actionTypeExtractor,
			_actionRouter,
			_dataFlowExecutionIdProvider,
			_dataFlowExecutionContextProvider,
			_dataFlowExecutionContextContainer);
	}

	[Fact]
	public async Task HandleAsync_ShouldReturnWithoutRouting_WhenExecutionIdIsMissing()
	{
		// Arrange
		var envelope = new Envelope { PartitionKey = "not-a-guid" };
		var changeEvent = new ChangeEvent(
			1, 10, [ParseElement("""{"type":"ADD_PLUGIN_V1"}""")]);

		_dataFlowExecutionIdProvider
			.TryGetDataFlowExecutionId(envelope, out Arg.Any<Guid>())
			.Returns(callInfo =>
			{
				callInfo[1] = Guid.Empty;
				return false;
			});

		// Act
		await _sut.HandleAsync(changeEvent, envelope, CancellationToken.None);

		// Assert
		await _actionRouter.DidNotReceiveWithAnyArgs()
			.RouteAsync(null!, default, null!, CancellationToken.None);
		_dataFlowExecutionContextContainer.DidNotReceiveWithAnyArgs().AddOrUpdateDataFlowExecutionContext(null!);
	}

	[Fact]
	public async Task HandleAsync_ShouldRouteOnlyActionsWithValidType_WhenSomeActionsHaveNoType()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var envelope = new Envelope { PartitionKey = executionId.ToString() };

		_dataFlowExecutionIdProvider
			.TryGetDataFlowExecutionId(envelope, out Arg.Any<Guid>())
			.Returns(callInfo =>
			{
				callInfo[1] = executionId;
				return true;
			});

		JsonElement actionWithType = ParseElement("""{"type":"ADD_PLUGIN_V1","x":1}""");
		JsonElement actionWithoutType = ParseElement("""{"x":2}""");

		_actionTypeExtractor
			.TryGetActionType(actionWithType, out Arg.Any<string?>())
			.Returns(callInfo =>
			{
				callInfo[1] = "ADD_PLUGIN_V1";
				return true;
			});

		_actionTypeExtractor
			.TryGetActionType(actionWithoutType, out Arg.Any<string?>())
			.Returns(callInfo =>
			{
				callInfo[1] = null;
				return false;
			});

		var changeEvent = new ChangeEvent(1, 10, [actionWithType, actionWithoutType]);

		var runningContext = new DataFlowExecutionContext
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = new DataFlowExecutionId(executionId),
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = DefaultProcessorInfoCreator.Create(),
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider
			.TryGetRunningDataFlowExecutionContext(new DataFlowExecutionId(executionId),
				out Arg.Any<DataFlowExecutionContext?>())
			.Returns(callInfo =>
			{
				callInfo[1] = runningContext;
				return true;
			});

		// Act
		await _sut.HandleAsync(changeEvent, envelope, CancellationToken.None);

		// Assert
		await _actionRouter.Received(1).RouteAsync(
			"ADD_PLUGIN_V1",
			actionWithType,
			Arg.Is<DataFlowChangeContext>(context =>
				context.DataFlowExecutionId == new DataFlowExecutionId(executionId) &&
				context.NewVersion == new DataFlowVersion(10)),
			CancellationToken.None);

		await _actionRouter.DidNotReceive().RouteAsync(
			Arg.Is<string>(t => t != "ADD_PLUGIN_V1"),
			Arg.Any<JsonElement>(),
			Arg.Any<DataFlowChangeContext>(),
			CancellationToken.None);

		_dataFlowExecutionContextContainer.Received(2).AddOrUpdateDataFlowExecutionContext(
			Arg.Is<DataFlowExecutionContext>(context =>
				context.Graph == runningContext.Graph &&
				context.DataFlowExecutionId == runningContext.DataFlowExecutionId &&
				context.ProcessorInfo == runningContext.ProcessorInfo &&
				context.ExecutionSettings == runningContext.ExecutionSettings));

		_dataFlowExecutionContextContainer.Received(1).AddOrUpdateDataFlowExecutionContext(
			Arg.Is<DataFlowExecutionContext>(context =>
				ReferenceEquals(context.Graph, runningContext.Graph) &&
				context.DataFlowExecutionId == runningContext.DataFlowExecutionId &&
				context.DataFlowVersion.Equals(new DataFlowVersion(1)) &&
				context.ProcessorInfo == runningContext.ProcessorInfo &&
				context.ExecutionSettings == runningContext.ExecutionSettings));

		_dataFlowExecutionContextContainer.Received(1).AddOrUpdateDataFlowExecutionContext(
			Arg.Is<DataFlowExecutionContext>(context =>
				ReferenceEquals(context.Graph, runningContext.Graph) &&
				context.DataFlowExecutionId == runningContext.DataFlowExecutionId &&
				context.DataFlowVersion.Equals(new DataFlowVersion(10)) &&
				context.ProcessorInfo == runningContext.ProcessorInfo &&
				context.ExecutionSettings == runningContext.ExecutionSettings));
	}

	private static JsonElement ParseElement(string json)
	{
		using var doc = JsonDocument.Parse(json);
		return doc.RootElement.Clone();
	}
}
