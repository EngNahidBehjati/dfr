
namespace DataFlowRunner.Integration.Tests.Common.Constants;

internal static class AccessManagementTestConstants
{
	public static readonly string AuthTokenValue = Guid.NewGuid().ToString();
	public static readonly string AuthCookie = $"Auth-Token={AuthTokenValue}";
	public static readonly string TestRoleId = Guid.NewGuid().ToString();
	public static readonly string TestAuthenticationUserId = Guid.NewGuid().ToString();
	public const string TestUserName = "username";
	public const int TestUserId = 1;
}
