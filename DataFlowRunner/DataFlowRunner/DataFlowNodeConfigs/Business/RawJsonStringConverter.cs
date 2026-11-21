using System.Text.Json;
using System.Text.Json.Serialization;

namespace DataFlowRunner.DataFlowNodeConfigs.Business;

public class RawJsonStringConverter : JsonConverter<string>
{
	public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		using var doc = JsonDocument.ParseValue(ref reader);
		return doc.RootElement.GetRawText();
	}

	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
	{
		throw new NotSupportedException("serializing is not supported");
	}
}

