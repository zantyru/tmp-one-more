using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Gameplay;

namespace TicTacToe.Application;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует сервисный слой. Composition root остаётся в UI-проекте:
    /// он лишь вызывает этот метод и метод адаптера СУБД.
    /// </summary>
    public static IServiceCollection AddTicTacToeApplication(this IServiceCollection services)
    {
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<IComputerStrategy>(_ => new SimpleComputerStrategy());
        services.AddSingleton<IGameSessionService, GameSessionService>();

        return services;
    }
}
