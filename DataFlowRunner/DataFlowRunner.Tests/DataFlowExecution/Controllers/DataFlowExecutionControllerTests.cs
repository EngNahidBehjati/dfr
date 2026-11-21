using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Controllers;
using DataFlowRunner.DataFlowExecution.Dto;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Controllers;

public class DataFlowExecutionControllerTests
{
	private readonly IDataFlowExecutionFacade _dataFlowExecutionFacade;
	private readonly DataFlowExecutionController _sut;
	private readonly ILogger<DataFlowExecutionController> _logger;

	public DataFlowExecutionControllerTests()
	{
		_dataFlowExecutionFacade = Substitute.For<IDataFlowExecutionFacade>();
		_logger = Substitute.For<ILogger<DataFlowExecutionController>>();
		_sut = new DataFlowExecutionController(_dataFlowExecutionFacade, _logger);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenIDataFlowExecutionFacadeIsNull()
	{
		// Arrange
		IDataFlowExecutionFacade dataFlowExecutionFacade = null!;

		// Act
		Action act = () => { _ = new DataFlowExecutionController(dataFlowExecutionFacade, _logger); };

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("dataFlowExecutionFacade");
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenILoggerIsNull()
	{
		// Arrange
		ILogger<DataFlowExecutionController> logger = null!;

		// Act
		Action act = () => { _ = new DataFlowExecutionController(_dataFlowExecutionFacade, logger); };

		// Assert
		act.Should().Throw<ArgumentNullException>()
			.WithParameterName("logger");
	}

	[Fact]
	public async Task RunDataFlow_ShouldCallFacadeAndReturnOk_Whenever()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		const long dataFlowVersion = 5;
		CancellationToken cancellationToken = CancellationToken.None;
		var dataFlowExecutionRequestBody = new RunDataFlowDto();

		// Act
		IActionResult result = await _sut.RunDataFlow(executionId, dataFlowVersion, dataFlowExecutionRequestBody,
			cancellationToken);

		// Assert
		await _dataFlowExecutionFacade.Received(1)
			.RunDataFlow(executionId, dataFlowVersion, dataFlowExecutionRequestBody, cancellationToken);
		result.Should().BeOfType<OkResult>();
	}

	[Fact]
	public async Task RunDataFlowNode_ShouldCallFacadeAndReturnOk_Whenever()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		const long dataFlowVersion = 7;
		CancellationToken cancellationToken = CancellationToken.None;
		var dataFlowExecutionRequestBody = new RunDataFlowNodeDto();

		// Act
		IActionResult result = await _sut
			.RunDataFlowNode(executionId, nodeId, dataFlowVersion, dataFlowExecutionRequestBody, cancellationToken);

		// Assert
		await _dataFlowExecutionFacade.Received(1)
			.RunDataFlowNode(executionId, nodeId, dataFlowVersion, dataFlowExecutionRequestBody, cancellationToken);
		result.Should().BeOfType<OkResult>();
	}

	[Fact]
	public async Task CancelDataFlow_ShouldCallFacadeAndReturnOk_Whenever()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		CancellationToken cancellationToken = CancellationToken.None;

		// Act
		IActionResult result = await _sut.CancelDataFlow(executionId, cancellationToken);

		// Assert
		await _dataFlowExecutionFacade.Received(1).CancelDataFlow(executionId, cancellationToken);
		result.Should().BeOfType<OkResult>();
	}

	[Fact]
	public async Task CancelDataFlowNode_ShouldCallFacadeAndReturnOk_Whenever()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		CancellationToken cancellationToken = CancellationToken.None;

		// Act
		IActionResult result = await _sut.CancelDataFlowNode(executionId, nodeId, cancellationToken);

		// Assert
		await _dataFlowExecutionFacade.Received(1)
			.CancelDataFlowNode(executionId, nodeId, cancellationToken);
		result.Should().BeOfType<OkResult>();
	}

	[Fact]
	public async Task InvalidateDataFlow_ShouldCallFacadeAndReturnOk_Whenever()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		CancellationToken cancellationToken = CancellationToken.None;

		// Act
		IActionResult result = await _sut.InvalidateDataFlow(executionId, cancellationToken);

		// Assert
		await _dataFlowExecutionFacade.Received(1).InvalidateDataFlow(executionId, cancellationToken);
		result.Should().BeOfType<OkResult>();
	}

	[Fact]
	public async Task InvalidateDataFlowNode_ShouldCallFacadeAndReturnOk_Whenever()
	{
		// Arrange
		var executionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		CancellationToken cancellationToken = CancellationToken.None;

		// Act
		IActionResult result = await _sut.InvalidateDataFlowNode(executionId, nodeId, cancellationToken);

		// Assert
		await _dataFlowExecutionFacade.Received(1)
			.InvalidateDataFlowNode(executionId, nodeId, cancellationToken);
		result.Should().BeOfType<OkResult>();
	}
}
