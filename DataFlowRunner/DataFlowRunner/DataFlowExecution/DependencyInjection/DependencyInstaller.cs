using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Business;

namespace DataFlowRunner.DataFlowExecution.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IDataFlowExecutor, DataFlowExecutor>();
		services.AddSingleton<IGraphExecutionEligibilityChecker, GraphExecutionEligibilityChecker>();
		services.AddSingleton<IGraphExecutionSetupService, GraphExecutionSetupService>();
		services.AddSingleton<IGraphExecutor, GraphExecutor>();
		services.AddSingleton<INodeConfigInitializer, NodeConfigInitializer>();
		services.AddSingleton<INodeExecutor, NodeExecutor>();
		services.AddSingleton<INodeExecutorEngineGraphInitializer, NodeExecutorEngineGraphInitializer>();
		services.AddSingleton<IDataFlowDisposer, DataFlowDisposer>();
		services.AddSingleton<IDynamicParameterMismatchNodeInvalidator, DynamicParameterMismatchNodeInvalidator>();
		services.AddSingleton<IProcessorInfoProvider, ProcessorInfoProvider>();

		services.AddSingleton<IDataFlowVersionConflictHandler, DataFlowVersionConflictHandler>();
		services.AddSingleton<IDataFlowExecutionFacade, DataFlowExecutionFacade>();
		services.AddSingleton<INodeExecutorEngineGraphInitializationTracker, NodeExecutorEngineGraphInitializationTracker>();
		services.AddSingleton<IDataFlowExecutionContextProvider, DataFlowExecutionContextProvider>();
		services.AddSingleton<IDataFlowExecutionContextContainer, DataFlowExecutionContextContainer>();
		services.AddSingleton<IDataFlowInvalidator, DataFlowInvalidator>();
		services.AddSingleton<INodeCanceller, NodeCanceller>();
		services.AddSingleton<IDataFlowInvalidationNodeNextState, DataFlowInvalidationNodeNextState>();
		services.AddSingleton<IDataFlowCanceller, DataFlowCanceller>();
		services.AddSingleton<IDataFlowCancellationNodeNextState, DataFlowCancellationNodeNextState>();
		services.AddSingleton<INodeExecutionRevoker, NodeExecutionRevoker>();
	}
}
