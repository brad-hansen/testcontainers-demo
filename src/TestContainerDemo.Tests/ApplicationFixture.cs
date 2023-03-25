using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Respawn;
using Testcontainers.SqlEdge;

namespace TestContainerDemo.Tests
{
    /// <summary>
    /// Test fixture that creates a clean database and api instance.
    /// </summary>
    public sealed class ApplicationFixture : IAsyncLifetime
    {
        private const string SqlNetworkAlias = "sqlserver";
        private readonly string _localConnectionString = $"Server={SqlNetworkAlias}; Database={SqlEdgeBuilder.DefaultDatabase}; User Id={SqlEdgeBuilder.DefaultUsername};Password={SqlEdgeBuilder.DefaultPassword};";
        private readonly SqlEdgeContainer _sqlContainer;
        private readonly ApiContainer _apiContainer;
        private readonly INetwork _network;
        private Respawner? _respawner;

        public ApplicationFixture()
        {
            // We explicitly create a network to isolate the network aliases for cross container communication.
            _network = new NetworkBuilder().Build();

            // Use the offical sql edge module to build a database container.
            _sqlContainer = new SqlEdgeBuilder()
                .WithNetwork(_network)
                .WithNetworkAliases(SqlNetworkAlias)
                .Build();

            // Cross container communications uses the standard container ports and the configured network aliases so we can build
            // the connection string before the sql container is started.
            var apiOptions = new ApiContainerOptions
            {
                SqlConnectionString = _localConnectionString
            };

            _apiContainer = new ApiContainer(_network, apiOptions);

        }

        public HttpClient GetClient() => _apiContainer.GetClient();


        /// <summary>
        /// Use respawner to clear the database between tests.
        /// </summary>
        /// <returns></returns>
        public async Task ResetAsync()
        {
            if (_respawner is not null)
            {
                await _respawner.ResetAsync(_sqlContainer.GetConnectionString());
            }
        }

        public async Task DisposeAsync()
        {
            await _apiContainer.StopAsync().ConfigureAwait(false);
            await _sqlContainer.StopAsync().ConfigureAwait(false);
            await _network.DeleteAsync().ConfigureAwait(false);
        }

        public async Task InitializeAsync()
        {
            await _network.CreateAsync().ConfigureAwait(false);
            await _sqlContainer.StartAsync().ConfigureAwait(false);
            await _apiContainer.StartAsync().ConfigureAwait(false);

            // We have to wait for the sql container to start before we can access 'localhost' connection string.
            _respawner = await Respawner.CreateAsync(_sqlContainer.GetConnectionString());
        }
    }
}
