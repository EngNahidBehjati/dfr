using DataFlowRunner.Common.DependencyInjection;
using DataFlowRunner.DataFlowChangeEvents.Business.Parsing;
using DataFlowRunner.DataFlowChangeEvents.Events;
using DataFlowRunner.DataFlowChangeEvents.Business.Handlers;
using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Business.ActionRouting;
using DataFlowRunner.DataFlowChangeEvents.Business.Facades;
using DataFlowRunner.DataFlowChangeEvents.Business.WolverineIntegration;

namespace DataFlowRunner.DataFlowChangeEvents.DependencyInjection;

internal sealed class DependencyInstaller : IDependencyInstaller
{
	public void Install(IServiceCollection services)
	{
		services.AddSingleton<IDataFlowChangeEventsFacade, DataFlowChangeEventsFacade>();
		services.AddSingleton<IActionTypeExtractor, ActionTypeExtractor>();
		services.AddSingleton<IDataFlowChangeActionRouter, DataFlowChangeActionRouter>();
		services.AddSingleton<IDataFlowExecutionIdProvider, KafkaDataFlowExecutionIdProvider>();

		services.AddSingleton<IDataFlowChangeEventHandler<AddNode>, AddNodeHandler>();
		services.AddSingleton<IDataFlowChangeEventHandler<SetNodeConfig>, SetNodeConfigHandler>();
		services.AddSingleton<IDataFlowChangeEventHandler<RemoveNode>, RemoveNodeHandler>();
		services.AddSingleton<IDataFlowChangeEventHandler<AddEdge>, AddEdgeHandler>();
		services.AddSingleton<IDataFlowChangeEventHandler<RemoveEdge>, RemoveEdgeHandler>();
		services.AddSingleton<IDataFlowChangeEventHandler<CreateDataFlowVariable>, CreateDataFlowVariableHandler>();
		services.AddSingleton<IDataFlowChangeEventHandler<UpdateDataFlowVariable>, UpdateDataFlowVariableHandler>();
		services.AddSingleton<IDataFlowChangeEventHandler<DeleteDataFlowVariable>, DeleteDataFlowVariableHandler>();
		services.AddSingleton<IDataFlowChangeEventHandler<ChangeProcessor>, ChangeProcessorHandler>();
		services.AddSingleton<IDataFlowChangeEventHandler<ChangeExecutionSettings>, ChangeExecutionSettingsHandler>();
	}
}
