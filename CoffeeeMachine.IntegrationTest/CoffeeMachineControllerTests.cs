using CoffeeMachine.Api.Controllers;
using CoffeeMachine.Api.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;

namespace CoffeeMachine.IntegrationTest;

public class CoffeeMachineControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CoffeeMachineControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task First_Call_Should_Return_200_With_Valid_Json()
    {
        // Mock date to a normal day (not April 1st)
        var client = CreateClientWithMockDate(new DateTime(2026, 2, 17));

        var response = await client.GetAsync("/brew-coffee");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<CoffeeResponse>();
        body!.Message.Should().Be("Your piping hot coffee is ready");
        body.Prepared.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{4}");
    }

    [Fact]
    public async Task Fifth_Call_Should_Return_503_EmptyBody()
    {
        var client = CreateClientWithMockDate(new DateTime(2026, 2, 17));

        HttpResponseMessage? response = null;

        // Clean up any previous calls to ensure we are testing the 5th call logic correctly
        for (var i = 0; i < 5; i++)
        {
            response = await client.GetAsync("/brew-coffee");
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                break;
        }

        for (var i = 0; i < 5; i++)
        {
            response = await client.GetAsync("/brew-coffee");
        }

        response!.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task Sixth_Call_Should_Return_200_Again()
    {
        var client = CreateClientWithMockDate(new DateTime(2026, 2, 17));

        for (var i = 0; i < 5; i++)
            await client.GetAsync("/brew-coffee");

        var response = await client.GetAsync("/brew-coffee");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Should_Return_418_On_April_First()
    {
        var client = CreateClientWithMockDate(new DateTime(2026, 4, 1));
        var response = await client.GetAsync("/brew-coffee");
        response.StatusCode.Should().Be((HttpStatusCode)418);
        (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
    }

    private HttpClient CreateClientWithMockDate(DateTime mockDate)
    {
        var mockDateProvider = new Mock<IDateTimeProvider>();
        mockDateProvider.Setup(d => d.Now).Returns(mockDate);

        return _factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDateTimeProvider));
                if (descriptor != null) services.Remove(descriptor);

                services.AddSingleton(mockDateProvider.Object);
            })).CreateClient();
    }
}
