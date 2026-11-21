using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.DataFlowSnapshot.Abstractions;

internal interface ITopologyMapper
{
	IGraph MapToGraph(Guid graphId, DataFlowTopologyDto dataFlowTopology);
}
