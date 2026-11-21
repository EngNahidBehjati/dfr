namespace DataFlowRunner.Integration.Tests.DataFlowExecution.Models;

internal sealed class HttpResponseContainer
{
	private HttpResponseMessage? _response;
	public HttpResponseMessage Response => _response ?? throw new InvalidOperationException("HTTP response has not set.");
	public string ResponseBody = string.Empty;

	public void SetProperties(HttpResponseMessage response, string body)
	{
		_response = response ?? throw new ArgumentNullException(nameof(response));
		ResponseBody = body;
	}
}
