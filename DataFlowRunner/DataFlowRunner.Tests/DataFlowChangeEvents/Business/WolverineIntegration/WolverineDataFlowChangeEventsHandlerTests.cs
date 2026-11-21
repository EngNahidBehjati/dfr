using DataFlowRunner.DataFlowChangeEvents.Abstractions;
using DataFlowRunner.DataFlowChangeEvents.Business.WolverineIntegration;
using DataFlowRunner.DataFlowChangeEvents.Events;
using FluentAssertions;
using NSubstitute;
using Wolverine;

namespace DataFlowRunner.Tests.DataFlowChangeEvents.Business.WolverineIntegration;

public sealed class WolverineDataFlowChangeEventsHandlerTests
{
	private readonly IDataFlowChangeEventsFacade _facade;
	private readonly WolverineDataFlowChangeEventsHandler _sut;

	public WolverineDataFlowChangeEventsHandlerTests()
	{
		_facade = Substitute.For<IDataFlowChangeEventsFacade>();
		_sut = new WolverineDataFlowChangeEventsHandler(_facade);
	}

	[Fact]
	public async Task HandleAsync_ShouldDelegateToFacade_WhenCalled()
	{
		// Arrange
		var changeEvent = new ChangeEvent(
			SchemaVersion: 1,
			DataFlowVersion: 10,
			Actions: Array.Empty<System.Text.Json.JsonElement>());
		var envelope = new Envelope();

		// Act
		await _sut.HandleAsync(changeEvent, envelope, CancellationToken.None);

		// Assert
		await _facade.Received(1).HandleAsync(changeEvent, envelope, CancellationToken.None);
	}

	[Fact]
	public void Constructor_ShouldThrowArgumentNullException_WhenFacadeIsNull()
	{
		// Arrange
		IDataFlowChangeEventsFacade facade = null!;

		// Act
		Action act = () => _ = new WolverineDataFlowChangeEventsHandler(facade);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}
}
