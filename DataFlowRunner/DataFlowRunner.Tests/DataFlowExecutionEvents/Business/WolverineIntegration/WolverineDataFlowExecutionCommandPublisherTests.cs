using DataFlowRunner.DataFlowExecutionEvents.Abstractions;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Mappers;
using DataFlowRunner.DataFlowExecutionEvents.Business.WolverineIntegration.Messages.Commands;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace DataFlowRunner.Tests.DataFlowExecutionEvents.Business.WolverineIntegration;

public sealed class WolverineDataFlowExecutionCommandPublisherTests
{
	private readonly IMessageBus _messageBus;
	private readonly ILogger<WolverineDataFlowExecutionCommandPublisher> _logger;
	private readonly WolverineDataFlowExecutionCommandPublisher _sut;

	public WolverineDataFlowExecutionCommandPublisherTests()
	{
		_messageBus = Substitute.For<IMessageBus>();
		_logger = Substitute.For<ILogger<WolverineDataFlowExecutionCommandPublisher>>();
		_sut = new WolverineDataFlowExecutionCommandPublisher(
			_messageBus, _logger, new WolverineExecutionCommandMapper());
	}

	public static TheoryData<object, object> SupportedCommands()
	{
		var data = new TheoryData<object, object>();
		AddInitializeCommand(data);
		AddExecuteNodeCommand(data);
		AddCancelNodeCommand(data);
		AddDisposeCommand(data);
		AddChangeProcessorCommand(data);
		return data;
	}

	[Theory]
	[MemberData(nameof(SupportedCommands))]
	public async Task PublishCommandAsync_ShouldSendMappedWolverineCommand_WhenCommandIsSupported(
		object commandObject,
		object expectedWolverineCommand)
	{
		// Arrange
		var command = (IExecutionCommand)commandObject;

		// Act
		await _sut.PublishCommandAsync(command, CancellationToken.None);

		// Assert
		var sendCalls = _messageBus.ReceivedCalls()
			.Where(call => call.GetMethodInfo().Name == nameof(IMessageBus.SendAsync))
			.ToArray();
		sendCalls.Should().ContainSingle();
		object? wolverineCommand = sendCalls[0].GetArguments()[0];
		wolverineCommand.Should().BeOfType(expectedWolverineCommand.GetType());
		wolverineCommand.Should().BeEquivalentTo(expectedWolverineCommand);
		var deliveryOptions = sendCalls[0].GetArguments()[1].Should().BeOfType<DeliveryOptions>().Subject;
		deliveryOptions.PartitionKey.Should().Be(command.GraphExecutionId.ToString());
	}

	[Fact]
	public async Task PublishCommandAsync_ShouldScopeLogsWithApplicationCommand_WhenCommandIsSupported()
	{
		// Arrange
		var command = new CancelNodeCommand
		{
			GraphExecutionId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			NodeId = Guid.NewGuid()
		};

		// Act
		await _sut.PublishCommandAsync(command, CancellationToken.None);

		// Assert
		_logger.Received(1).BeginScope(Arg.Is<IReadOnlyDictionary<string, object>>(state =>
			state["GraphExecutionId"].Equals(command.GraphExecutionId)
			&& state["UserId"].Equals(command.UserId)
			&& state["NodeId"].Equals(command.NodeId)
			&& state["CommandType"].Equals(nameof(CancelNodeCommand))));
	}

	[Fact]
	public async Task PublishCommandAsync_ShouldThrowInvalidOperationException_WhenCommandIsUnsupported()
	{
		// Arrange
		var command = new UnsupportedExecutionCommand();

		// Act
		Func<Task> act = async () => await _sut.PublishCommandAsync(command, CancellationToken.None);

		// Assert
		await act.Should().ThrowAsync<InvalidOperationException>()
			.WithMessage("*UnsupportedExecutionCommand*");
	}

	private static void AddInitializeCommand(TheoryData<object, object> data)
	{
		var processorInfo = new ProcessorInfo
		{
			ProcessorType = "PROCESSOR_LIST",
			ProcessorId = Guid.NewGuid()
		};
		var command = new InitializeDataFlowExecutionCommand
		{
			GraphExecutionId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			DataFlowId = Guid.NewGuid(),
			ProcessorInfo = processorInfo
		};
		data.Add(command, new InitializeDataFlowExecutionWolverineCommand
		{
			GraphExecutionId = command.GraphExecutionId,
			UserId = command.UserId,
			DataFlowId = command.DataFlowId,
			ProcessorInfo = processorInfo
		});
	}

	private static void AddExecuteNodeCommand(TheoryData<object, object> data)
	{
		IReadOnlyCollection<NodeInputPortToUpstreamPorts> portConnections =
		[
			new NodeInputPortToUpstreamPorts
			{
				InputPort = Guid.NewGuid(),
				UpstreamPorts = [Guid.NewGuid()]
			}
		];
		IReadOnlyCollection<ExecutionDynamicParameter> dynamicParameters =
		[
			new ExecutionDynamicParameter
			{
				Name = "age",
				Value = 1
			}
		];
		var command = new ExecuteNodeCommand
		{
			GraphExecutionId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			NodeId = Guid.NewGuid(),
			Config = "config",
			PortConnections = portConnections,
			DynamicParameters = dynamicParameters
		};
		data.Add(command, new ExecuteNodeWolverineCommand
		{
			GraphExecutionId = command.GraphExecutionId,
			UserId = command.UserId,
			NodeId = command.NodeId,
			Config = command.Config,
			PortConnections = portConnections,
			DynamicParameters = dynamicParameters
		});
	}

	private static void AddCancelNodeCommand(TheoryData<object, object> data)
	{
		var command = new CancelNodeCommand
		{
			GraphExecutionId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			NodeId = Guid.NewGuid()
		};
		data.Add(command, new CancelNodeWolverineCommand
		{
			GraphExecutionId = command.GraphExecutionId,
			UserId = command.UserId,
			NodeId = command.NodeId
		});
	}

	private static void AddDisposeCommand(TheoryData<object, object> data)
	{
		var command = new DisposeDataFlowExecutionCommand
		{
			GraphExecutionId = Guid.NewGuid(),
			UserId = Guid.NewGuid()
		};
		data.Add(command, new DisposeDataFlowExecutionWolverineCommand
		{
			GraphExecutionId = command.GraphExecutionId,
			UserId = command.UserId
		});
	}

	private static void AddChangeProcessorCommand(TheoryData<object, object> data)
	{
		var processorInfo = new ProcessorInfo
		{
			ProcessorType = "PROCESSOR_LIST",
			ProcessorId = Guid.NewGuid()
		};
		var command = new ChangeDataFlowProcessorCommand
		{
			GraphExecutionId = Guid.NewGuid(),
			UserId = Guid.NewGuid(),
			ProcessorInfo = processorInfo
		};
		data.Add(command, new ChangeDataFlowProcessorWolverineCommand
		{
			GraphExecutionId = command.GraphExecutionId,
			UserId = command.UserId,
			ProcessorInfo = processorInfo
		});
	}

	private sealed record UnsupportedExecutionCommand : IExecutionCommand
	{
		public Guid GraphExecutionId { get; } = Guid.NewGuid();
		public Guid UserId { get; } = Guid.NewGuid();

		public IReadOnlyDictionary<string, object> GetLogParameters()
		{
			return new Dictionary<string, object>();
		}
	}
}
