using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.Language.LanguageDictionary;

namespace DataFlowRunner.Common.Authentication.Business;

internal sealed class AuthenticationTokenAccessor : IAuthenticationTokenAccessor
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly ILanguageDictionaryProvider _languageDictionaryProvider;

	public AuthenticationTokenAccessor(IHttpContextAccessor httpContextAccessor,
		ILanguageDictionaryProvider languageDictionaryProvider)
	{
		_httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
		_languageDictionaryProvider = languageDictionaryProvider ?? throw new ArgumentNullException(nameof(languageDictionaryProvider));
	}

	public AuthenticationCookie GetAuthenticationCookie()
	{
		return new AuthenticationCookie("e1185264-5717-4565-b3fc-2c963f36af12");
	}
}
