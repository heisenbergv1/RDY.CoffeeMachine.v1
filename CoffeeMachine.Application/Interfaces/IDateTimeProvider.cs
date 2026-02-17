namespace CoffeeMachine.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}