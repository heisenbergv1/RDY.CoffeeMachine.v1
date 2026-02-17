using CoffeeMachine.Api.Dtos;
using CoffeeMachine.Application.Interfaces;
using CoffeeMachine.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Controllers;

[ApiController]
public class CoffeeMachineController : ControllerBase
{
    // Track calls per IP address
    private static readonly ConcurrentDictionary<string, int> _ipCallCounts = new();
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMediator _mediator;

    public CoffeeMachineController(IMediator mediator, IDateTimeProvider dateTimeProvider)
    {
        _mediator = mediator;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <remarks>
    /// This controller tracks calls per client IP address to implement specific behaviors:
    /// - Every 5th request from the same IP returns 503 Service Unavailable (empty body)
    /// - On April 1st, all requests return 418 I'm a teapot (empty body)
    ///
    /// <para>
    /// Rationale for using per-IP counters:
    /// - Using a global counter shared across all clients is **bad practice in REST APIs** because
    ///   one client's requests would affect other clients' experience, breaking isolation.
    /// - REST APIs should be **stateless per client** where possible. Using IP-based counters
    ///   preserves isolation: each client experiences their own “5th request” independently.
    /// - While using IP is not perfect (e.g., NAT or proxies), it is sufficient for this simulation
    ///   and avoids unintended cross-client interference.
    /// </para>
    /// </remarks>
    [HttpGet("brew-coffee")]
    public async Task<IActionResult> Get()
    {
        try
        {
            var now = _dateTimeProvider.Now;

            // If April 1st, return 418 I'm a teapot with empty body
            if (now.Month == 4 && now.Day == 1)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status418ImATeapot;
                return new EmptyResult();
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var currentCall = _ipCallCounts.AddOrUpdate(
                ipAddress,
                1,
                (key, existingValue) => existingValue + 1);

            // Return 503 on the 5th call, then reset counter
            if (currentCall == 5)
            {
                _ipCallCounts[ipAddress] = 0; // reset counter for this IP
                HttpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                return new EmptyResult();
            }

            var message = await _mediator.Send(new GetCoffeeMessageQuery("Manila"));

            // Format as ISO-8601 without milliseconds and without colon in timezone offset
            var prepared = now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            prepared = prepared.Remove(prepared.Length - 3, 1);

            var response = new CoffeeResponse
            {
                Message = message,
                Prepared = prepared
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }
}