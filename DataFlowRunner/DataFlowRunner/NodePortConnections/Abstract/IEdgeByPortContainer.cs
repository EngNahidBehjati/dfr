using DataFlowRunner.GraphCore.Models.EdgeModels;
using DataFlowRunner.NodePortConnections.Models;

namespace DataFlowRunner.NodePortConnections.Abstract;

internal interface IEdgeByPortContainer
{
	EdgeByPort GetEdgeByPort(EdgeId edgeId);
	bool TryAddEdgeByPort(EdgeByPort edgeByPort);
}
