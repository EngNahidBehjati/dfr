using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using DataFlowRunner.DataFlowExecution.Abstractions;
using DataFlowRunner.DataFlowExecution.Models;

namespace DataFlowRunner.DataFlowDynamicParameters.Business.ProvideRequiredDynamicParameters;

internal sealed class VersionSynchronizingRequiredDynamicParametersProvider : IRequiredDynamicParametersProvider
{
	private readonly IRequiredDynamicParametersProvider _inner;
	private readonly IDataFlowVersionConflictHandler _versionConflictHandler;

	public VersionSynchronizingRequiredDynamicParametersProvider(
		IRequiredDynamicParametersProvider inner,
		IDataFlowVersionConflictHandler versionConflictHandler)
	{
		_inner = inner ?? throw new ArgumentNullException(nameof(inner));
		_versionConflictHandler = versionConflictHandler ?? throw new ArgumentNullException(nameof(versionConflictHandler));
	}

	public async Task<List<DynamicParameterDto>> ProvideDataFlowDynamicParametersAsync(
		Guid dataFlowExecutionId,
		long dataFlowVersion,
		CancellationToken cancellationToken)

	{
		await _versionConflictHandler.HandleVersionConflictAsync(
			new DataFlowExecutionId(dataFlowExecutionId),
			new DataFlowVersion(dataFlowVersion),
			cancellationToken).ConfigureAwait(false);

		return await _inner.ProvideDataFlowDynamicParametersAsync(dataFlowExecutionId,
			dataFlowVersion, cancellationToken).ConfigureAwait(false);
	}

	/// <summary>
	/// Temporary decorator that synchronizes the execution context version before providing
	/// node-specific dynamic parameters.
	/// </summary>
	public async Task<List<DynamicParameterDto>> ProvideDataFlowDynamicParametersForRunningNodeAsync(
		Guid dataFlowExecutionId,
		Guid nodeId,
		long dataFlowVersion,
		CancellationToken cancellationToken)
	{
		await _versionConflictHandler.HandleVersionConflictAsync(
			new DataFlowExecutionId(dataFlowExecutionId),
			new DataFlowVersion(dataFlowVersion),
			cancellationToken).ConfigureAwait(false);

		return await _inner.ProvideDataFlowDynamicParametersForRunningNodeAsync(
			dataFlowExecutionId, nodeId, dataFlowVersion, cancellationToken).ConfigureAwait(false);
	}
}
