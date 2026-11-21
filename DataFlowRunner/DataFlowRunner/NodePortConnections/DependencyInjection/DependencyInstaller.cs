using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.NodePortConnections.Abstract;
using DataFlowRunner.NodePortConnections.Business;

namespace DataFlowRunner.NodePortConnections.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<INodeInputPortToUpstreamPortsResolver, NodeInputPortToUpstreamPortsResolver>();
		services.AddSingleton<IEdgeByPortContainer, EdgeByPortContainer>();
	}
}

