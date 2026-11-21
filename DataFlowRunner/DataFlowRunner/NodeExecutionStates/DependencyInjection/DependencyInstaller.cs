using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.NodeExecutionStates.Abstractions;
using DataFlowRunner.NodeExecutionStates.Business;
using DataFlowRunner.NodeExecutionStates.Mappers;
using DataFlowRunner.NodeExecutionStates.Store;

namespace DataFlowRunner.NodeExecutionStates.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<InMemoryNodeExecutionStateStore>();
		services.AddSingleton<INodeExecutionStateProvider>(sp => sp.GetRequiredService<InMemoryNodeExecutionStateStore>());
		services.AddSingleton<INodeExecutionStateStore>(sp => sp.GetRequiredService<InMemoryNodeExecutionStateStore>());
		services.AddSingleton<INodeExecutionStateUpdater, NodeExecutionStateUpdater>();
		services.AddSingleton<INodeExecutionStateTransitionPolicy, NodeExecutionStateTransitionPolicy>();
		services.AddSingleton<INodeExecutionStateMachine, NodeExecutionStateMachine>();
		services.AddSingleton<INodeExecutionStateMapper, NodeExecutionStateMapper>();
		services.AddSingleton<INodeExecutionStatesDtoProvider, NodeExecutionStatesDtoProvider>();
		services.AddSingleton<INodeErrorMessageDtoProvider, NodeErrorMessageDtoProvider>();
		services.AddSingleton<INodeErrorMessageStore, InMemoryNodeErrorMessageStore>();
		services.AddSingleton<INodeErrorMessageMapper, NodeErrorMessageMapper>();
		services.AddSingleton<INodeExecutionErrorRecorder, NodeExecutionErrorRecorder>();
		services.AddSingleton<INodeExecutionTimingStore, InMemoryNodeExecutionTimingStore>();
		services.AddSingleton<INodeExecutionTimeRecorder, NodeExecutionTimeRecorder>();
		services.AddSingleton<INodeExecutionDurationResolver, NodeExecutionDurationResolver>();
		services.AddSingleton<INodeExecutionStateClassifier, NodeExecutionStateClassifier>();
		services.AddSingleton<IRunnableNodesFinder, RunnableNodesFinder>();
		services.AddSingleton<IDataFlowExecutionStatusDtoProvider, DataFlowExecutionStatusDtoProvider>();
		services.AddSingleton<IDataFlowExecutionStatusStore, InMemoryDataFlowExecutionStatusStore>();
		services.AddSingleton(TimeProvider.System);
	}
}
