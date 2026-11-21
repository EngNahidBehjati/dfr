using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.DataFlowExecutionReportHandlers.Abstraction;
using DataFlowRunner.DataFlowExecutionReportHandlers.Business;
using DataFlowRunner.DataFlowExecutionReportHandlers.Handlers;

namespace DataFlowRunner.DataFlowExecutionReportHandlers.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IDataFlowExecutionInitializedReportHandler, DataFlowExecutionInitializedReportHandler>();
		services.AddSingleton<IGraphInitializationNotRequestedReportHandler, GraphInitializationNotRequestedReportHandler>();
		services.AddSingleton<INodeExecutionSucceededReportHandler, NodeExecutionSucceededReportHandler>();
		services.AddSingleton<INodeExecutionFailedReportHandler, NodeExecutionFailedReportHandler>();
		services.AddSingleton<INodeExecutionCanceledReportHandler, NodeExecutionCanceledReportHandler>();

		services.AddSingleton<INodeExecutionReadinessChecker, NodeExecutionReadinessChecker>();
		services.AddSingleton<IChildNodesExecutor, ChildNodesExecutor>();
		services.AddSingleton<IPendingDownstreamNodeCanceler, PendingDownstreamNodeCanceler>();
	}
}
