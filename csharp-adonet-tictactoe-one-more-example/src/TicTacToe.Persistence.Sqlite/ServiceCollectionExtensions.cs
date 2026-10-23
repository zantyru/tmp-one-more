using Microsoft.Extensions.DependencyInjection;
using TicTacToe.Persistence.Abstractions;

namespace TicTacToe.Persistence.Sqlite;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует конкретный адаптер СУБД. Чтобы перейти на другую СУБД,
    /// достаточно заменить этот вызов на регистрацию другого адаптера.
    /// </summary>
    public static IServiceCollection AddSqlitePersistence(this IServiceCollection services, string databasePath)
    {
        services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(databasePath));
        services.AddSingleton<IGameSessionRepository, SqliteGameSessionRepository>();
        services.AddSingleton<SqliteSchemaInitializer>();

        return services;
    }
}
