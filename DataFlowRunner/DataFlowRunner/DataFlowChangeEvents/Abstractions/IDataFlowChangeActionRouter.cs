using System.Text.Json;
using DataFlowRunner.DataFlowChangeEvents.Models;

namespace DataFlowRunner.DataFlowChangeEvents.Abstractions;

internal interface IDataFlowChangeActionRouter
{
	Task RouteAsync(string actionType, JsonElement action, DataFlowChangeContext context, CancellationToken cancellationToken);
}
