using CoffeeMachine.Api.Controllers;
using CoffeeMachine.Api.Dtos;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CoffeeMachine.UnitTest;

public class CoffeeMachineControllerUnitTests
{
    private static CoffeeMachineController CreateController(string ip, DateTime? now = null)
    {
        var dateProviderMock = new Mock<IDateTimeProvider>();
        dateProviderMock.Setup(d => d.Now).Returns(now ?? DateTime.Now);

        var controller = new CoffeeMachineController(dateProviderMock.Object);

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
        var controller1 = CreateController("192.168.1.10", new DateTime(2026, 2, 17));
        var controller2 = CreateController("192.168.1.20", new DateTime(2026, 2, 17));

        for (var i = 0; i < 4; i++)
            await controller1.Get();

        // Assert response body is empty for 503
        var result1 = await controller1.Get();
        controller1.Response.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
        result1.Should().BeOfType<EmptyResult>();

        // Assert response body contains expected message for OK
        var result2 = await controller2.Get();
        result2.Should().BeOfType<OkObjectResult>();
        var okBody = ((OkObjectResult)result2).Value as CoffeeResponse;
        okBody.Should().NotBeNull();
        okBody!.Message.Should().Be("Your piping hot coffee is ready");
    }

    [Fact]
    public async Task Should_Return_418_On_April_First()
    {
        // Mock date to April 1st
        var aprilFirst = new DateTime(2024, 4, 1);
        var controller = CreateController("192.168.1.50", aprilFirst);

        var result = await controller.Get();
        result.Should().BeOfType<EmptyResult>();
        controller.Response.StatusCode.Should().Be(418);
    }
}
