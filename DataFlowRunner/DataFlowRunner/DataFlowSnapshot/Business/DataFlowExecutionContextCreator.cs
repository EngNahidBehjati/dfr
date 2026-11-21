using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowExecutionEvents.ExecutionCommands;
using DataFlowRunner.DataFlowSnapshot.Abstractions;
using DataFlowRunner.DataFlowSnapshot.Models;
using DataFlowRunner.GraphCore.Abstractions;

namespace DataFlowRunner.DataFlowSnapshot.Business;

internal sealed class DataFlowExecutionContextCreator : IDataFlowExecutionContextCreator
{
	private readonly IDataFlowTopologyClientApi _dataFlowTopologyClientApi;
	private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;
	private readonly ITopologyMapper _mapper;

	public DataFlowExecutionContextCreator(IDataFlowTopologyClientApi clientApi, ITopologyMapper mapper,
		IAuthenticationTokenAccessor tokenAccessor)
	{
		_dataFlowTopologyClientApi = clientApi ?? throw new ArgumentNullException(nameof(clientApi));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		_authenticationTokenAccessor = tokenAccessor ?? throw new ArgumentNullException(nameof(tokenAccessor));
	}

	public async Task<DataFlowExecutionContext> CreateDataFlowExecutionContextAsync(Guid requestDataFlowId,
		CancellationToken cancellationToken,
		long? version = null)
	{
		DataFlowTopologyDto dataFlowTopology = await _dataFlowTopologyClientApi.GetDataFlowTopologyAsync(requestDataFlowId,
				_authenticationTokenAccessor.GetAuthenticationCookie(),
				version,
				cancellationToken)
			.ConfigureAwait(false);

		IGraph graph = _mapper.MapToGraph(requestDataFlowId, dataFlowTopology);
		ProcessorInfoDto processorInfoDto = dataFlowTopology.Topology.ProcessorInfo;
		ExecutionSettingsDto executionSettingsDto = dataFlowTopology.Topology.ExecutionSettings;
		return new DataFlowExecutionContext
		{
			Graph = graph,
			DataFlowVersion = new DataFlowVersion(dataFlowTopology.DataFlowVersion),
			DataFlowExecutionId = new DataFlowExecutionId(requestDataFlowId),
			ProcessorInfo = new ProcessorInfo
			{
				ProcessorType = processorInfoDto.ProcessorType,
				ProcessorId = processorInfoDto.ProcessorId
			},
			ExecutionSettings = new ExecutionSettings
			{
				ExecutorType = executionSettingsDto.ExecutorType,
				ExecutorId = executionSettingsDto.ExecutorId
			}
		};
	}
}
