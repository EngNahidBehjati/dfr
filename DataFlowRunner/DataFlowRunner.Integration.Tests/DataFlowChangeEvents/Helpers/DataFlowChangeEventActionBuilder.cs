using System.Text.Json;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.Integration.Tests.DataFlowChangeEvents.Helpers;

internal static class DataFlowChangeEventActionBuilder
{
	private static readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);

	public static JsonElement AddPluginAction(Guid nodeId)
	{
		const string type = "ADD_PLUGIN_V1";
		return SerializeAction(new
		{
			type,
			plugin = new NodeInfo(
				nodeId,
				"integration-test-node",
				"TestPlugin",
				1,
				new Dictionary<string, IEnumerable<PortInfo>>()
				{
					["in"] = new List<PortInfo>(),
					["out"] = new List<PortInfo>()
				})
		});
	}

	public static JsonElement SetPluginConfigAction(Guid nodeId)
	{
		const string type = "SET_PLUGIN_CONFIG_V1";
		return SerializeAction(new
		{
			type,
			id = nodeId,
			dynamicOutputPorts = new List<DynamicOutputPortInfo>()
		});
	}

	public static JsonElement RemovePluginAction(Guid nodeId)
	{
		const string type = "REMOVE_PLUGIN_V1";
		return SerializeAction(new
		{
			type,
			id = nodeId
		});
	}

	public static JsonElement AddLinkAction(Guid edgeId, Guid fromPortId, Guid toPortId, Guid fromNodeId, Guid toNodeId)
	{
		const string type = "ADD_LINK_V1";
		return SerializeAction(new
		{
			type,
			link = new EdgeInfo(
				edgeId,
				fromPortId,
				toPortId,
				fromNodeId,
				toNodeId)
		});
	}

	public static JsonElement RemoveLinkAction(Guid edgeId)
	{
		const string type = "REMOVE_LINK_V1";
		return SerializeAction(new
		{
			type,
			id = edgeId
		});
	}

	public static JsonElement ChangeExecutionSettingsAction(string executorType, Guid? executorId)
	{
		const string type = "CHANGE_PIPELINE_EXECUTION_SETTINGS_V1";
		return SerializeAction(new
		{
			type,
			newExecutionSettings = new
			{
				type = executorType,
				executorId
			}
		});
	}

	public static JsonElement ActionMissingTypeProperty(Guid nodeId)
	{
		return SerializeAction(new
		{
			plugin = new NodeInfo(
				nodeId,
				"no-type-field",
				"TestPlugin",
				1,
				new Dictionary<string, IEnumerable<PortInfo>>()
				{
					["in"] = new List<PortInfo>(),
					["out"] = new List<PortInfo>()
				})
		});
	}

	public static JsonElement UnknownTypeAction()
	{
		return SerializeAction(new
		{
			type = "UNKNOWN_ACTION_TYPE_V1"
		});
	}

	private static JsonElement SerializeAction(object payload)
	{
		return JsonSerializer.SerializeToElement(payload, _serializerOptions);
	}
}
