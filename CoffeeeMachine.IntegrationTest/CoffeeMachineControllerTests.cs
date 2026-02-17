using CoffeeMachine.Api.Dtos;
using CoffeeMachine.Application.Interfaces;
using Controllers;
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
    public async Task Cold_Weather_And_Not_April_First_Should_Return_200_With_Hot_Coffee_Message()
    {
        var client = CreateMockClient(20.0, new DateTime(2026, 2, 17));
        var response = await client.GetAsync("/api/coffeemachine/brew-coffee");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CoffeeResponse>();
        body!.Message.Should().Be("Your piping hot coffee is ready");
        body.Prepared.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{4}");
    }

    [Fact]
    public async Task Hot_Temperature_Should_Return_Iced_Coffee_Message()
    {
        var client = CreateMockClient(35.0, new DateTime(2026, 2, 17));
        var response = await client.GetAsync("/api/coffeemachine/brew-coffee");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<CoffeeResponse>();
        body.Should().NotBeNull();
        body!.Message.Should().Be("Your refreshing iced coffee is ready");
        body.Prepared.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{4}");
    }

    [Fact]
    public async Task Fifth_Call_Should_Return_503_EmptyBody()
    {
        // No effect on the 5th call
        var client = CreateMockClient(35.0, new DateTime(2026, 2, 17));

        HttpResponseMessage? response = null;

        // Clean up any previous calls to ensure we are testing the 5th call logic correctly
        for (var i = 0; i < 5; i++)
        {
            response = await client.GetAsync("/api/coffeemachine/brew-coffee");
            if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                break;
        }

        for (var i = 0; i < 5; i++)
        {
            response = await client.GetAsync("/api/coffeemachine/brew-coffee");
        }

        response!.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }

    [Fact]
    public async Task Sixth_Call_Should_Return_200_Again()
    {
        var client = CreateMockClient(20.0, new DateTime(2026, 2, 17));

        for (var i = 0; i < 5; i++)
            await client.GetAsync("/api/coffeemachine/brew-coffee");

        var response = await client.GetAsync("/api/coffeemachine/brew-coffee");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CoffeeResponse>();
        body!.Message.Should().Be("Your piping hot coffee is ready");
        body.Prepared.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[+-]\d{4}");
    }

    [Fact]
    public async Task Should_Return_418_On_April_First()
    {
        var client = CreateMockClient(20.0, new DateTime(2026, 4, 1));
        var response = await client.GetAsync("/api/coffeemachine/brew-coffee");
        response.StatusCode.Should().Be((HttpStatusCode)418);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().BeEmpty();
    }

    private HttpClient CreateMockClient(double temperature, DateTime mockDate)
    {
        var weatherMock = new Mock<IWeatherClient>();
        weatherMock.Setup(c => c.GetCurrentTemperatureAsync(It.IsAny<string>()))
                   .ReturnsAsync(temperature);

        var mockDateProvider = new Mock<IDateTimeProvider>();
        mockDateProvider.Setup(d => d.Now).Returns(mockDate);

        return _factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            {
                // Remove existing IWeatherClient registration if any
                var weatherDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IWeatherClient));
                if (weatherDescriptor != null) services.Remove(weatherDescriptor);
                services.AddSingleton(weatherMock.Object);

                // Remove existing IDateTimeProvider registration if any
                var datetimeDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IDateTimeProvider));
                if (datetimeDescriptor != null) services.Remove(datetimeDescriptor);
                services.AddSingleton(mockDateProvider.Object);
            })).CreateClient();
    }
}
