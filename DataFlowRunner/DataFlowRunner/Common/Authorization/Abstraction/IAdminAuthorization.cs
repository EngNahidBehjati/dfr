namespace DataFlowRunner.Common.Authorization.Abstraction;

public interface IAdminAuthorization
{
	Task<bool> UserIsAdmin();
}
