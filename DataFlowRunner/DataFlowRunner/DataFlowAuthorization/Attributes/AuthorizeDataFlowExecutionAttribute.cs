using DataFlowRunner.DataFlowAuthorization.Business;
using Microsoft.AspNetCore.Mvc;

namespace DataFlowRunner.DataFlowAuthorization.Attributes;

/// <summary>
/// Attribute that marks an endpoint as requiring data flow execution authorization.
/// When applied, the <see cref="DataFlowExecutionUserAuthorizationFilter" /> performs authorization
/// before allowing access to the endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthorizeDataFlowExecutionAttribute : TypeFilterAttribute
{
	public AuthorizeDataFlowExecutionAttribute()
		: base(typeof(DataFlowExecutionUserAuthorizationFilter))
	{
	}
}
