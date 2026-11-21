using DataFlowRunner.DataFlowChangeEvents.Business.WolverineIntegration;
using FluentAssertions;
using Wolverine;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.Kafka;

public sealed class KafkaDataFlowIdProviderTests
{
	private readonly KafkaDataFlowExecutionIdProvider _sut = new();

	[Fact]
	public void TryGetDataFlowExecutionId_ShouldReturnTrueAndParsedId_WhenPartitionKeyIsValidGuid()
	{
		// Arrange
		var expected = Guid.NewGuid();
		var envelope = new Envelope { PartitionKey = expected.ToString() };

		// Act
		var executionIdFound = _sut.TryGetDataFlowExecutionId(envelope, out Guid id);

		// Assert
		executionIdFound.Should().BeTrue();
		id.Should().Be(expected);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	[InlineData("not-a-guid")]
	public void TryGetDataFlowExecutionId_ShouldReturnFalseAndEmptyId_WhenPartitionKeyIsMissingOrUnparsable(string? partitionKey)
	{
		// Arrange
		var envelope = new Envelope { PartitionKey = partitionKey };

		// Act
		var executionIdFound = _sut.TryGetDataFlowExecutionId(envelope, out Guid id);

		// Assert
		executionIdFound.Should().BeFalse();
		id.Should().BeEmpty();
	}
}
