using CoffeeMachine.Api.Dtos;
using CoffeeMachine.Application.Interfaces;
using Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoffeeMachine.UnitTest;

public class CoffeeMachineControllerUnitTests
{
    private CoffeeMachineController CreateController(string ip, double? temp = null, DateTime? now = null)
    {
        // Mock the IMediator
        var mediatorMock = new Mock<MediatR.IMediator>();

        // Setup mediator to handle GetCoffeeCommand (or your actual command)
        mediatorMock
            .Setup(m => m.Send(It.IsAny<Application.Queries.GetCoffeeMessageQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(temp > 30 ? "Your refreshing iced coffee is ready" : "Your piping hot coffee is ready");

        var dateProviderMock = new Mock<IDateTimeProvider>();
        dateProviderMock.Setup(d => d.Now).Returns(now ?? DateTime.Now);

        var controller = new CoffeeMachineController(mediatorMock.Object, dateProviderMock.Object);

        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = System.Net.IPAddress.Parse(ip);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = context
        };

        return controller;
    }

    [Fact]
    public async Task Different_IPs_Should_Have_Independent_Counters()
    {
        if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
            return;

        var controller1 = CreateController("192.168.1.10", 20, new DateTime(2026, 2, 17));
        var controller2 = CreateController("192.168.1.20", 20, new DateTime(2026, 2, 17));

        for (var i = 0; i < 4; i++)
            await controller1.Get();

        var result1 = await controller1.Get();
        result1.Should().BeOfType<EmptyResult>();
        controller1.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);

        var result2 = await controller2.Get();
        result2.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Temperature_Above_30_Should_Return_IcedCoffee_Message()
    {
        var controller = CreateController("192.168.1.30", temp: 31, new DateTime(2026, 2, 17));

        var result = await controller.Get();
        result.Should().BeOfType<OkObjectResult>();

        var body = ((OkObjectResult)result).Value as CoffeeResponse;
        body.Should().NotBeNull();
        body.Message.Should().Be("Your refreshing iced coffee is ready");
    }

    [Fact]
    public async Task Temperature_Below_30_Should_Return_HotCoffee_Message()
    {
        var controller = CreateController("192.168.1.40", temp: 25, new DateTime(2026, 2, 17));

        var result = await controller.Get();
        result.Should().BeOfType<OkObjectResult>();

        var body = ((OkObjectResult)result).Value as CoffeeResponse;
        body.Should().NotBeNull();
        body.Message.Should().Be("Your piping hot coffee is ready");
    }

    [Fact]
    public async Task Should_Return_418_On_April_First()
    {
        // Mock date to April 1st
        var aprilFirst = new DateTime(2024, 4, 1);
        var controller = CreateController("192.168.1.50", temp: 25, aprilFirst);

        var result = await controller.Get();
        controller.Response.StatusCode.Should().Be(418);
        result.Should().BeOfType<EmptyResult>();
    }
}
