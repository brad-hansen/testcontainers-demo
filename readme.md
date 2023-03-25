
# Test Containers Demo

This repository contains a simple demo application using XUnit with testcontainers to verify an ASP.NET Core API. 

## What are Testcontainers?

The official documentation for [.NET Testcontainers](https://dotnet.testcontainers.org/)

From the docs:

> Testcontainers for .NET is a library to support tests with throwaway instances of Docker containers for all compatible .NET Standard versions. The library is built on top of the .NET Docker remote API and provides a lightweight implementation to support your test environment in all circumstances.

## Why Should I Care?

- Testcontainers make it trivial to use real databases and services to test your application code.
- When used with build pipelines you can spin up a container from the same image to be deployed and run integration tests against it.

## What Should I be Wary of?

- Tests that have lots of dependencies can use a lot of system resources on the test server (local or pipeline)
- In this demo application the api image is built every time the fixture is initialized, it is far more efficient to build and push the image then pull it  by name instead.
- If you have access to isolated external environments that the tests can run against it might make more sense to use those:
  - Doing a helm deploy into a isolated K8s cluster or namespace.
  - External environments that can reset between tests

## Getting Started

### The Api Container 

Testcontainers allow developers the ability to create modules that expose a clean builder syntax for common containers but for simple scenarios it is easier to create a simple [_builder_](./src/TestContainerDemo.Tests/ApiContainer.cs).

```cs
// Creating a new container from an image name
var container = new ContainerBuilder()
                .WithImage("testcontainerdemoapi:latest")
                .WithPortBinding(80, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(Port))
                .WithEnvironment("ConnectionStrings__SqlConnectionString", myConnectionString)
                .WithNetwork(network)
                .Build();

// Starting the container
await container.StartAsync();
```

### Test Fixtures

Test fixture provide reusable sets of containers that are also responsible for lifecycle management.

```cs
    public sealed class ApplicationFixture : IAsyncLifetime
    {
        private readonly SqlEdgeContainer _sqlContainer;
        private readonly ApiContainer _apiContainer;
        private readonly INetwork _network;

        public ApplicationFixture()
        {
            // Build a network to isolate our group of containers
            _network = new NetworkBuilder().Build();

            // Build the sql db container
            _sqlContainer = new SqlEdgeBuilder()
                .WithNetwork(_network)
                .WithNetworkAliases("sqlserver")
                .Build();

            var apiOptions = new ApiContainerOptions
            {
                SqlConnectionString = $"Server=sqlserver; Database={SqlEdgeBuilder.DefaultDatabase}; User Id={SqlEdgeBuilder.DefaultUsername};Password={SqlEdgeBuilder.DefaultPassword};"
            };

            // Build the container for our api project
            _apiContainer = new ApiContainer(_network, apiOptions);

        }

        public async Task DisposeAsync()
        {
            await _apiContainer.StopAsync();
            await _sqlContainer.StopAsync();
            await _network.DeleteAsync();
        }

        public async Task InitializeAsync()
        {
            await _network.CreateAsync();
            await _sqlContainer.StartAsync();
            await _apiContainer.StartAsync();
        }
    }
```

### Tests

```cs
public class PersonTests : IClassFixture<ApplicationFixture>
{
    private readonly ApplicationFixture _fixture;

    public PersonTests(ApplicationFixture fixture)
    {
        _fixture = fixture;
    }


[Fact]
public async Task ShouldCreatePersonWhenRequestIsValid()
{
    await _fixture.ResetAsync();

    var request = new CreatePersonRequest
    {
        FirstName = "Frank",
        LastName = "Tank",
        Occupation = "Programmer"
    };

    using var client = _fixture.GetClient();

    var response = await client.PostAsJsonAsync("/person", request);
    Assert.True(response.IsSuccessStatusCode);
}
```






