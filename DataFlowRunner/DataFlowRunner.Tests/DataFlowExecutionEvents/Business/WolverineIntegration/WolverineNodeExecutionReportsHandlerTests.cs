using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Reports;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionReports;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;

namespace DataFlowRunner.Tests.DataFlowExecutionEvents.Business.WolverineIntegration;

public sealed class WolverineNodeExecutionReportsHandlerTests
{
	private readonly IDataFlowExecutionInitializedReportHandler _dataFlowExecutionInitializedReportHandler;
	private readonly INodeExecutionSucceededReportHandler _nodeExecutionSucceededReportHandler;
	private readonly INodeExecutionFailedReportHandler _nodeExecutionFailedReportHandler;
	private readonly INodeExecutionCanceledReportHandler _nodeExecutionCanceledReportHandler;
	private readonly IGraphInitializationNotRequestedReportHandler _graphInitializationNotRequestedReportHandler;
	private readonly IWolverineExecutionReportMapper _executionReportMapper;
	private readonly WolverineNodeExecutionReportsHandler _sut;

	public WolverineNodeExecutionReportsHandlerTests()
	{
		_dataFlowExecutionInitializedReportHandler = Substitute.For<IDataFlowExecutionInitializedReportHandler>();
		_nodeExecutionSucceededReportHandler = Substitute.For<INodeExecutionSucceededReportHandler>();
		_nodeExecutionFailedReportHandler = Substitute.For<INodeExecutionFailedReportHandler>();
		_nodeExecutionCanceledReportHandler = Substitute.For<INodeExecutionCanceledReportHandler>();
		_graphInitializationNotRequestedReportHandler =
			Substitute.For<IGraphInitializationNotRequestedReportHandler>();
		_executionReportMapper = Substitute.For<IWolverineExecutionReportMapper>();
		_sut = new WolverineNodeExecutionReportsHandler(
			_dataFlowExecutionInitializedReportHandler,
			_nodeExecutionSucceededReportHandler,
			_nodeExecutionFailedReportHandler,
			_nodeExecutionCanceledReportHandler,
			_graphInitializationNotRequestedReportHandler,
			_executionReportMapper);
	}

	[Fact]
	public async Task HandleAsync_ShouldDelegateToDataFlowExecutionInitializedReportHandler_WhenDataFlowExecutionInitializedWolverineReportIsReceived()
	{
		// Arrange
		var wolverineReport = new DataFlowExecutionInitializedWolverineReport
		{
			GraphExecutionId = Guid.NewGuid(),
			IsSucceeded = true,
			Error = null
		};
		var report = new DataFlowExecutionInitializedReport
		{
			GraphExecutionId = Guid.NewGuid(),
			IsSucceeded = false,
			Error = new GraphExecutionError("mapped", "stack")
		};
		_executionReportMapper.ToReport(wolverineReport).Returns(report);
		using var cancellationTokenSource = new CancellationTokenSource();

		// Act
		await _sut.HandleAsync(wolverineReport, cancellationTokenSource.Token);

		// Assert
		_executionReportMapper.Received(1).ToReport(
			Arg.Is<DataFlowExecutionInitializedWolverineReport>(actual => ReferenceEquals(actual, wolverineReport)));
		await _dataFlowExecutionInitializedReportHandler.Received(1).HandleAsync(
			Arg.Is<DataFlowExecutionInitializedReport>(actual => ReferenceEquals(actual, report)),
			CancellationToken.None);
	}

	[Fact]
	public async Task HandleAsync_ShouldDelegateToGraphInitializationNotRequestedReportHandler_WhenGraphInitializationNotRequestedReportIsProvided()
	{
		// Arrange
		var wolverineReport = new GraphInitializationNotRequestedWolverineReport
		{
			GraphExecutionId = Guid.NewGuid()
		};
		var report = new GraphInitializationNotRequestedReport
		{
			GraphExecutionId = Guid.NewGuid()
		};
		_executionReportMapper.ToReport(wolverineReport).Returns(report);
		using var cancellationTokenSource = new CancellationTokenSource();

		// Act
		await _sut.HandleAsync(wolverineReport, cancellationTokenSource.Token);

		// Assert
		_executionReportMapper.Received(1).ToReport(
			Arg.Is<GraphInitializationNotRequestedWolverineReport>(actual =>
				ReferenceEquals(actual, wolverineReport)));
		await _graphInitializationNotRequestedReportHandler.Received(1).HandleAsync(
			Arg.Is<GraphInitializationNotRequestedReport>(actual => ReferenceEquals(actual, report)),
			CancellationToken.None);
	}

	[Fact]
	public async Task HandleAsync_ShouldDelegateToNodeExecutionSucceededReportHandler_WhenNodeExecutionSucceededReportIsProvided()
	{
		// Arrange
		var wolverineReport = new NodeExecutionSucceededWolverineReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid()
		};
		var report = new NodeExecutionSucceededReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid()
		};
		_executionReportMapper.ToReport(wolverineReport).Returns(report);
		using var cancellationTokenSource = new CancellationTokenSource();

		// Act
		await _sut.HandleAsync(wolverineReport, cancellationTokenSource.Token);

		// Assert
		_executionReportMapper.Received(1).ToReport(
			Arg.Is<NodeExecutionSucceededWolverineReport>(actual => ReferenceEquals(actual, wolverineReport)));
		await _nodeExecutionSucceededReportHandler.Received(1).HandleAsync(
			Arg.Is<NodeExecutionSucceededReport>(actual => ReferenceEquals(actual, report)),
			CancellationToken.None);
	}

	[Fact]
	public async Task HandleAsync_ShouldDelegateToNodeExecutionFailedReportHandler_WhenNodeExecutionFailedReportIsProvided()
	{
		// Arrange
		var wolverineReport = new NodeExecutionFailedWolverineReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			Error = new GraphExecutionError("wire", "wire-stack")
		};
		var report = new NodeExecutionFailedReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			Error = new GraphExecutionError("mapped", "mapped-stack")
		};
		_executionReportMapper.ToReport(wolverineReport).Returns(report);
		using var cancellationTokenSource = new CancellationTokenSource();

		// Act
		await _sut.HandleAsync(wolverineReport, cancellationTokenSource.Token);

		// Assert
		_executionReportMapper.Received(1).ToReport(
			Arg.Is<NodeExecutionFailedWolverineReport>(actual => ReferenceEquals(actual, wolverineReport)));
		await _nodeExecutionFailedReportHandler.Received(1).HandleAsync(
			Arg.Is<NodeExecutionFailedReport>(actual => ReferenceEquals(actual, report)),
			CancellationToken.None);
	}

	[Fact]
	public async Task HandleAsync_ShouldDelegateToNodeExecutionCanceledReportHandler_WhenNodeExecutionCanceledReportIsProvided()
	{
		// Arrange
		var wolverineReport = new NodeExecutionCanceledWolverineReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			IsSucceeded = true,
			Error = null
		};
		var report = new NodeExecutionCanceledReport
		{
			GraphExecutionId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			IsSucceeded = false,
			Error = new GraphExecutionError("mapped", "stack")
		};
		_executionReportMapper.ToReport(wolverineReport).Returns(report);
		using var cancellationTokenSource = new CancellationTokenSource();

		// Act
		await _sut.HandleAsync(wolverineReport, cancellationTokenSource.Token);

		// Assert
		_executionReportMapper.Received(1).ToReport(
			Arg.Is<NodeExecutionCanceledWolverineReport>(actual => ReferenceEquals(actual, wolverineReport)));
		await _nodeExecutionCanceledReportHandler.Received(1).HandleAsync(
			Arg.Is<NodeExecutionCanceledReport>(actual => ReferenceEquals(actual, report)),
			CancellationToken.None);
	}
}
