using System.Text.Json;
using DataFlowRunner.DataFlowChangeEvents.Business.Parsing;
using FluentAssertions;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Parsing;

public sealed class ActionTypeExtractorTests
{
	private readonly ActionTypeExtractor _sut = new();

	[Fact]
	public void TryGetActionType_ShouldReturnTrueAndType_WhenTypeIsPresent()
	{
		// Arrange
		JsonElement action = ParseElement("""{"type":"ADD_LINK_V1","link":{"id":"00000000-0000-0000-0000-000000000001"}}""");

		// Act
		var ok = _sut.TryGetActionType(action, out var type);

		// Assert
		ok.Should().BeTrue();
		type.Should().Be("ADD_LINK_V1");
	}

	[Theory]
	[InlineData("[]")]
	[InlineData("""{"x":1}""")]
	[InlineData("""{"type":123}""")]
	[InlineData("""{"type":"   "}""")]
	[InlineData("""{"type":""}""")]
	public void TryGetActionType_ShouldReturnFalseAndNoType_WhenActionLacksValidTypeProperty(string json)
	{
		// Arrange
		JsonElement action = ParseElement(json);

		// Act
		var ok = _sut.TryGetActionType(action, out var type);

		// Assert
		ok.Should().BeFalse();
		type.Should().BeNullOrWhiteSpace();
	}

	private static JsonElement ParseElement(string json)
	{
		using var doc = JsonDocument.Parse(json);
		return doc.RootElement.Clone();
	}
}
