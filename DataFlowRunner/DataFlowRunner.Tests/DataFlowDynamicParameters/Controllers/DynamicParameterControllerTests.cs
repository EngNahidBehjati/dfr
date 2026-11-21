using DataFlowRunner.DataFlowDynamicParameters.Abstractions;
using DataFlowRunner.DataFlowDynamicParameters.Abstractions.ProvideRequiredDynamicParameters;
using DataFlowRunner.DataFlowDynamicParameters.Controllers;
using DataFlowRunner.DataFlowDynamicParameters.Models;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowDynamicParameters.Controllers;

public class DynamicParameterControllerTests
{
	private readonly IRequiredDynamicParametersProvider _requiredDynamicParametersProvider;
	private readonly DynamicParameterController _sut;

	public DynamicParameterControllerTests()
	{
		_requiredDynamicParametersProvider = Substitute.For<IRequiredDynamicParametersProvider>();
		_sut = new DynamicParameterController(_requiredDynamicParametersProvider);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenDynamicParametersProviderIsNull()
	{
		// Arrange
		IRequiredDynamicParametersProvider requiredDynamicParametersProvider = null!;

		// Act
		Action act = () => _ = new DynamicParameterController(requiredDynamicParametersProvider);

		// Assert
		act.Should().Throw<ArgumentNullException>().WithParameterName("requiredDynamicParametersProvider");
	}

	[Fact]
	public async Task GetDataFlowDynamicParameters_ShouldCallProviderAndReturnParameters_WhenProviderReturnsParameters()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		const long dataFlowVersion = 10;
		CancellationToken cancellationToken = CancellationToken.None;

		List<DynamicParameterDto> expected =
		[
			new()
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Parameter1",
				Required = true,
				DataType = "TEXT",
				IsList = false,
				DefaultValue = "value1"
			},
			new()
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Parameter2",
				Required = true,
				DataType = "INTEGER",
				IsList = false,
				DefaultValue = 42
			}
		];

		_requiredDynamicParametersProvider.ProvideDataFlowDynamicParametersAsync(
				dataFlowExecutionId,
				dataFlowVersion,
				cancellationToken)
			.Returns(expected);

		// Act
		List<DynamicParameterDto> actual =
			await _sut.GetDataFlowDynamicParameters(dataFlowExecutionId, dataFlowVersion, cancellationToken);

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public async Task GetDataFlowNodeExecutionDynamicParameters_ShouldCallProviderAndReturnParameters_WhenProviderReturnsParameters()
	{
		// Arrange
		var dataFlowExecutionId = Guid.NewGuid();
		var nodeId = Guid.NewGuid();
		const long dataFlowVersion = 10;
		CancellationToken cancellationToken = CancellationToken.None;

		List<DynamicParameterDto> expected =
		[
			new()
			{
				Id = Guid.NewGuid().ToString(),
				Name = "UpstreamParameter1",
				Required = true,
				DataType = "TEXT",
				IsList = false,
				DefaultValue = "value1"
			}
		];

		_requiredDynamicParametersProvider.ProvideDataFlowDynamicParametersForRunningNodeAsync(
				dataFlowExecutionId,
				nodeId,
				dataFlowVersion,
				cancellationToken)
			.Returns(expected);

		// Act
		List<DynamicParameterDto> actual =
			await _sut.GetDataFlowNodeExecutionDynamicParameters(
				dataFlowExecutionId,
				nodeId,
				dataFlowVersion,
				cancellationToken);

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}
