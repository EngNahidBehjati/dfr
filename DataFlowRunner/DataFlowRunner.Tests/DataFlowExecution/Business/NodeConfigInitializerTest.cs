using DataFlowRunner.Common.Authentication.Abstractions;
using DataFlowRunner.Common.Authentication.Models;
using DataFlowRunner.DataFlowExecution.Business;
using DataFlowRunner.DataFlowExecution.Models;
using DataFlowRunner.DataFlowNodeConfigs.Abstractions;
using DataFlowRunner.DataFlowNodeConfigs.Models;
using DataFlowRunner.GraphCore.Models.NodeModels;
using FluentAssertions;
using NSubstitute;

namespace DataFlowRunner.Tests.DataFlowExecution.Business
{
	public class NodeConfigInitializerTests
	{
		private readonly IDataFlowNodeConfigsStore _dataFlowNodeConfigsStore;
		private readonly INodeConfigRetrieveClientApi _nodeConfigRetrieveClientApi;
		private readonly IAuthenticationTokenAccessor _authenticationTokenAccessor;
		private readonly NodeConfigInitializer _sut;

		public NodeConfigInitializerTests()
		{
			_dataFlowNodeConfigsStore = Substitute.For<IDataFlowNodeConfigsStore>();
			_nodeConfigRetrieveClientApi = Substitute.For<INodeConfigRetrieveClientApi>();
			_authenticationTokenAccessor = Substitute.For<IAuthenticationTokenAccessor>();
			_sut = new NodeConfigInitializer
				(_dataFlowNodeConfigsStore, _nodeConfigRetrieveClientApi, _authenticationTokenAccessor);
		}

		[Fact]
		public void Constructor_ShouldThrowArgumentNullException_WhenDataFlowNodeConfigsStoreIsNull()
		{
			// Arrange
			IDataFlowNodeConfigsStore store = null!;

			// Act
			Action act = () =>
			{
				_ = new NodeConfigInitializer(store, _nodeConfigRetrieveClientApi, _authenticationTokenAccessor);
			};

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParameterName("dataFlowNodeConfigsStore");
		}

		[Fact]
		public void Constructor_ShouldThrowArgumentNullException_WhenNodeConfigRetrieveClientApiIsNull()
		{
			// Arrange
			INodeConfigRetrieveClientApi api = null!;

			// Act
			Action act = () =>
			{
				_ = new NodeConfigInitializer(_dataFlowNodeConfigsStore, api, _authenticationTokenAccessor);
			};

			// Assert
			act.Should().Throw<ArgumentNullException>().WithParameterName("nodeConfigRetrieveClientApi");
		}

		[Fact]
		public void Constructor_ShouldThrowArgumentNullException_WhenAuthenticationTokenAccessorIsNull()
		{
			// Arrange
			IAuthenticationTokenAccessor tokenAccessor = null!;

			// Act
			Action act = () =>
			{
				_ = new NodeConfigInitializer(_dataFlowNodeConfigsStore, _nodeConfigRetrieveClientApi,
					tokenAccessor);
			};

			// Assert
			act.Should().Throw<ArgumentNullException>()
				.WithParameterName("authenticationTokenAccessor");
		}


		[Fact]
		public async Task EnsureNodeConfigsLoadedAsync_ShouldRetrieveAndStoreConfigs_Whenever()
		{
			// Arrange
			var dataFlowExecutionId = new DataFlowExecutionId(Guid.NewGuid());
			var node1 = new Node { Id = new NodeId(Guid.NewGuid()) };
			var node2 = new Node { Id = new NodeId(Guid.NewGuid()) };
			var nodes = new HashSet<Node> { node1, node2 };

			var response = new ResponseDataFlowNodeConfigsDto([
				new ResponseNodeConfigDto(node1.Id.Value, "1", "config1"),
				new ResponseNodeConfigDto(node2.Id.Value, "2", "config2")
			]);

			_nodeConfigRetrieveClientApi
				.RetrieveNodeConfigsAsync(
					dataFlowExecutionId.Value,
					Arg.Any<AuthenticationCookie>(),
					Arg.Is<RequestDateFlowNodeConfigsDto>(dto =>
						dto.RequestNodeConfigs.Count == 2 &&
						dto.RequestNodeConfigs.Any(r =>
							r.NodeId == node1.Id.Value && r.Version == null) &&
						dto.RequestNodeConfigs.Any(r =>
							r.NodeId == node2.Id.Value && r.Version == null)),
					Arg.Any<CancellationToken>())
				.Returns(response);

			// Act
			await _sut.EnsureNodeConfigsLoadedAsync(dataFlowExecutionId, nodes, CancellationToken.None);

			// Assert
			_dataFlowNodeConfigsStore.Received(1).SetNodeConfig(dataFlowExecutionId, node1.Id,
				Arg.Is<NodeConfig>(n => n.Content == "config1" && n.Version == new NodeVersion("1")));
			_dataFlowNodeConfigsStore.Received(1).SetNodeConfig(dataFlowExecutionId, node2.Id,
				Arg.Is<NodeConfig>(n => n.Content == "config2" && n.Version == new NodeVersion("2")));

		}
	}
}
