using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.GraphTraversal.Abstractions;
using DataFlowRunner.GraphTraversal.Business;

namespace DataFlowRunner.GraphTraversal.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IChildNodesFinder, ChildNodesFinder>();
		services.AddSingleton<IStartingNodesFinder, StartingNodesFinder>();
		services.AddSingleton<IUpstreamSubgraphDetector, UpstreamSubgraphDetector>();
		services.AddSingleton<IDownstreamSubgraphDetector, DownstreamSubgraphDetector>();
		services.AddSingleton<ISubGraphCreator, SubGraphCreator>();
		services.AddSingleton<IUnExecutedSubgraphDetector, UnExecutedSubgraphDetector>();
	}
}
