namespace DataFlowRunner.Common.Authorization.Models;

internal sealed record DiaRegistrationConfigs
{
	public Guid TargetSystemId { get; set; }
	public Guid AdminServiceId { get; set; }
	public Guid ManageServiceAccessLevelId { get; set; }
}
