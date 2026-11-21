using DataFlowRunner.GraphCore.Models.NodeModels;

namespace DataFlowRunner.DataFlowNodeConfigs.Models;

internal sealed class NodeConfig
{
	public required string Content { set; get; }
	public required NodeVersion Version { set; get; }
}
