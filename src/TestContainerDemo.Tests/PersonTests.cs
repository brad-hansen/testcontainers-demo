using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using TestContainerDemo.Common;

namespace TestContainerDemo.Tests
{
    public class PersonTests : IClassFixture<ApplicationFixture>
    {
        private readonly ApplicationFixture _fixture;
        private readonly HttpClient _httpClient;

        public PersonTests(ApplicationFixture fixture)
        {
            _fixture = fixture;
            _httpClient = _fixture.GetClient();
        }


        [Fact]
        public async Task ShouldCreatePersonWhenRequestIsValid()
        {
            await _fixture.ResetAsync();

            var people = await GetPeople(_httpClient);
            Assert.Empty(people);

            var request = new CreatePersonRequest
            {
                FirstName = "Frank",
                LastName = "Tank",
                Occupation = "Programmer"
            };

            var response = await _httpClient.PostAsJsonAsync("/person", request);
            Assert.True(response.IsSuccessStatusCode);

            var location = response.Headers.Location;
            Assert.NotNull(location);

            var person = await GetPerson(_httpClient, location.LocalPath);

            Assert.Equal(request.FirstName, person.FirstName);
            Assert.Equal(request.LastName, person.LastName);
            Assert.Equal(request.Occupation, person.Occupation);
        }

        [Fact]
        public async Task ShouldFailWhenFirstNameMissing()
        {
            await _fixture.ResetAsync();

            var people = await GetPeople(_httpClient);
            Assert.Empty(people);

            var request = new CreatePersonRequest
            {
                LastName = "Tank",
                Occupation = "Programmer"
            };

            var response = await _httpClient.PostAsJsonAsync("/person", request);

            Assert.False(response.IsSuccessStatusCode);

            var content = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(content);
            Assert.Equal(400, content.Status);
        }

        [Fact]
        public async Task ShouldFailWhenLastNameMissing()
        {
            await _fixture.ResetAsync();

            var people = await GetPeople(_httpClient);
            Assert.Empty(people);

            var request = new CreatePersonRequest
            {
                FirstName = "Frank",
                Occupation = "Programmer"
            };

            var response = await _httpClient.PostAsJsonAsync("/person", request);

            Assert.False(response.IsSuccessStatusCode);

            var content = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(content);
            Assert.Equal(400, content.Status);
        }

        [Fact]
        public async Task ShouldFailWhenOccupationMissing()
        {
            await _fixture.ResetAsync();

            var people = await GetPeople(_httpClient);
            Assert.Empty(people);

            var request = new CreatePersonRequest
            {
                FirstName = "Frank",
                LastName = "Tank",
            };

            var response = await _httpClient.PostAsJsonAsync("/person", request);

            Assert.False(response.IsSuccessStatusCode);

            var content = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(content);
            Assert.Equal(400, content.Status);
        }

        private static async Task<IEnumerable<PersonResponse>> GetPeople(HttpClient client)
        {
            var response = await client.GetAsync("/person");
            response.EnsureSuccessStatusCode();

            var people = await response.Content.ReadFromJsonAsync<IEnumerable<PersonResponse>>();

            return people ?? throw new Exception("Invalid response");
        }

        private static async Task<PersonResponse> GetPerson(HttpClient client, string requestUri)
        {
            var response = await client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var person = await response.Content.ReadFromJsonAsync<PersonResponse>();

            return person ?? throw new Exception("Invalid response");
        }
    }
}