using System.Text.Json;
using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Business.ActionRouting;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Models;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.GraphCore.Abstractions;
using DataFlowRunner.Tests.Common;
using Microsoft.Extensions.Logging;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Routing;

public sealed class DataFlowChangeActionRouterTests
{
	private readonly DataFlowChangeContext _context;

	private readonly IDataFlowChangeEventHandler<AddNode> _addNodeHandler;
	private readonly IDataFlowChangeEventHandler<SetNodeConfig> _setNodeConfigHandler;
	private readonly IDataFlowChangeEventHandler<RemoveNode> _removeNodeHandler;
	private readonly IDataFlowChangeEventHandler<AddEdge> _addEdgeHandler;
	private readonly IDataFlowChangeEventHandler<RemoveEdge> _removeEdgeHandler;
	private readonly IDataFlowChangeEventHandler<CreateDataFlowVariable> _createDataFlowVariableHandler;
	private readonly IDataFlowChangeEventHandler<UpdateDataFlowVariable> _updateDataFlowVariableHandler;
	private readonly IDataFlowChangeEventHandler<DeleteDataFlowVariable> _deleteDataFlowVariableHandler;
	private readonly IDataFlowChangeEventHandler<ChangeProcessor> _changeProcessorHandler;
	private readonly IDataFlowChangeEventHandler<ChangeExecutionSettings> _changeExecutionSettingsHandler;

	private readonly DataFlowChangeActionRouter _sut;

	public DataFlowChangeActionRouterTests()
	{
		var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
		var runningVersion = new DataFlowVersion(1);
		IGraph graph = Substitute.For<IGraph>();
		ProcessorInfo processorInfo = DefaultProcessorInfoCreator.Create();
		_context = new DataFlowChangeContext(
			dataFlowExecutionId,
			runningVersion,
			new DataFlowVersion(10),
			graph,
			processorInfo,
			DefaultExecutionSettingsCreator.Create());

		_addNodeHandler = Substitute.For<IDataFlowChangeEventHandler<AddNode>>();
		_setNodeConfigHandler = Substitute.For<IDataFlowChangeEventHandler<SetNodeConfig>>();
		_removeNodeHandler = Substitute.For<IDataFlowChangeEventHandler<RemoveNode>>();
		_addEdgeHandler = Substitute.For<IDataFlowChangeEventHandler<AddEdge>>();
		_removeEdgeHandler = Substitute.For<IDataFlowChangeEventHandler<RemoveEdge>>();
		_createDataFlowVariableHandler = Substitute.For<IDataFlowChangeEventHandler<CreateDataFlowVariable>>();
		_updateDataFlowVariableHandler = Substitute.For<IDataFlowChangeEventHandler<UpdateDataFlowVariable>>();
		_deleteDataFlowVariableHandler = Substitute.For<IDataFlowChangeEventHandler<DeleteDataFlowVariable>>();
		_changeProcessorHandler = Substitute.For<IDataFlowChangeEventHandler<ChangeProcessor>>();
		_changeExecutionSettingsHandler = Substitute.For<IDataFlowChangeEventHandler<ChangeExecutionSettings>>();
		ILogger<DataFlowChangeActionRouter> logger = Substitute.For<ILogger<DataFlowChangeActionRouter>>();

		_sut = new DataFlowChangeActionRouter(
			_addNodeHandler,
			_setNodeConfigHandler,
			_removeNodeHandler,
			_addEdgeHandler,
			_removeEdgeHandler,
			_createDataFlowVariableHandler,
			_updateDataFlowVariableHandler,
			_deleteDataFlowVariableHandler,
			_changeProcessorHandler,
			_changeExecutionSettingsHandler,
			logger);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallAddNodeHandlerWithDeserializedEvent_WhenActionTypeIsAddPluginV1()
	{
		// Arrange
		var pluginId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""
			{
			  "type": "ADD_PLUGIN_V1",
			  "eagerConfig": { "packageId": "00000000-0000-0000-0000-000000000099" },
			  "plugin": {
			    "id": "{{pluginId}}",
			    "title": "test-plugin",
			    "type": "PACKAGE",
			    "version": 1,
			    "ports": {}
			  }
			}
			""");

		// Act
		await _sut.RouteAsync("ADD_PLUGIN_V1", action, _context, CancellationToken.None);

		// Assert
		await _addNodeHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<AddNode>(x => x.NodeInfo.Id == pluginId && x.NodeInfo.Title == "test-plugin"),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallSetNodeConfigHandlerWithDeserializedEvent_WhenActionTypeIsSetPluginConfigV1()
	{
		// Arrange
		var pluginId = Guid.NewGuid();
		var portId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""
			{
			  "type": "SET_PLUGIN_CONFIG_V1",
			  "id": "{{pluginId}}",
			  "dynamicOutputPorts": [
			    { "id": "{{portId}}", "title": "manual_table" }
			  ]
			}
			""");

		// Act
		await _sut.RouteAsync("SET_PLUGIN_CONFIG_V1", action, _context, CancellationToken.None);

		// Assert
		await _setNodeConfigHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<SetNodeConfig>(x =>
				x.Id == pluginId &&
				x.DynamicOutputPorts.Count == 1 &&
				x.DynamicOutputPorts[0].Id == portId),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallRemoveNodeHandlerWithDeserializedEvent_WhenActionTypeIsRemovePluginV1()
	{
		// Arrange
		var pluginId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""{"type":"REMOVE_PLUGIN_V1","id":"{{pluginId}}"}""");

		// Act
		await _sut.RouteAsync("REMOVE_PLUGIN_V1", action, _context, CancellationToken.None);

		// Assert
		await _removeNodeHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<RemoveNode>(x => x.Id == pluginId),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallAddEdgeHandlerWithDeserializedEvent_WhenActionTypeIsAddLinkV1()
	{
		// Arrange
		var linkId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""
			{
			  "type": "ADD_LINK_V1",
			  "link": {
			    "id": "{{linkId}}",
			    "from": "00000000-0000-0000-0000-000000000001",
			    "to": "00000000-0000-0000-0000-000000000002",
			    "fromPlugin": "00000000-0000-0000-0000-000000000003",
			    "toPlugin": "00000000-0000-0000-0000-000000000004"
			  }
			}
			""");

		// Act
		await _sut.RouteAsync("ADD_LINK_V1", action, _context, CancellationToken.None);

		// Assert
		await _addEdgeHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<AddEdge>(x => x.EdgeInfo.Id == linkId),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallRemoveEdgeHandlerWithDeserializedEvent_WhenActionTypeIsRemoveLinkV1()
	{
		// Arrange
		var linkId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""{"type":"REMOVE_LINK_V1","id":"{{linkId}}"}""");

		// Act
		await _sut.RouteAsync("REMOVE_LINK_V1", action, _context, CancellationToken.None);

		// Assert
		await _removeEdgeHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<RemoveEdge>(x => x.Id == linkId),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallCreateDataFlowVariableHandlerWithDeserializedEvent_WhenActionTypeIsCreateDataFlowVariableV1()
	{
		// Arrange
		var variableId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""
			{
			  "type": "CREATE_DATA_FLOW_VARIABLE_V1",
			  "dataFlowVariable": {
			    "id": "{{variableId}}",
			    "type": "VARIABLE",
			    "name": "myVar",
			    "list": false,
			    "required": false,
			    "dataType": "INTEGER"
			  }
			}
			""");

		// Act
		await _sut.RouteAsync("CREATE_DATA_FLOW_VARIABLE_V1", action, _context, CancellationToken.None);

		// Assert
		await _createDataFlowVariableHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<CreateDataFlowVariable>(x =>
				x.DataFlowVariableInfo.Id == variableId &&
				x.DataFlowVariableInfo.Name == "myVar"),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallUpdateDataFlowVariableHandlerWithDeserializedEvent_WhenActionTypeIsUpdateDataFlowVariableV1()
	{
		// Arrange
		var variableId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""
			{
			  "type": "UPDATE_DATA_FLOW_VARIABLE_V1",
			  "dataFlowVariable": {
			    "id": "{{variableId}}",
			    "type": "VARIABLE",
			    "name": "myVar",
			    "list": false,
			    "required": false,
			    "dataType": "INTEGER"
			  }
			}
			""");

		// Act
		await _sut.RouteAsync("UPDATE_DATA_FLOW_VARIABLE_V1", action, _context, CancellationToken.None);

		// Assert
		await _updateDataFlowVariableHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<UpdateDataFlowVariable>(x =>
				x.DataFlowVariable.Id == variableId &&
				x.DataFlowVariable.Name == "myVar"),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallDeleteDataFlowVariableHandlerWithDeserializedEvent_WhenActionTypeIsDeleteDataFlowVariableV1()
	{
		// Arrange
		var variableId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""{"type":"DELETE_DATA_FLOW_VARIABLE_V1","id":"{{variableId}}"}""");

		// Act
		await _sut.RouteAsync("DELETE_DATA_FLOW_VARIABLE_V1", action, _context, CancellationToken.None);

		// Assert
		await _deleteDataFlowVariableHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<DeleteDataFlowVariable>(x => x.Id == variableId),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallChangeProcessorHandlerWithDeserializedEvent_WhenActionTypeIsChangePipelineDataDestinationV1()
	{
		// Arrange
		var processorId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""
			{
			  "type": "CHANGE_PIPELINE_DATA_DESTINATION_V1",
			  "newDataDestination": {
			    "type": "SOME_PROCESSOR",
			    "processorId": "{{processorId}}"
			  }
			}
			""");

		// Act
		await _sut.RouteAsync("CHANGE_PIPELINE_DATA_DESTINATION_V1", action, _context, CancellationToken.None);

		// Assert
		await _changeProcessorHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<ChangeProcessor>(x => x.DataFlowProcessorInfo.ProcessorId == processorId),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldCallChangeExecutionSettingsHandlerWithDeserializedEvent_WhenActionTypeIsChangePipelineExecutionSettingsV1()
	{
		// Arrange
		var executorId = Guid.NewGuid();
		JsonElement action = ParseElement($$"""
			{
			  "type": "CHANGE_PIPELINE_EXECUTION_SETTINGS_V1",
			  "newExecutionSettings": {
			    "type": "EXECUTORS_LIST",
			    "executorId": "{{executorId}}"
			  }
			}
			""");

		// Act
		await _sut.RouteAsync("CHANGE_PIPELINE_EXECUTION_SETTINGS_V1", action, _context, CancellationToken.None);

		// Assert
		await _changeExecutionSettingsHandler.Received(1).HandleAsync(
			_context,
			Arg.Is<ChangeExecutionSettings>(x =>
				x.DataFlowExecutionSettings.ExecutorId == executorId &&
				x.DataFlowExecutionSettings.Type == "EXECUTORS_LIST"),
			CancellationToken.None);
	}

	[Fact]
	public async Task RouteAsync_ShouldNotCallAnyHandler_WhenActionTypeIsUnknown()
	{
		// Arrange
		JsonElement action = ParseElement("""{"type":"SOMETHING_ELSE","x":1}""");

		// Act
		await _sut.RouteAsync("SOMETHING_ELSE", action, _context, CancellationToken.None);

		// Assert
		await _addNodeHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
		await _setNodeConfigHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
		await _removeNodeHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
		await _addEdgeHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
		await _removeEdgeHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
		await _createDataFlowVariableHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
		await _updateDataFlowVariableHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
		await _deleteDataFlowVariableHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
		await _changeProcessorHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
		await _changeExecutionSettingsHandler.DidNotReceiveWithAnyArgs().HandleAsync(null!, null!, CancellationToken.None);
	}

	private static JsonElement ParseElement(string json)
	{
		using var doc = JsonDocument.Parse(json);
		return doc.RootElement.Clone();
	}
}
