using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using DotNet.Testcontainers.Networks;

namespace TestContainerDemo.Tests
{
    internal class ApiContainer 
    {
        private const int Port = 80;
        private readonly IFutureDockerImage _image;
        private readonly IContainer _container;

        public ApiContainer(INetwork network, ApiContainerOptions options)
        {
            // Create a future image that will be built from the api docker file. 
            _image = new ImageFromDockerfileBuilder()
                 .WithName(Guid.NewGuid().ToString("D"))
                 .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath)
                 .WithDockerfile("TestContainerDemo.API/Dockerfile")
                 .Build();

            _container = new ContainerBuilder()
                .WithImage(_image) // Rather then build the image every time we could build it in an external process and use the image name instead
                .WithPortBinding(Port, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(Port))
                .WithEnvironment("ConnectionStrings__SqlConnectionString", options.SqlConnectionString)
                .WithNetwork(network)
                .Build();
        }

        /// <summary>
        /// Get a http client configured for the containers generated public port. 
        /// </summary>
        /// <returns>Rturns a http client configured with a base address.</returns>
        public HttpClient GetClient() => new HttpClient { BaseAddress = new Uri($"http://{_container.Hostname}:{_container.GetMappedPublicPort(Port)}") };

        public async Task StartAsync()
        {
            // Create the image before we can use it to start the container
            await _image.CreateAsync().ConfigureAwait(false);
            await _container.StartAsync().ConfigureAwait(false);
        }
        
        public async Task StopAsync() => await _container.StopAsync().ConfigureAwait(false); 
    }
}
