using CoffeeMachine.Application.Interfaces;

namespace CoffeeMachine.Infrastructure.Models;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
