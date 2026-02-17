using CoffeeMachine.Application.Interfaces;
using MediatR;

namespace CoffeeMachine.Application.Queries;

/// <summary>
/// Query to get the coffee message (hot or iced) based on current weather.
/// </summary>
public class GetCoffeeMessageQuery : IRequest<string>
{
    public string City { get; }

    public GetCoffeeMessageQuery(string city)
    {
        City = city;
    }
}

public class GetCoffeeMessageHandler : IRequestHandler<GetCoffeeMessageQuery, string>
{
    private readonly IWeatherClient _weatherClient;

    public GetCoffeeMessageHandler(IWeatherClient weatherClient)
    {
        _weatherClient = weatherClient;
    }

    public async Task<string> Handle(GetCoffeeMessageQuery request, CancellationToken cancellationToken)
    {
        var message = "Your piping hot coffee is ready";

        var temp = await _weatherClient.GetCurrentTemperatureAsync(request.City);
        if (temp.HasValue && temp.Value > 30.0)
        {
            message = "Your refreshing iced coffee is ready";
        }

        return message;
    }
}
