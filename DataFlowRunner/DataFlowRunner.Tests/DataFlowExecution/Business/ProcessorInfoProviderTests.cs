using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.Tests.Common;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business;

public class ProcessorInfoProviderTests
{
	private readonly IDataFlowExecutionContextProvider _dataFlowExecutionContextProvider;
	private readonly ProcessorInfoProvider _sut;

	public ProcessorInfoProviderTests()
	{
		_dataFlowExecutionContextProvider = Substitute.For<IDataFlowExecutionContextProvider>();
		_sut = new ProcessorInfoProvider(_dataFlowExecutionContextProvider);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowExecutionContextProviderIsNull()
	{
		// Arrange
		IDataFlowExecutionContextProvider dataFlowExecutionContextProvider = null!;

		// Act
		Action act = () => _ = new ProcessorInfoProvider(dataFlowExecutionContextProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowExecutionContextProvider");
	}

	[Fact]
	public async Task GetProcessorInfoAsync_ShouldReturnProcessorInfoFromContext_WhenContextExists()
	{
		// Arrange
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var expectedProcessorInfo = new ProcessorInfo
		{
			ProcessorType = "SYSTEM_DEFAULT_PROCESSOR",
			ProcessorId = Guid.NewGuid()
		};
		var dataFlowExecutionContext = new DataFlowExecutionContext
		{
			Graph = Substitute.For<IGraph>(),
			DataFlowExecutionId = dataFlowExecutionId,
			DataFlowVersion = new DataFlowVersion(1),
			ProcessorInfo = expectedProcessorInfo,
			ExecutionSettings = DefaultExecutionSettingsCreator.Create()
		};
		_dataFlowExecutionContextProvider
			.GetRunningDataFlowExecutionContextAsync(dataFlowExecutionId, CancellationToken.None)
			.Returns(dataFlowExecutionContext);

		// Act
		ProcessorInfo result = await _sut.GetProcessorInfoAsync(dataFlowExecutionId, CancellationToken.None);

		// Assert
		result.Should().BeSameAs(expectedProcessorInfo);
	}
}
